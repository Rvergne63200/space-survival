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
// Created on first use, persists between the scenes
public class EnergyNetworkManager : MonoBehaviour
{
    public enum NetworkState { NotLinked, Powered, Tripped }

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

    private void Simulate(float deltaTime)
    {
        float hours = deltaTime / 3600f;

        foreach (Network network in networks)
        {
            network.demand = 0f;

            foreach (EnergyConsumer consumer in network.consumers)
            {
                if (consumer != null) network.demand += consumer.Consumption;
            }
        }

        // The networks that cannot be powered trip one after the other, freeing the generators they shared
        while (true)
        {
            Allocate(hours);

            bool trippedOne = false;

            foreach (Network network in networks)
            {
                if (!network.tripped && network.lack > Epsilon)
                {
                    network.tripped = true;
                    network.rearmTimer = 0f;
                    trippedOne = true;
                }
            }

            if (!trippedOne) break;
        }

        UpdateRearm(deltaTime, hours);
        ApplyResults(hours);
    }

    private void Allocate(float hours)
    {
        generatorLeft.Clear();
        generatorGiven.Clear();
        storageCharge.Clear();
        storageDischarge.Clear();

        foreach (Network network in networks)
        {
            network.lack = network.tripped ? 0f : network.demand;
        }

        foreach (KeyValuePair<EnergyGenerator, List<Network>> entry in generatorNetworks)
        {
            generatorLeft[entry.Key] = entry.Key != null ? entry.Key.Generation : 0f;
        }

        // Generators linked to a single powered network, then the shared ones: what is still missing, in proportion to
        // the lack of each network
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (KeyValuePair<EnergyGenerator, List<Network>> entry in generatorNetworks)
            {
                List<Network> powered = PoweredNetworks(entry.Value);

                if (powered.Count == 0 || (pass == 0) != (powered.Count == 1)) continue;

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

        // Surplus of the generators: charges the storages of their networks (also the tripped ones)
        foreach (KeyValuePair<EnergyGenerator, List<Network>> entry in generatorNetworks)
        {
            float left = generatorLeft[entry.Key];

            if (left <= 0f) continue;

            List<EnergyStorage> storages = new List<EnergyStorage>();

            foreach (Network network in entry.Value)
            {
                foreach (EnergyStorage storage in network.storages)
                {
                    if (storage != null && !storages.Contains(storage)) storages.Add(storage);
                }
            }

            float charged = Charge(left, storages, hours);
            generatorGiven[entry.Key] = Get(generatorGiven, entry.Key) + charged;
            generatorLeft[entry.Key] = Subtract(left, charged);
        }

        // Storages: what is still missing, energy charged during this update included
        foreach (KeyValuePair<EnergyStorage, List<Network>> entry in storageNetworks)
        {
            EnergyStorage storage = entry.Key;

            if (storage == null) continue;

            List<Network> powered = PoweredNetworks(entry.Value);

            if (powered.Count == 0) continue;

            float available = Mathf.Min(storage.OutputRate, storage.MaxDischarge(hours) + Get(storageCharge, storage));
            storageDischarge[storage] = Give(available, powered);
        }
    }

    // Infinite stays infinite
    private static float Subtract(float amount, float given)
    {
        return float.IsPositiveInfinity(amount) ? amount : Mathf.Max(0f, amount - given);
    }

    private static List<Network> PoweredNetworks(List<Network> networksOfDevice)
    {
        List<Network> powered = new List<Network>();

        foreach (Network network in networksOfDevice)
        {
            if (!network.tripped) powered.Add(network);
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

    // Equal shares between the storages, each one within what it can still take. Returns what was taken
    private float Charge(float amount, List<EnergyStorage> storages, float hours)
    {
        List<EnergyStorage> open = new List<EnergyStorage>();

        foreach (EnergyStorage storage in storages)
        {
            if (Room(storage, hours) > 0f) open.Add(storage);
        }

        // The storages with the least room first: what they cannot take goes to the others
        open.Sort((a, b) => Room(a, hours).CompareTo(Room(b, hours)));

        float total = 0f;

        for (int i = 0; i < open.Count && amount > 0f; i++)
        {
            EnergyStorage storage = open[i];
            float share = float.IsPositiveInfinity(amount) ? amount : amount / (open.Count - i);
            float taken = Mathf.Min(share, Room(storage, hours));

            // Infinite energy into an infinite room: nothing to store
            if (float.IsPositiveInfinity(taken)) continue;

            storageCharge[storage] = Get(storageCharge, storage) + taken;
            amount = Subtract(amount, taken);
            total += taken;
        }

        return total;
    }

    private float Room(EnergyStorage storage, float hours)
    {
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

            float available = 0f;

            foreach (EnergyGenerator generator in network.generators)
            {
                if (generator != null) available += Get(generatorLeftBeforeCharge, generator);
            }

            foreach (EnergyStorage storage in network.storages)
            {
                if (storage != null) available += storage.MaxDischarge(hours) - Get(storageDischarge, storage);
            }

            if (available + Epsilon >= network.demand)
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

                if (consumer.Cables.Count == 0)
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
