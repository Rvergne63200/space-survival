using System.Collections.Generic;
using UnityEngine;

// Something of the electrical network (EnergyNetworkManager): consumer, generator or storage, linked by electric cables.
// The consumers link the networks: everything linked to them is in their network. The generators and the storages do
// not: linked on two sides, they belong to two networks without merging them
public abstract class EnergyDevice : MonoBehaviour
{
    // Where the cables are attached. Empty: the child named "Connector", or else the top of the object
    [SerializeField] private Transform connector;

    private readonly List<ElectricCable> cables = new List<ElectricCable>();
    public IReadOnlyList<ElectricCable> Cables => cables;

    public abstract bool LinksNetworks { get; }

    // Display name (text at the pointer): the placed item, or the object
    public string DisplayName
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
}
