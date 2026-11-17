using System.Collections.Generic;
using UnityEngine;

// Shows the state of the electrical devices of the object at the pointer (consumption, production, charge...)
public class EnergyDeviceInfo : MonoBehaviour, IInterractable
{
    public void Interract(GameObject interractor) { }

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
