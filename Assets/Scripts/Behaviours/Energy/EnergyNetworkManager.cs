using System.Collections.Generic;
using UnityEngine;

// Electrical networks: built from the devices (EnergyDevice) and their cables, updated every "interval" seconds.
// - A network is a group of consumers linked by cables, with the generators and storages linked to them. Generators and
//   storages do not link networks: linked on two sides, they belong to two networks. Two of them linked together
//   (generator - battery...) form a network without consumer.
// - Each update: the generators linked to a single network give it first what it needs, then the shared generators
//   cover what is still missing, in proportion to what each of their networks lacks. Their surplus charges the
//   storages of their networks (equal shares, within their input rate), and the storages then give what is still
//   missing (within their output rate). The rest of the production is lost.
// - A network that still lacks power trips: its consumers stop, and its share of the shared generators goes to their
//   other networks. It is rearmed after "rearmDelay" seconds once its generators and storages could cover its demand.
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
    private readonly Dictionary<EnergyGenerator, List<Network>> generatorNetworks = new Dictionary<EnergyGenerator, List<Network>>();
    private readonly Dictionary<EnergyStorage, List<Network>> storageNetworks = new Dictionary<EnergyStorage, List<Network>>();

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

        // A new network stays tripped if one of its consumers was in a tripped network
        HashSet<EnergyConsumer> wasTripped = new HashSet<EnergyConsumer>();

        foreach (Network network in networks)
        {
            if (!network.tripped) continue;

            foreach (EnergyConsumer consumer in network.consumers)
            {
                wasTripped.Add(consumer);
            }
        }

        networks.Clear();
        generatorNetworks.Clear();
        storageNetworks.Clear();
        consumerNetworks.Clear();
        devices.RemoveWhere(device => device == null);

        HashSet<EnergyDevice> visited = new HashSet<EnergyDevice>();

        foreach (EnergyDevice device in devices)
        {
            if (!(device is EnergyConsumer start) || visited.Contains(device)) continue;

            Network network = new Network();
            Queue<EnergyConsumer> queue = new Queue<EnergyConsumer>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                EnergyConsumer consumer = queue.Dequeue();
                network.consumers.Add(consumer);
                consumerNetworks[consumer] = network;
                network.tripped |= wasTripped.Contains(consumer);

                foreach (ElectricCable cable in consumer.Cables)
                {
                    EnergyDevice other = cable != null ? cable.Other(consumer) : null;

                    if (other == null || !devices.Contains(other)) continue;

                    if (other is EnergyConsumer otherConsumer)
                    {
                        if (visited.Add(otherConsumer))
                        {
                            queue.Enqueue(otherConsumer);
                        }
                    }
                    else
                    {
                        AddSource(network, other);
                    }
                }
            }

            AddNetwork(network);
        }

        // Generators and storages linked together, without consumer between them
        HashSet<ElectricCable> sourceCables = new HashSet<ElectricCable>();

        foreach (EnergyDevice device in devices)
        {
            if (device.LinksNetworks) continue;

            foreach (ElectricCable cable in device.Cables)
            {
                EnergyDevice other = cable != null ? cable.Other(device) : null;

                if (other == null || other.LinksNetworks || !devices.Contains(other) || !sourceCables.Add(cable)) continue;

                Network network = new Network();
                AddSource(network, device);
                AddSource(network, other);
                AddNetwork(network);
            }
        }
    }

    private static void AddSource(Network network, EnergyDevice device)
    {
        if (device is EnergyGenerator generator && !network.generators.Contains(generator))
        {
            network.generators.Add(generator);
        }
        else if (device is EnergyStorage storage && !network.storages.Contains(storage))
        {
            network.storages.Add(storage);
        }
    }

    private void AddNetwork(Network network)
    {
        networks.Add(network);

        foreach (EnergyGenerator generator in network.generators)
        {
            GetList(generatorNetworks, generator).Add(network);
        }

        foreach (EnergyStorage storage in network.storages)
        {
            GetList(storageNetworks, storage).Add(network);
        }
    }

    private static List<Network> GetList<T>(Dictionary<T, List<Network>> map, T key)
    {
        if (!map.TryGetValue(key, out List<Network> list))
        {
            list = new List<Network>();
            map[key] = list;
        }

        return list;
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
            bool loop = inputNetwork != null && generatorNetworks.TryGetValue(panel.Output, out List<Network> outputNetworks) && outputNetworks.Contains(inputNetwork);

            panel.BeginUpdate(deltaTime, BatteryRatio(inputNetwork), loop);
            SetPanelFlow(panel, panel.IsFeeding);
        }

        // 3. Distribution. The networks that cannot be powered shed or trip one after the other, freeing the generators
        // they shared
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
    private readonly Dictionary<EnergyConsumer, Network> consumerNetworks = new Dictionary<EnergyConsumer, Network>();

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
    // circuit). A device in several circuits is off as soon as one of their panels is (AND). Not the outputs of other
    // panels feeding the same networks: their production follows their own panel
    private readonly HashSet<EnergyDevice> forcedOff = new HashSet<EnergyDevice>();

    private void UpdateForcedOff()
    {
        forcedOff.Clear();
        Queue<EnergyDevice> queue = new Queue<EnergyDevice>();

        foreach (DistributionPanel panel in panels)
        {
            if (!panel.SwitchedOn) queue.Enqueue(panel.Output);
        }

        while (queue.Count > 0)
        {
            EnergyDevice device = queue.Dequeue();

            foreach (ElectricCable cable in device.Cables)
            {
                EnergyDevice other = cable != null ? cable.Other(device) : null;

                if (other == null || other is DistributionPanelOutput || !forcedOff.Add(other)) continue;

                if (other is EnergyConsumer)
                {
                    queue.Enqueue(other);
                }

                if (other is DistributionPanelInput input && input.Panel != null && input.Panel.Output != null)
                {
                    queue.Enqueue(input.Panel.Output);
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
        return panel.Input != null && panel.Input.Cables.Count > 0 && consumerNetworks.TryGetValue(panel.Input, out Network network) ? network : null;
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

        foreach (KeyValuePair<EnergyGenerator, List<Network>> entry in generatorNetworks)
        {
            generatorLeft[entry.Key] = entry.Key != null ? entry.Key.Production : 0f;
        }

        // Generators linked to a single powered network, then the shared ones: what is still missing, in proportion to
        // the lack of each network. The outputs of the distribution panels last: their circuit uses its own
        // generators first
        for (int pass = 0; pass < 3; pass++)
        {
            foreach (KeyValuePair<EnergyGenerator, List<Network>> entry in generatorNetworks)
            {
                List<Network> powered = PoweredNetworks(entry.Value);

                if (powered.Count == 0) continue;

                bool isPanel = entry.Key is DistributionPanelOutput;

                if (pass == 2 ? !isPanel : (isPanel || (pass == 0) != (powered.Count == 1))) continue;

                float given = Give(generatorLeft[entry.Key], powered);
                generatorGiven[entry.Key] = Get(generatorGiven, entry.Key) + given;
                generatorLeft[entry.Key] = Subtract(generatorLeft[entry.Key], given);
            }
        }

        generatorLeftBeforeCharge.Clear();

        foreach (KeyValuePair<EnergyGenerator, float> entry in generatorLeft)
        {
            generatorLeftBeforeCharge[entry.Key] = entry.Value;
        }

        // Surplus of the generators: charges the storages of their networks (also the tripped ones). The panels last,
        // the surplus of the circuit's own generators goes first
        List<KeyValuePair<EnergyGenerator, List<Network>>> chargers = new List<KeyValuePair<EnergyGenerator, List<Network>>>(generatorNetworks);
        chargers.Sort((a, b) => (a.Key is DistributionPanelOutput ? 1 : 0).CompareTo(b.Key is DistributionPanelOutput ? 1 : 0));

        // The distribution panels first: for upstream, the charge of the batteries of their circuit is a consumption like
        // the others (before the upstream batteries), only it does not trip the network when it is not fully covered.
        // Then the storages of the networks
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (KeyValuePair<EnergyGenerator, List<Network>> entry in chargers)
            {
                float left = generatorLeft[entry.Key];

                if (left <= 0f) continue;

                List<object> sinks = new List<object>();

                foreach (Network network in entry.Value)
                {
                    if (pass == 0)
                    {
                        foreach (DistributionPanel panel in panels)
                        {
                            if (panel.ChargeRoom > 0f && !sinks.Contains(panel) && InputNetwork(panel) == network && (ignoreTrips || !network.tripped))
                            {
                                sinks.Add(panel);
                            }
                        }
                    }
                    else
                    {
                        foreach (EnergyStorage storage in network.storages)
                        {
                            if (storage != null && !sinks.Contains(storage)) sinks.Add(storage);
                        }
                    }
                }

                if (sinks.Count == 0) continue;

                float charged = Charge(left, sinks, hours);
                generatorGiven[entry.Key] = Get(generatorGiven, entry.Key) + charged;
                generatorCharged[entry.Key] = Get(generatorCharged, entry.Key) + charged;
                generatorLeft[entry.Key] = Subtract(left, charged);
            }
        }

        // Storages: what is still missing, energy charged during this update included
        foreach (KeyValuePair<EnergyStorage, List<Network>> entry in storageNetworks)
        {
            EnergyStorage storage = entry.Key;

            if (storage == null) continue;

            List<Network> powered = PoweredNetworks(entry.Value);

            if (powered.Count == 0) continue;

            float available = Mathf.Min(storage.OutputRate, storage.MaxDischarge(hours) + Get(storageCharge, storage));
            float given = Give(available, powered);

            // Then the charge of the batteries behind the distribution panels of its networks (optional, like the
            // surplus of a generator). Not a panel whose circuit holds this storage: the energy would only loop
            List<object> sinks = new List<object>();

            foreach (DistributionPanel panel in panels)
            {
                if (panel.ChargeRoom <= 0f || !powered.Contains(InputNetwork(panel))) continue;

                if (generatorNetworks.TryGetValue(panel.Output, out List<Network> circuit) && circuit.Exists(network => network.storages.Contains(storage))) continue;

                sinks.Add(panel);
            }

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

    private List<Network> PoweredNetworks(List<Network> networksOfDevice)
    {
        List<Network> powered = new List<Network>();

        foreach (Network network in networksOfDevice)
        {
            if (ignoreTrips || !network.tripped) powered.Add(network);
        }

        return powered;
    }

    // Gives "amount" to the lack of the networks, in proportion to their lack. Returns what was given
    private static float Give(float amount, List<Network> targets)
    {
        if (amount <= 0f) return 0f;

        float totalLack = 0f;

        foreach (Network network in targets)
        {
            totalLack += network.lack;
        }

        if (totalLack <= 0f) return 0f;

        if (float.IsPositiveInfinity(amount))
        {
            foreach (Network network in targets)
            {
                network.lack = 0f;
            }

            return totalLack;
        }

        // Infinite demand: cannot be covered by a finite amount, the network will trip
        if (float.IsPositiveInfinity(totalLack)) return 0f;

        float ratio = Mathf.Min(1f, amount / totalLack);

        foreach (Network network in targets)
        {
            network.lack = ratio >= 1f ? 0f : network.lack * (1f - ratio);
        }

        return totalLack * ratio;
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

    // A tripped network is rearmed after rearmDelay seconds once its generators (what the others leave them) and its
    // storages could cover its demand
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

    // What the generators (what the other networks leave them) and the storages of the network could still give
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
        foreach (KeyValuePair<EnergyGenerator, List<Network>> entry in generatorNetworks)
        {
            if (entry.Key != null)
            {
                entry.Key.Output = Get(generatorGiven, entry.Key);
            }
        }

        foreach (KeyValuePair<EnergyStorage, List<Network>> entry in storageNetworks)
        {
            if (entry.Key != null)
            {
                entry.Key.Apply(Get(storageCharge, entry.Key), Get(storageDischarge, entry.Key), hours);
            }
        }

        foreach (Network network in networks)
        {
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

        // Devices without network: nothing given or taken
        foreach (EnergyDevice device in devices)
        {
            if (device is EnergyGenerator generator && !generatorNetworks.ContainsKey(generator))
            {
                generator.Output = 0f;
            }
            else if (device is EnergyStorage storage && !storageNetworks.ContainsKey(storage))
            {
                storage.Apply(0f, 0f, hours);
            }
        }
    }
}
