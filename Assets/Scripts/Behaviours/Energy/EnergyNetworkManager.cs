using System.Collections.Generic;
using UnityEngine;

// Electrical networks, updated every "interval" seconds. The rules:
// 1. Network: everything linked (cables, inside of a building, corridors) is one network. Only the panels separate
//    networks: their input is a consumer of the network above, their output a generator of their circuit, served after
//    the generators of the circuit. One way only.
// 2. Demand of a panel: required (the machines of its circuit) and optional (the charge of its batteries, the output
//    sockets of buildings behind it), within its power limit. The optional part is only served by the surplus, before
//    the batteries above: not fully served, it never trips anything.
// 3. Building: a panel whose circuit is its inside (BuildingPowerPanel). Its output socket is a panel inside it whose
//    whole demand is optional (BuildingOutlet). A corridor links the inputs of two buildings, and their insides (they
//    share their generators and batteries).
// 4. Each network: its generators give what it needs, their surplus charges its storages (equal shares, within their
//    input rate), the storages give what is still missing (within their output rate). The rest is lost.
// 5. Lack: once the flows are settled, a network that still lacks power sheds its panels set to "shortage" (lowest
//    priority first), then trips: its consumers stop. Rearmed after "rearmDelay" seconds once it could be powered.
// 6. Switching off: a switched off panel switches off its circuit (a building: what is inside it) and the circuits of the
//    panels in it, except the panels built into a building, which are only no longer fed.
// 7. Restart: a panel without input or shed starts again once upstream could feed it again. A built-in panel also
//    counts on what the panels above could still get from further upstream.
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

        BuildBuildingLinks();

        // Everything linked (cables, buildings), whatever the kind of device
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

                foreach (EnergyDevice other in Links(device))
                {
                    if (!devices.Contains(other) || deviceNetworks.ContainsKey(other)) continue;

                    deviceNetworks[other] = network;
                    queue.Enqueue(other);
                }
            }

            networks.Add(network);
        }
    }

    // Links without cable, built with the networks:
    // - the output of the panel built into a building (BuildingPowerPanel) feeds everything placed inside the building
    //   (BuildingInterior). Not the outputs of other distribution panels: their circuit is what is linked after them
    // - a corridor links the inputs of its two buildings
    private readonly Dictionary<EnergyDevice, List<EnergyDevice>> buildingLinks = new Dictionary<EnergyDevice, List<EnergyDevice>>();

    private void BuildBuildingLinks()
    {
        buildingLinks.Clear();

        foreach (EnergyDevice device in devices)
        {
            if (!(device is DistributionPanelOutput output) || !(output.Panel is BuildingPowerPanel building) || building.Interior == null) continue;

            foreach (EnergyDevice other in devices)
            {
                if (other is DistributionPanelOutput || building.IsPartOfBuilding(other)) continue;
                if (!building.Interior.Contains(other.transform.position)) continue;

                AddBuildingLink(output, other);
                AddBuildingLink(other, output);
            }

            // The output socket takes from the inside (what is in excess)
            DistributionPanelInput outletInput = building.Outlet != null ? building.Outlet.Input : null;

            if (outletInput != null && devices.Contains(outletInput))
            {
                AddBuildingLink(output, outletInput);
                AddBuildingLink(outletInput, output);
            }
        }

        // Corridors (finished): the inputs of their two buildings are linked, both ways, like a cable from input to input,
        // and so are their insides (the outputs of their built-in panels): the buildings linked by corridors share their
        // generators and batteries. Each building keeps its own panel (switching one off only switches off what is inside it)
        foreach (Corridor corridor in Corridor.All)
        {
            if (corridor == null || !corridor.IsBuilt) continue;

            DistributionPanelInput startInput = BuildingInput(corridor.Start);
            DistributionPanelInput endInput = BuildingInput(corridor.End);

            if (startInput == null || endInput == null || !devices.Contains(startInput) || !devices.Contains(endInput)) continue;

            AddBuildingLink(startInput, endInput);
            AddBuildingLink(endInput, startInput);

            DistributionPanelOutput startOutput = BuildingOutput(corridor.Start);
            DistributionPanelOutput endOutput = BuildingOutput(corridor.End);

            if (startOutput == null || endOutput == null || !devices.Contains(startOutput) || !devices.Contains(endOutput)) continue;

            AddBuildingLink(startOutput, endOutput);
            AddBuildingLink(endOutput, startOutput);
        }
    }

    private static DistributionPanelInput BuildingInput(BuiltStructure structure)
    {
        BuildingPowerPanel panel = structure != null ? structure.GetComponentInChildren<BuildingPowerPanel>() : null;
        return panel != null ? panel.Input : null;
    }

    private static DistributionPanelOutput BuildingOutput(BuiltStructure structure)
    {
        BuildingPowerPanel panel = structure != null ? structure.GetComponentInChildren<BuildingPowerPanel>() : null;
        return panel != null ? panel.Output : null;
    }

    private void AddBuildingLink(EnergyDevice from, EnergyDevice to)
    {
        if (!buildingLinks.TryGetValue(from, out List<EnergyDevice> links))
        {
            links = new List<EnergyDevice>();
            buildingLinks[from] = links;
        }

        links.Add(to);
    }

    // Devices linked to this one: by cable, or through a building (inside of it, or its built-in panel). Up to date
    // after each rebuild of the networks
    public IEnumerable<EnergyDevice> Links(EnergyDevice device)
    {
        foreach (ElectricCable cable in device.Cables)
        {
            EnergyDevice other = cable != null ? cable.Other(device) : null;

            if (other != null) yield return other;
        }

        if (buildingLinks.TryGetValue(device, out List<EnergyDevice> links))
        {
            foreach (EnergyDevice other in links)
            {
                if (other != null) yield return other;
            }
        }
    }

    // Linked to at least one device, by cable or through a building
    public bool IsLinked(EnergyDevice device)
    {
        using (IEnumerator<EnergyDevice> links = Links(device).GetEnumerator())
        {
            return links.MoveNext();
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

    // Power used by what is inside the building only (not by the other buildings sharing its inside)
    public float BuildingConsumption(BuildingPowerPanel building)
    {
        Network network = building != null ? NetworkOf(building.Output) : null;

        if (network == null || network.tripped || building.Interior == null) return 0f;

        float consumption = 0f;

        foreach (EnergyConsumer consumer in network.consumers)
        {
            if (consumer == null || building.IsPartOfBuilding(consumer) || !building.Interior.Contains(consumer.transform.position)) continue;

            consumption += consumer.Demand;
        }

        return consumption;
    }

    // The network of the device is tripped (map of the control panel: generators and storages in red)
    public bool IsTripped(EnergyDevice device)
    {
        Network network = NetworkOf(device);
        return network != null && network.tripped;
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
        ComputeDemands(hours);

        // 2. Decisions of the panels
        bool feedingChanged = false;

        foreach (DistributionPanel panel in panels)
        {
            Network inputNetwork = InputNetwork(panel);
            bool loop = inputNetwork != null && NetworkOf(panel.Output) == inputNetwork;
            bool wasFeeding = panel.IsFeeding;

            panel.BeginUpdate(deltaTime, BatteryRatio(inputNetwork), loop);
            feedingChanged |= wasFeeding != panel.IsFeeding;
        }

        // A panel that starts (or stops) changes what the panels above it ask: their demand is computed again, so that
        // a circuit starting under another one does not trip it
        if (feedingChanged)
        {
            ComputeDemands(hours);
        }

        foreach (DistributionPanel panel in panels)
        {
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
                float supply = (panel.OptionalDemand ? 0f : panel.Request) + Get(panelCharge, panel);

                if (!Mathf.Approximately(supply, panel.OutputSupply))
                {
                    panel.OutputSupply = supply;
                    changed = true;
                }
            }

            // The optional power (charge, output sockets of buildings) goes down one panel per pass: the networks only
            // trip once it has arrived everywhere (otherwise a circuit behind an output socket would trip before
            // receiving anything)
            if (changed && iteration < MaxTripIterations - 1) continue;

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
            panel.OutletSupplied = Mathf.Min(panel.ChargeSupplied, panel.OutletRequest);
            // A built-in panel also counts on what the panels feeding its network could still get upstream
            float upstream = !inputPowered ? 0f : panel.Building != null ? Potential(inputNetwork, hours, new HashSet<DistributionPanel>()) : Available(inputNetwork, hours);
            panel.EndUpdate(deltaTime, hours, inputPowered, upstream);
        }
    }

    // The outputs of the panels give without limit (after the other generators of their circuit): what they give is
    // what the circuit asks. Repeated for the panels inside the circuit of others. Tripped networks count too (to be
    // rearmed, they must receive their demand)
    private void ComputeDemands(float hours)
    {
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
                SetDemand(panel, panel.IsFeeding);
            }

            if (!changed && iteration > 0) break;
        }

        // Part of the optional demand going to output sockets of buildings (display: "Sortie", not a charge)
        foreach (DistributionPanel panel in panels)
        {
            panel.OutletRequest = OutletRequest(panel, 0);
        }

        ignoreTrips = false;
    }

    // Optional power the panel passes to output sockets of buildings: all of it for an output socket, otherwise what the
    // panels of its circuit pass there (within what they may take as optional)
    private float OutletRequest(DistributionPanel panel, int depth)
    {
        if (panel.OptionalDemand) return panel.Request + panel.ChargeRequest;
        if (depth > MaxPanelIterations) return 0f;

        Network circuit = NetworkOf(panel.Output);
        float outlet = 0f;

        foreach (DistributionPanel child in panels)
        {
            if (child == panel || circuit == null || InputNetwork(child) != circuit) continue;

            outlet += Mathf.Min(OutletRequest(child, depth + 1), child.ChargeRoom);
        }

        return Mathf.Min(outlet, panel.ChargeRequest);
    }

    // Seen from the circuit above the panel: its machines are required, the charge of its batteries optional and within
    // its power limit. A panel with an optional demand (output socket of a building): everything is optional
    private static void SetDemand(DistributionPanel panel, bool feeding)
    {
        if (!feeding)
        {
            panel.InputDemand = 0f;
            panel.ChargeRoom = 0f;
        }
        else if (panel.OptionalDemand)
        {
            panel.InputDemand = 0f;
            panel.ChargeRoom = Mathf.Min(panel.Request + panel.ChargeRequest, panel.PowerLimit.Value);
        }
        else
        {
            panel.InputDemand = panel.Request;
            panel.ChargeRoom = Mathf.Min(panel.ChargeRequest, Mathf.Max(0f, Subtract(panel.PowerLimit.Value, panel.Request)));
        }
    }

    // What the network could still give, counting what the panels feeding it could still get from upstream (within
    // their power limit). Used for the buildings, to start again behind the output socket of another one
    private float Potential(Network network, float hours, HashSet<DistributionPanel> seen)
    {
        float potential = Available(network, hours);

        foreach (EnergyGenerator generator in network.generators)
        {
            if (!(generator is DistributionPanelOutput output) || output.Panel == null || !output.Panel.IsFeeding || !seen.Add(output.Panel)) continue;

            DistributionPanel panel = output.Panel;
            Network inputNetwork = InputNetwork(panel);

            if (inputNetwork == null || inputNetwork.tripped) continue;

            float given = (panel.OptionalDemand ? 0f : panel.Request) + Get(panelCharge, panel);
            float room = Subtract(panel.PowerLimit.Value, given);

            potential += Mathf.Min(room, Potential(inputNetwork, hours, seen));
        }

        return potential;
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

    // A switched off panel switches off its circuit (ForcedOff, their own setting being kept), and the circuits of the
    // panels in it, except the panels built into a building (the building fed by the circuit, the output socket of a
    // building): they are only no longer fed. Never the outputs of other panels feeding the same network: their
    // production follows their own panel. A building only switches off what is inside it (its inside is shared with the
    // buildings linked by corridors)
    private readonly HashSet<EnergyDevice> forcedOff = new HashSet<EnergyDevice>();

    private void UpdateForcedOff()
    {
        forcedOff.Clear();

        // Circuit to switch off, and the building it is limited to: the inside of a building is shared with the buildings
        // linked by corridors (one network), switching it off only switches off what is inside it
        HashSet<(Network, BuildingPowerPanel)> seen = new HashSet<(Network, BuildingPowerPanel)>();
        Queue<(Network network, BuildingPowerPanel building)> queue = new Queue<(Network, BuildingPowerPanel)>();

        void SwitchOffCircuit(DistributionPanel panel)
        {
            Network circuit = NetworkOf(panel.Output);
            BuildingPowerPanel building = panel as BuildingPowerPanel;

            if (circuit != null && seen.Add((circuit, building))) queue.Enqueue((circuit, building));
        }

        foreach (DistributionPanel panel in panels)
        {
            if (!panel.SwitchedOn) SwitchOffCircuit(panel);
        }

        while (queue.Count > 0)
        {
            (Network network, BuildingPowerPanel building) = queue.Dequeue();

            foreach (EnergyDevice device in network.devices)
            {
                if (device == null || device is DistributionPanelOutput) continue;

                // Only what is inside the building (its own ports included)
                if (building != null && !building.IsPartOfBuilding(device)
                    && (building.Interior == null || !building.Interior.Contains(device.transform.position))) continue;

                if (device is DistributionPanelInput input && input.Panel != null)
                {
                    if (input.Panel.Building != null) continue;

                    SwitchOffCircuit(input.Panel);
                }

                forcedOff.Add(device);
            }
        }

        foreach (EnergyDevice device in devices)
        {
            device.SetForcedOff(forcedOff.Contains(device));
        }
    }

    private Network InputNetwork(DistributionPanel panel)
    {
        return panel.Input != null && IsLinked(panel.Input) ? NetworkOf(panel.Input) : null;
    }

    // Required power asked upstream (a lack trips or sheds), and room for the charge of the circuit's batteries, given by
    // the surplus of the upstream generators like a battery (within the power limit). The output first gives the
    // required power, the charge is added once the upstream surplus is known (Simulate)
    private static void SetPanelFlow(DistributionPanel panel, bool feeding)
    {
        SetDemand(panel, feeding);

        // The required power at first, the charge is added once the upstream surplus is known (Simulate). Nothing
        // required for an optional demand: it only gives what it received
        panel.OutputSupply = feeding && !panel.OptionalDemand ? panel.Request : 0f;
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
                else if (!IsLinked(consumer))
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
