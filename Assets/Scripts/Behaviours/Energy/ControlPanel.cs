using System.Collections.Generic;
using UnityEngine;

// Control panel: linked by cable to a network, it consumes a little power (its EnergyConsumer) and, while powered,
// opens a map of the network (ControlPanelUI, interaction key) to see and manage its devices from afar. It reaches:
// - its own network, and through the distribution panels of that network, their circuits (downstream, everything)
// - not upstream of a distribution panel: a panel whose circuit holds the control panel is shown, not what feeds it
// - in a building: also the whole network feeding the building ("upstream" network), with the circuits of its panels
//   (other buildings through corridors included), but not further up: a panel feeding that network is shown (and what
//   else it feeds there), not what is behind its input; a building feeding it, or fed by it without being linked by
//   corridor to the building of the control panel, is shown (with its cable), not its content.
//   Examples: PD1 ---> G1 ---> Bât1, panel in Bât1: G1, PD1 and what else PD1 feeds are seen, not what feeds PD1.
//   G1 ---> Bât1 ---> Bât2, panel in Bât2: Bât1 is seen, not its content nor G1. G1 ---- Bât1 ==== Bât2, panel in
//   Bât1: G1, Bât1 and Bât2 (with its content) are seen
[RequireComponent(typeof(EnergyConsumer))]
public class ControlPanel : MonoBehaviour, IInterractable
{
    private EnergyConsumer consumer;

    public EnergyConsumer Consumer => consumer != null ? consumer : (consumer = GetComponent<EnergyConsumer>());
    public bool IsPowered => Consumer.IsPowered;

    // Devices and cables reached from the network of the control panel (itself included)
    public void CollectNetwork(List<EnergyDevice> devices, List<ElectricCable> cables)
    {
        devices.Clear();
        cables.Clear();

        HashSet<EnergyDevice> visited = new HashSet<EnergyDevice>();
        HashSet<ElectricCable> seenCables = new HashSet<ElectricCable>();
        // Outputs reached through their input: their circuit is downstream, it is followed
        HashSet<EnergyDevice> opened = new HashSet<EnergyDevice>();
        // Buildings linked by corridors to the building of the control panel (their content is shown)
        HashSet<BuiltStructure> group = new HashSet<BuiltStructure>();
        // upstream: in the network feeding the building of the control panel
        Queue<(EnergyDevice device, bool upstream)> queue = new Queue<(EnergyDevice, bool)>();

        void Visit(EnergyDevice device, bool upstream)
        {
            if (device == null || !visited.Add(device)) return;

            devices.Add(device);
            queue.Enqueue((device, upstream));
        }

        Visit(Consumer, false);

        while (queue.Count > 0)
        {
            (EnergyDevice device, bool upstream) = queue.Dequeue();

            // In the upstream network: the output of a panel feeding it is in the same network, its cables are followed
            // (what else it feeds), but not what is behind its input. The output socket of a building: the building is
            // shown (its panel, to select it on the map), not its content
            if (upstream && device is DistributionPanelOutput && !opened.Contains(device))
            {
                if (device.Panel is BuildingOutlet outlet && outlet.Building != null && outlet.Building.Input != null && visited.Add(outlet.Building.Input))
                {
                    devices.Add(outlet.Building.Input);
                }

                // The inside of another building (reached otherwise than through its input) is not shown
                if (device.Panel is BuildingPowerPanel) continue;
            }

            // The input of a panel opens its circuit. In the upstream network, the input of a building opens it only if
            // the building is linked by corridors to the building of the control panel: another building fed by the same
            // network is shown (with its cable), not its content
            if (device is DistributionPanelInput input && input.Panel != null && input.Panel.Output != null
                && !(upstream && input.Panel is BuildingPowerPanel && !group.Contains(input.Panel.GetComponentInParent<BuiltStructure>())))
            {
                // Its circuit is downstream (a building of the group, a panel fed by the network...): everything behind
                // it is shown, buildings behind its output socket included
                opened.Add(input.Panel.Output);
                Visit(input.Panel.Output, false);
            }

            // The building holding the control panel: the network feeding it is seen too, with its corridor group
            if (!upstream && device is DistributionPanelOutput output && output.Panel is BuildingPowerPanel building && !opened.Contains(output))
            {
                group.UnionWith(Corridor.Group(building.GetComponentInParent<BuiltStructure>()));
                Visit(building.Input, true);
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
                Visit(other, upstream);
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
