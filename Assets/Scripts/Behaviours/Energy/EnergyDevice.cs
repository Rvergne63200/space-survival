using System.Collections.Generic;
using UnityEngine;

// Something of the electrical network (EnergyNetworkManager): consumer, generator or storage, linked by electric cables.
// The consumers link the networks: everything linked to them is in their network. The generators and the storages do
// not: linked on two sides, they belong to two networks without merging them
public abstract class EnergyDevice : MonoBehaviour, ISaveableComponent
{
    // Off: it stops (a consumer no longer consumes, a generator no longer produces, a storage neither charges nor
    // discharges), but the cables still pass through it: a consumer off still links the networks. Saved
    [SerializeField] private bool switchedOn = true;

    // Where the cables are attached. Empty: the child named "Connector", or else the top of the object
    [SerializeField] private Transform connector;

    private readonly List<ElectricCable> cables = new List<ElectricCable>();
    public IReadOnlyList<ElectricCable> Cables => cables;

    public abstract bool LinksNetworks { get; }

    // Its own setting (kept while a distribution panel holds it off)
    public bool SwitchedOn => switchedOn;

    // In the circuit of a switched off distribution panel, directly or through the panels under it (set by the
    // EnergyNetworkManager): off whatever its own setting, which cannot be changed meanwhile
    public bool ForcedOff { get; private set; }

    // Actually running: its own setting AND every panel above it
    public bool IsOn => switchedOn && !ForcedOff;

    // The ports of a distribution panel follow the switch of their panel
    public virtual bool CanSwitch => true;

    public void SetSwitchedOn(bool on)
    {
        if (CanSwitch && !ForcedOff) switchedOn = on;
    }

    public void SetForcedOff(bool forced)
    {
        ForcedOff = forced;
    }

    // Text of the state for the pointer and the control panel: "" when running
    protected string OffText(bool feminine)
    {
        if (ForcedOff) return feminine ? "éteinte par le tableau, " : "éteint par le tableau, ";
        if (!switchedOn) return feminine ? "éteinte, " : "éteint, ";
        return "";
    }

    // Several devices on the same saved object (input and output of a distribution panel): saved with the cables to
    // find the right one again. Null for a single device
    public virtual string PortId => null;

    // Distribution panel the device is a port of
    public virtual DistributionPanel Panel => null;

    // Aimed device: the one holding the collider, or the closest port of an aimed distribution panel
    public static EnergyDevice FromHit(RaycastHit hit)
    {
        EnergyDevice device = hit.collider.GetComponentInParent<EnergyDevice>();

        if (device != null) return device;

        DistributionPanel panel = hit.collider.GetComponentInParent<DistributionPanel>();
        return panel != null ? panel.ClosestPort(hit.point) : null;
    }

    // Display name (text at the pointer): the placed item, or the object
    public virtual string DisplayName
    {
        get
        {
            PlacedObject placed = GetComponentInParent<PlacedObject>();
            return placed != null && placed.Data != null ? placed.Data.Name : gameObject.name;
        }
    }

    public Vector3 ConnectionPoint
    {
        get
        {
            if (connector == null)
            {
                connector = transform.Find("Connector");
            }

            if (connector != null) return connector.position;

            Bounds bounds = new Bounds(transform.position, Vector3.zero);

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer is LineRenderer) continue;
                bounds.Encapsulate(renderer.bounds);
            }

            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }
    }

    // Id of the saved object holding the device (cables save their ends with it). Null if it is not saved
    public string SaveId
    {
        get
        {
            SaveableObject saveable = GetComponentInParent<SaveableObject>();
            return saveable != null ? saveable.Id : null;
        }
    }

    protected virtual void OnEnable()
    {
        EnergyNetworkManager.Instance.Register(this);
    }

    protected virtual void OnDisable()
    {
        if (EnergyNetworkManager.HasInstance)
        {
            EnergyNetworkManager.Instance.Unregister(this);
        }
    }

    // Destroyed with the object (destruction from the build wheel): its cables are removed and given back
    protected virtual void OnDestroy()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.IsUnloading) return;
        if (!gameObject.scene.isLoaded) return;

        foreach (ElectricCable cable in new List<ElectricCable>(cables))
        {
            if (cable != null)
            {
                cable.Deconstruct();
            }
        }
    }

    public void AddCable(ElectricCable cable)
    {
        if (!cables.Contains(cable))
        {
            cables.Add(cable);
            EnergyNetworkManager.Instance.MarkDirty();
        }
    }

    public void RemoveCable(ElectricCable cable)
    {
        if (cables.Remove(cable) && EnergyNetworkManager.HasInstance)
        {
            EnergyNetworkManager.Instance.MarkDirty();
        }
    }

    public bool IsLinkedTo(EnergyDevice other)
    {
        foreach (ElectricCable cable in cables)
        {
            if (cable != null && cable.Other(this) == other) return true;
        }

        return false;
    }

    // Text at the pointer (EnergyDeviceInfo)
    public abstract string GetInfo();

    // ---------- Save ----------

    [System.Serializable]
    private class DeviceState
    {
        public bool switchedOn = true;
    }

    public virtual string SaveKey => "energyDevice" + PortId;

    public virtual string CaptureState()
    {
        return JsonUtility.ToJson(new DeviceState { switchedOn = switchedOn });
    }

    public virtual void RestoreState(string json)
    {
        DeviceState state = JsonUtility.FromJson<DeviceState>(json);

        if (state != null)
        {
            switchedOn = state.switchedOn;
        }
    }

    // For the storage, saved with its own state
    protected void RestoreSwitchedOn(bool on)
    {
        switchedOn = on;
    }
}
