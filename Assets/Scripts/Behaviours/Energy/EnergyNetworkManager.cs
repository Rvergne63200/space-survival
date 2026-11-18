using System.Collections.Generic;
using UnityEngine;

// Electrical networks: built from the devices (EnergyDevice) and their cables, updated every "interval" seconds.
// - A network is everything linked by cables (consumers, generators, storages): each device is in a single network.
//   Only the distribution panels separate networks: their input and their output are two devices without cable
//   between them.
// - Each update: the generators of the network give it what it needs, their surplus charges its storages (equal
//   shares, within their input rate), and the storages then give what is still missing (within their output rate).
//   The rest of the production is lost.
// - A network that still lacks power trips: its consumers stop. It is rearmed after "rearmDelay" seconds once its
//   generators and storages could cover its demand.
// - Distribution panels (DistributionPanel): their input is a consumer of the upstream network asking what their
//   circuit needs, their output a generator of the circuit served last (see Simulate).
// Created on first use, persists between the scenes
public class EnergyNetworkManager : MonoBehaviour
{
    public enum NetworkState { NotLinked, Powered, Tripped, SwitchedOff }

    private static EnergyNetworkManager instance;
    public static bool HasInstance => instance != null;

    public static EnergyNetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerObject = new GameObject("EnergyNetworkManager");
                DontDestroyOnLoad(managerObject);
                instance = managerObject.AddComponent<EnergyNetworkManager>();
            }

            return instance;
        }
    }

    [SerializeField] private float interval = 0.2f;
    [SerializeField] private float rearmDelay = 3f;

    // Below this lack (W), a network is considered fully powered (float errors)
    private const float Epsilon = 0.001f;

    private class Network
    {
        public readonly List<EnergyDevice> devices = new List<EnergyDevice>();
        public readonly List<EnergyConsumer> consumers = new List<EnergyConsumer>();
        public readonly List<EnergyGenerator> generators = new List<EnergyGenerator>();
        public readonly List<EnergyStorage> storages = new List<EnergyStorage>();

        public bool tripped;
        public float rearmTimer;

        // Current update
        public float demand;
        public float lack;
    }

    private readonly HashSet<EnergyDevice> devices = new HashSet<EnergyDevice>();
    private readonly List<Network> networks = new List<Network>();
    private readonly Dictionary<EnergyDevice, Network> deviceNetworks = new Dictionary<EnergyDevice, Network>();

    private bool dirty;
    private float timer;

    // Current update: what is left of each generator, and what each storage takes and gives
    private readonly Dictionary<EnergyGenerator, float> generatorLeft = new Dictionary<EnergyGenerator, float>();
    private readonly Dictionary<EnergyGenerator, float> generatorGiven = new Dictionary<EnergyGenerator, float>();
    private readonly Dictionary<EnergyGenerator, float> generatorCharged = new Dictionary<EnergyGenerator, float>();
    private readonly Dictionary<DistributionPanel, float> panelCharge = new Dictionary<DistributionPanel, float>();
    private readonly Dictionary<EnergyGenerator, float> generatorLeftBeforeCharge = new Dictionary<EnergyGenerator, float>();
    private readonly Dictionary<EnergyStorage, float> storageCharge = new Dictionary<EnergyStorage, float>();
    private readonly Dictionary<EnergyStorage, float> storageDischarge = new Dictionary<EnergyStorage, float>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void Register(EnergyDevice device)
    {
        if (devices.Add(device)) dirty = true;
    }

    public void Unregister(EnergyDevice device)
    {
        if (devices.Remove(device)) dirty = true;
    }

    // A cable was added or removed
    public void MarkDirty()
    {
        dirty = true;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < interval) return;

        if (dirty)
        {
            Rebuild();
        }

        Simulate(timer);
        timer = 0f;
    }

    // ---------- Networks ----------

    private void Rebuild()
    {
        dirty = false;

        // A new network stays tripped if one of its devices was in a tripped network
        HashSet<EnergyDevice> wasTripped = new HashSet<EnergyDevice>();

        foreach (Network network in networks)
        {
            if (!network.tripped) continue;

            foreach (EnergyDevice device in network.devices)
            {
                wasTripped.Add(device);
            }
        }

        networks.Clear();
        deviceNetworks.Clear();
        devices.RemoveWhere(device => device == null);

        // Everything linked by cables, whatever the kind of device
        foreach (EnergyDevice start in devices)
        {
            if (deviceNetworks.ContainsKey(start)) continue;

            Network network = new Network();
            Queue<EnergyDevice> queue = new Queue<EnergyDevice>();

            queue.Enqueue(start);
            deviceNetworks[start] = network;

            while (queue.Count > 0)
            {
                EnergyDevice device = queue.Dequeue();
                Add(network, device);
                network.tripped |= wasTripped.Contains(device);

                foreach (ElectricCable cable in device.Cables)
                {
                    EnergyDevice other = cable != null ? cable.Other(device) : null;

                    if (other == null || !devices.Contains(other) || deviceNetworks.ContainsKey(other)) continue;

                    deviceNetworks[other] = network;
                    queue.Enqueue(other);
                }
            }

            networks.Add(network);
        }
    }

    private static void Add(Network network, EnergyDevice device)
    {
        network.devices.Add(device);

        if (device is EnergyConsumer consumer)
        {
            network.consumers.Add(consumer);
        }
        else if (device is EnergyGenerator generator)
        {
            network.generators.Add(generator);
        }
        else if (device is EnergyStorage storage)
        {
            network.storages.Add(storage);
        }
    }

    private Network NetworkOf(EnergyDevice device)
    {
        return device != null && deviceNetworks.TryGetValue(device, out Network network) ? network : null;
    }

    // State of the network of a cable (both ends are in the same network), for the map of the control panel:
    // Tripped, Powered (at least one consumer powered), NotLinked (no consumer), SwitchedOff otherwise
    public NetworkState CableState(ElectricCable cable)
    {
        Network network = NetworkOf(cable.Start) ?? NetworkOf(cable.End);

        if (network == null || network.consumers.Count == 0) return NetworkState.NotLinked;
        if (network.tripped) return NetworkState.Tripped;

        foreach (EnergyConsumer consumer in network.consumers)
        {
            if (consumer != null && consumer.State == NetworkState.Powered) return NetworkState.Powered;
        }

        return NetworkState.SwitchedOff;
    }

    // ---------- Simulation ----------

    // Without distribution panel, only the second pass is done. With panels:
    // 1. Demand of the circuits: the outputs of the panels give without limit (after the other generators of their
    //    circuit), what they give is what the circuit asks. Repeated for the panels inside the circuit of others.
    // 2. Decisions of the panels: switch, fuse (request above the limit), shedding on low upstream batteries, delay.
    // 3. Distribution: the inputs ask the request of their circuit upstream, the outputs give it to the circuit. A
    //    network that lacks power first sheds its panels set to "shortage" (lowest priority first), then trips.
    //    A panel whose input network trips gives nothing to its circuit.
    private void Simulate(float deltaTime)
    {
        float hours = deltaTime / 3600f;

        CollectPanels();
        UpdateForcedOff();

        // 1. Demand of the circuits
        ignoreTrips = true;

        foreach (DistributionPanel panel in panels)
        {
            panel.InputDemand = 0f;
            panel.ChargeRoom = 0f;
            panel.OutputSupply = float.PositiveInfinity;
        }

        for (int iteration = 0; iteration < MaxPanelIterations && panels.Count > 0; iteration++)
        {
            Allocate(hours);

            bool changed = false;

            foreach (DistributionPanel panel in panels)
            {
                // What the output gave to the consumers is required, what it gave to the batteries is optional
                float charge = Get(generatorCharged, panel.Output);
                float request = Mathf.Max(0f, Get(generatorGiven, panel.Output) - charge);

                if (!Mathf.Approximately(request, panel.Request) || !Mathf.Approximately(charge, panel.ChargeRequest)) changed = true;

                panel.Request = request;
                panel.ChargeRequest = charge;
                // Seen from the circuit of a panel above it: its machines are required, the charge of its batteries
                // optional and within its power limit
                panel.InputDemand = panel.IsFeeding ? request : 0f;
                panel.ChargeRoom = panel.IsFeeding ? Mathf.Min(charge, Mathf.Max(0f, Subtract(panel.PowerLimit.Value, request))) : 0f;
            }

            if (!changed && iteration > 0) break;
        }

        ignoreTrips = false;

        // 2. Decisions of the panels
        foreach (DistributionPanel panel in panels)
        {
            Network inputNetwork = InputNetwork(panel);
            bool loop = inputNetwork != null && NetworkOf(panel.Output) == inputNetwork;

            panel.BeginUpdate(deltaTime, BatteryRatio(inputNetwork), loop);
            SetPanelFlow(panel, panel.IsFeeding);
        }

        // 3. Distribution. The networks that cannot be powered shed or trip one after the other, until nothing changes
        for (int iteration = 0; iteration < MaxTripIterations; iteration++)
        {
            Allocate(hours);

            bool changed = false;

            foreach (DistributionPanel panel in panels)
            {
                if (!panel.IsFeeding) continue;

                Network inputNetwork = InputNetwork(panel);

                if (inputNetwork == null || inputNetwork.tripped)
                {
                    panel.SetNoInput();
                    SetPanelFlow(panel, false);
                    changed = true;
                    continue;
                }

                // The charge of the circuit's batteries is what the upstream surplus gave (optional): the output gives
                // the required power plus that
                float supply = panel.Request + Get(panelCharge, panel);

                if (!Mathf.Approximately(supply, panel.OutputSupply))
                {
                    panel.OutputSupply = supply;
                    changed = true;
                }
            }

            foreach (Network network in networks)
            {
                if (network.tripped || network.lack <= Epsilon) continue;

                DistributionPanel shed = PanelToShed(network);

                if (shed != null)
                {
                    shed.Shed();
                    SetPanelFlow(shed, false);
                }
                else
                {
                    network.tripped = true;
                    network.rearmTimer = 0f;
                }

                changed = true;
            }

            if (!changed) break;
        }

        UpdateRearm(deltaTime, hours);
        ApplyResults(hours);

        foreach (DistributionPanel panel in panels)
        {
            Network inputNetwork = InputNetwork(panel);
            bool inputPowered = inputNetwork != null && !inputNetwork.tripped;

            // What the output actually gave to the batteries of the circuit
            panel.ChargeSupplied = Get(generatorCharged, panel.Output);
            panel.EndUpdate(deltaTime, hours, inputPowered, inputPowered ? Available(inputNetwork, hours) : 0f);
        }
    }

    private const int MaxPanelIterations = 8;
    private const int MaxTripIterations = 64;

    // While computing the demand of the circuits: every network is served, even the tripped ones (to be rearmed,
    // they must receive their demand)
    private bool ignoreTrips;

    private readonly List<DistributionPanel> panels = new List<DistributionPanel>();

    private void CollectPanels()
    {
        panels.Clear();

        foreach (EnergyDevice device in devices)
        {
            if (device is DistributionPanelOutput output && output.Panel != null && output.Panel.Input != null
                && devices.Contains(output.Panel.Input) && !panels.Contains(output.Panel))
            {
                panels.Add(output.Panel);
            }
        }
    }

    // Everything in the circuit of a switched off panel is off, through the panels under it too (their input is in the
    // circuit). Not the outputs of other panels feeding the same network: their production follows their own panel
    private readonly HashSet<EnergyDevice> forcedOff = new HashSet<EnergyDevice>();

    private void UpdateForcedOff()
    {
        forcedOff.Clear();
        HashSet<Network> seen = new HashSet<Network>();
        Queue<Network> queue = new Queue<Network>();

        foreach (DistributionPanel panel in panels)
        {
            Network circuit = NetworkOf(panel.Output);

            if (!panel.SwitchedOn && circuit != null && seen.Add(circuit)) queue.Enqueue(circuit);
        }

        while (queue.Count > 0)
        {
            foreach (EnergyDevice device in queue.Dequeue().devices)
            {
                if (device == null || device is DistributionPanelOutput) continue;

                forcedOff.Add(device);

                if (device is DistributionPanelInput input && input.Panel != null)
                {
                    Network circuit = NetworkOf(input.Panel.Output);

                    if (circuit != null && seen.Add(circuit)) queue.Enqueue(circuit);
                }
            }
        }

        foreach (EnergyDevice device in devices)
        {
            device.SetForcedOff(forcedOff.Contains(device));
        }
    }

    private Network InputNetwork(DistributionPanel panel)
    {
        return panel.Input != null && panel.Input.Cables.Count > 0 ? NetworkOf(panel.Input) : null;
    }

    // Required power asked upstream (a lack trips or sheds), and room for the charge of the circuit's batteries, given by
    // the surplus of the upstream generators like a battery (within the power limit). The output first gives the
    // required power, the charge is added once the upstream surplus is known (Simulate)
    private static void SetPanelFlow(DistributionPanel panel, bool feeding)
    {
        float limitRoom = Mathf.Max(0f, Subtract(panel.PowerLimit.Value, panel.Request));

        panel.InputDemand = feeding ? panel.Request : 0f;
        panel.ChargeRoom = feeding ? Mathf.Min(panel.ChargeRequest, limitRoom) : 0f;
        panel.OutputSupply = feeding ? panel.Request : 0f;
    }

    // Charge of the batteries of the network (0 to 1). 1 without battery
    private static float BatteryRatio(Network network)
    {
        if (network == null) return 1f;

        float stored = 0f;
        float capacity = 0f;

        foreach (EnergyStorage storage in network.storages)
        {
            if (storage == null || float.IsPositiveInfinity(storage.Capacity)) continue;

            stored += storage.Stored;
            capacity += storage.Capacity;
        }

        return capacity > 0f ? stored / capacity : 1f;
    }

    // Feeding panel of the network set to shed on shortage, with the lowest priority
    private DistributionPanel PanelToShed(Network network)
    {
        DistributionPanel shed = null;

        foreach (DistributionPanel panel in panels)
        {
            if (!panel.IsFeeding || panel.Shedding != DistributionPanel.SheddingMode.Shortage || panel.Request <= 0f) continue;
            if (InputNetwork(panel) != network) continue;

            if (shed == null || panel.Priority < shed.Priority)
            {
                shed = panel;
            }
        }

        return shed;
    }

    private void Allocate(float hours)
    {
        generatorLeft.Clear();
        generatorGiven.Clear();
        generatorCharged.Clear();
        generatorLeftBeforeCharge.Clear();
        panelCharge.Clear();
        storageCharge.Clear();
        storageDischarge.Clear();

        foreach (Network network in networks)
        {
            network.demand = 0f;

            foreach (EnergyConsumer consumer in network.consumers)
            {
                if (consumer != null) network.demand += consumer.Demand;
            }

            network.lack = network.tripped && !ignoreTrips ? 0f : network.demand;
        }

        // The networks share nothing: each one is computed on its own (the distribution panels pass from one to the
        // other through their values, set between two allocations)
        foreach (Network network in networks)
        {
            Allocate(network, hours);
        }
    }

    private void Allocate(Network network, float hours)
    {
        bool powered = ignoreTrips || !network.tripped;

        // The outputs of the distribution panels last: the circuit uses its own generators first
        List<EnergyGenerator> generators = new List<EnergyGenerator>();

        foreach (EnergyGenerator generator in network.generators)
        {
            if (generator != null && !(generator is DistributionPanelOutput)) generators.Add(generator);
        }

        foreach (EnergyGenerator generator in network.generators)
        {
            if (generator is DistributionPanelOutput) generators.Add(generator);
        }

        foreach (EnergyGenerator generator in generators)
        {
            float production = generator.Production;
            float given = powered ? Give(production, network) : 0f;

            generatorGiven[generator] = given;
            generatorLeft[generator] = Subtract(production, given);
            generatorLeftBeforeCharge[generator] = generatorLeft[generator];
        }

        // Surplus of the generators. First the charge of the batteries behind the distribution panels of the network:
        // for upstream, a consumption like the others (before its own batteries), only it does not trip the network
        // when it is not fully covered. Then the storages of the network (also when it is tripped)
        List<object> panelSinks = new List<object>();

        if (powered)
        {
            foreach (DistributionPanel panel in panels)
            {
                if (panel.ChargeRoom > 0f && InputNetwork(panel) == network) panelSinks.Add(panel);
            }
        }

        List<object> storageSinks = new List<object>();

        foreach (EnergyStorage storage in network.storages)
        {
            if (storage != null) storageSinks.Add(storage);
        }

        foreach (List<object> sinks in new[] { panelSinks, storageSinks })
        {
            if (sinks.Count == 0) continue;

            foreach (EnergyGenerator generator in generators)
            {
                float left = generatorLeft[generator];

                if (left <= 0f) continue;

                float charged = Charge(left, sinks, hours);
                generatorGiven[generator] += charged;
                generatorCharged[generator] = Get(generatorCharged, generator) + charged;
                generatorLeft[generator] = Subtract(left, charged);
            }
        }

        if (!powered) return;

        // Storages: what is still missing, energy charged during this update included
        foreach (EnergyStorage storage in network.storages)
        {
            if (storage == null) continue;

            float available = Mathf.Min(storage.OutputRate, storage.MaxDischarge(hours) + Get(storageCharge, storage));
            float given = Give(available, network);

            // Then the charge of the batteries behind the distribution panels of the network (optional, like the
            // surplus of a generator). Not a panel whose circuit is this network: the energy would only loop
            List<object> sinks = panelSinks.FindAll(sink => NetworkOf(((DistributionPanel)sink).Output) != network);

            if (sinks.Count > 0)
            {
                given += Charge(Subtract(available, given), sinks, hours);
            }

            storageDischarge[storage] = given;
        }
    }

    // Infinite stays infinite
    private static float Subtract(float amount, float given)
    {
        return float.IsPositiveInfinity(amount) ? amount : Mathf.Max(0f, amount - given);
    }

    // Gives "amount" to the lack of the network. Returns what was given
    private static float Give(float amount, Network network)
    {
        if (amount <= 0f || network.lack <= 0f) return 0f;

        if (float.IsPositiveInfinity(amount))
        {
            float lack = network.lack;
            network.lack = 0f;
            return lack;
        }

        // Infinite demand: cannot be covered by a finite amount, the network will trip
        if (float.IsPositiveInfinity(network.lack)) return 0f;

        float given = Mathf.Min(amount, network.lack);
        network.lack = given >= network.lack ? 0f : network.lack - given;
        return given;
    }

    // Equal shares between the sinks (storages, or distribution panels charging the batteries of their circuit), each one
    // within what it can still take. Returns what was taken
    private float Charge(float amount, List<object> sinks, float hours)
    {
        List<object> open = new List<object>();

        foreach (object sink in sinks)
        {
            if (Room(sink, hours) > 0f) open.Add(sink);
        }

        // The sinks with the least room first: what they cannot take goes to the others
        open.Sort((a, b) => Room(a, hours).CompareTo(Room(b, hours)));

        float total = 0f;

        for (int i = 0; i < open.Count && amount > 0f; i++)
        {
            object sink = open[i];
            float share = float.IsPositiveInfinity(amount) ? amount : amount / (open.Count - i);
            float taken = Mathf.Min(share, Room(sink, hours));

            // Infinite energy into an infinite room: nothing to store
            if (float.IsPositiveInfinity(taken)) continue;

            if (sink is EnergyStorage storage)
            {
                storageCharge[storage] = Get(storageCharge, storage) + taken;
            }
            else if (sink is DistributionPanel panel)
            {
                panelCharge[panel] = Get(panelCharge, panel) + taken;
            }

            amount = Subtract(amount, taken);
            total += taken;
        }

        return total;
    }

    private float Room(object sink, float hours)
    {
        if (sink is DistributionPanel panel)
        {
            return Mathf.Max(0f, panel.ChargeRoom - Get(panelCharge, panel));
        }

        EnergyStorage storage = (EnergyStorage)sink;
        float room = storage.MaxCharge(hours);
        return float.IsPositiveInfinity(room) ? room : room - Get(storageCharge, storage);
    }

    private static float Get<T>(Dictionary<T, float> map, T key)
    {
        return map.TryGetValue(key, out float value) ? value : 0f;
    }

    // A tripped network is rearmed after rearmDelay seconds once its generators and its storages could cover its demand
    private void UpdateRearm(float deltaTime, float hours)
    {
        foreach (Network network in networks)
        {
            if (!network.tripped) continue;

            if (Available(network, hours) + Epsilon >= network.demand)
            {
                network.rearmTimer += deltaTime;

                if (network.rearmTimer >= rearmDelay)
                {
                    network.tripped = false;
                    network.rearmTimer = 0f;
                }
            }
            else
            {
                network.rearmTimer = 0f;
            }
        }
    }

    // What the generators and the storages of the network could still give
    private float Available(Network network, float hours)
    {
        float available = 0f;

        foreach (EnergyGenerator generator in network.generators)
        {
            if (generator != null) available += Get(generatorLeftBeforeCharge, generator);
        }

        foreach (EnergyStorage storage in network.storages)
        {
            if (storage != null) available += Mathf.Max(0f, storage.MaxDischarge(hours) - Get(storageDischarge, storage));
        }

        return available;
    }

    private void ApplyResults(float hours)
    {
        foreach (Network network in networks)
        {
            foreach (EnergyGenerator generator in network.generators)
            {
                if (generator != null) generator.Output = Get(generatorGiven, generator);
            }

            foreach (EnergyStorage storage in network.storages)
            {
                if (storage != null) storage.Apply(Get(storageCharge, storage), Get(storageDischarge, storage), hours);
            }

            foreach (EnergyConsumer consumer in network.consumers)
            {
                if (consumer == null) continue;

                if (!consumer.IsOn)
                {
                    consumer.SetState(NetworkState.SwitchedOff);
                }
                else if (consumer.Cables.Count == 0)
                {
                    consumer.SetState(NetworkState.NotLinked);
                }
                else
                {
                    consumer.SetState(network.tripped ? NetworkState.Tripped : NetworkState.Powered);
                }
            }
        }
    }
}
