using System.Collections.Generic;
using UnityEngine;

// Control panel: linked by cable to a network, it consumes a little power (its EnergyConsumer) and, while powered,
// opens a map of the network (ControlPanelUI, interaction key) to see and manage its devices from afar.
// It reaches the devices of its network and, through the distribution panels of that network, those of their circuits
// (not upstream: a panel whose circuit holds the control panel is shown, not what feeds it)
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

        HashSet<EnergyDevice> visited = new HashSet<EnergyDevice> { Consumer };
        HashSet<ElectricCable> seenCables = new HashSet<ElectricCable>();
        Queue<EnergyDevice> queue = new Queue<EnergyDevice>();

        devices.Add(Consumer);
        queue.Enqueue(Consumer);

        // Consumers link the networks; the output of a panel reached by its input opens its circuit. A generator or a
        // storage is in the network but does not lead further
        while (queue.Count > 0)
        {
            EnergyDevice device = queue.Dequeue();

            if (device is DistributionPanelInput input && input.Panel != null && input.Panel.Output != null && visited.Add(input.Panel.Output))
            {
                devices.Add(input.Panel.Output);
                queue.Enqueue(input.Panel.Output);
            }

            foreach (ElectricCable cable in device.Cables)
            {
                if (cable == null) continue;

                if (seenCables.Add(cable))
                {
                    cables.Add(cable);
                }

                EnergyDevice other = cable.Other(device);

                if (other == null || !visited.Add(other)) continue;

                devices.Add(other);

                if (other is EnergyConsumer)
                {
                    queue.Enqueue(other);
                }
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
