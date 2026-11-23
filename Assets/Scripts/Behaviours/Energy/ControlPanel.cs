using System.Collections.Generic;
using UnityEngine;

// Control panel: linked by cable to a network, it consumes a little power (its EnergyConsumer) and, while powered,
// opens a map of the network (ControlPanelUI, interaction key) to see and manage its devices from afar. What it sees:
// 1. its network, and everything downstream: behind the input of every panel it meets, buildings included
// 2. never what is behind the input of a panel feeding what it sees (upstream)
// 3. in a building, its network is that of its corridor group, with the network feeding the group
// 4. on that feeding network, the other buildings are seen from outside (outline, cable), not their inside
// Examples: PD1 ---> G1 ---> B1, panel in B1: G1, PD1 and what else PD1 feeds, not what feeds PD1 (3, 2).
// G1 ---> B1 ---> B2, panel in B2: B1 from outside, not G1 (4, 2). G1 ---- B1 ==== B2 ---> B3, panel in B1: G1, B1,
// B2 and B3 with their content (3, 1). PD1 ---> B1 and PD1 ---> B5, panel in B1: B5 from outside (4)
// Model (child "Model", Resources/Models/ControlPanel): its ControlPanelAnimator deploys the holographic screen while
// the panel is powered and folds it otherwise
[RequireComponent(typeof(EnergyConsumer))]
public class ControlPanel : MonoBehaviour, IInterractable
{
    private EnergyConsumer consumer;
    private ControlPanelAnimator display;

    public EnergyConsumer Consumer => consumer != null ? consumer : (consumer = GetComponent<EnergyConsumer>());
    public bool IsPowered => Consumer.IsPowered;

    private void Awake()
    {
        display = GetComponentInChildren<ControlPanelAnimator>(true);

        if (display == null) Debug.LogWarning(name + ": no ControlPanelAnimator in the children, the screen is not shown", this);
    }

    private void Update()
    {
        if (display != null && display.IsPowered != IsPowered) display.SetPowered(IsPowered);
    }

    // Devices and cables seen from the control panel (itself included)
    public void CollectNetwork(List<EnergyDevice> devices, List<ElectricCable> cables)
    {
        devices.Clear();
        cables.Clear();

        HashSet<EnergyDevice> listed = new HashSet<EnergyDevice>();
        HashSet<EnergyDevice> visited = new HashSet<EnergyDevice>();
        HashSet<ElectricCable> seenCables = new HashSet<ElectricCable>();
        // Outputs reached through their input: their circuit is downstream (rule 1)
        HashSet<EnergyDevice> opened = new HashSet<EnergyDevice>();
        // Corridor group of the building of the control panel (rule 3)
        HashSet<BuiltStructure> group = new HashSet<BuiltStructure>();
        // feeding: on the network feeding the group (rule 4)
        Queue<(EnergyDevice device, bool feeding)> queue = new Queue<(EnergyDevice, bool)>();

        void Show(EnergyDevice device)
        {
            if (device != null && listed.Add(device)) devices.Add(device);
        }

        void Visit(EnergyDevice device, bool feeding)
        {
            Show(device);

            if (device != null && visited.Add(device)) queue.Enqueue((device, feeding));
        }

        // Rule 4: the inside of a building outside the group, met on the feeding network, is not entered
        bool Closed(DistributionPanel panel, bool feeding)
        {
            return feeding && panel is BuildingPowerPanel building && !group.Contains(building.Structure);
        }

        Visit(Consumer, false);

        while (queue.Count > 0)
        {
            (EnergyDevice device, bool feeding) = queue.Dequeue();
            DistributionPanel panel = device.Panel;

            // A port of a building: the building is shown (its panel, selected on the map)
            if (panel != null && panel.Building != null) Show(panel.Building.Input);

            // Rule 1: the input of a panel opens its circuit, which is downstream
            if (device is DistributionPanelInput && panel != null && panel.Output != null && !Closed(panel, feeding))
            {
                opened.Add(panel.Output);
                Visit(panel.Output, false);
            }

            // The output of a panel met from its circuit: same network, its cables are followed, never what is behind its
            // input (rule 2)
            if (device is DistributionPanelOutput && !opened.Contains(device))
            {
                if (Closed(panel, feeding)) continue;

                // Rule 3: the building of the control panel
                if (!feeding && panel is BuildingPowerPanel building)
                {
                    group.UnionWith(Corridor.Group(building.Structure));
                    Visit(building.Input, true);
                }
            }

            foreach (ElectricCable cable in device.Cables)
            {
                if (cable != null && seenCables.Add(cable))
                {
                    cables.Add(cable);
                }
            }

            // Cables, the inside of the buildings and the corridors (without cable)
            foreach (EnergyDevice other in EnergyNetworkManager.Instance.Links(device))
            {
                Visit(other, feeding);
            }
        }
    }

    public string DisplayName => Consumer.DisplayName;

    // ---------- Interaction ----------

    public void Interract(GameObject interractor)
    {
        if (Consumer.ForcedOff)
        {
            NotificationUI.Show(DisplayName + " est éteint par son tableau de distribution.", NotificationType.Warning);
            return;
        }

        // Switched off from its own map: switched on again here, it is the only place to do it
        if (!Consumer.SwitchedOn)
        {
            Consumer.SetSwitchedOn(true);
            NotificationUI.Show(DisplayName + " rallumé.");
            return;
        }

        if (!IsPowered)
        {
            NotificationUI.Show(DisplayName + " n'est pas alimenté.", NotificationType.Warning);
            return;
        }

        if (ControlPanelUI.Instance != null)
        {
            ControlPanelUI.Instance.Open(this);
        }
    }

    public string GetInfo()
    {
        return Consumer.GetInfo();
    }
}
