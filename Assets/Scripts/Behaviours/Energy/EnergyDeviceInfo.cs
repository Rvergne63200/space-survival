using System.Collections.Generic;
using UnityEngine;

// Shows the state of the electrical devices of the object at the pointer (consumption, production, charge...).
// Interacting with it switches them on or off, unless a switched off distribution panel holds them off
public class EnergyDeviceInfo : MonoBehaviour, IInterractable
{
    public void Interract(GameObject interractor)
    {
        EnergyDevice[] devices = GetComponentsInChildren<EnergyDevice>();
        bool anyOn = false;

        foreach (EnergyDevice device in devices)
        {
            if (device.ForcedOff)
            {
                NotificationUI.Show(device.DisplayName + " est éteint par son tableau de distribution.", NotificationType.Warning);
                return;
            }

            if (device.CanSwitch && device.SwitchedOn) anyOn = true;
        }

        foreach (EnergyDevice device in devices)
        {
            device.SetSwitchedOn(!anyOn);
        }
    }

    public string GetInfo()
    {
        List<string> lines = new List<string>();

        foreach (EnergyDevice device in GetComponentsInChildren<EnergyDevice>())
        {
            lines.Add(device.GetInfo());
        }

        return string.Join("\n", lines);
    }
}
