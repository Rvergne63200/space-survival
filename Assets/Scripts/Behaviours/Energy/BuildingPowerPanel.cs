using UnityEngine;

// Distribution panel built into a building, on the power socket of its prefab (interaction: the same interface as a
// distribution panel, DistributionPanelUI, and the same settings). Its circuit is the inside of the building. Ports:
// - input (DistributionPanelInput, green): cabled from outside, it feeds the building
// - output (DistributionPanelOutput, hidden, port "interior"): linked without cable to everything placed inside the
//   building (BuildingInterior, EnergyNetworkManager)
// - output socket (BuildingOutlet, orange): what is cabled after the building, fed with the surplus only
// Corridors link the inputs of two buildings (Corridor)
public class BuildingPowerPanel : DistributionPanel
{
    [SerializeField] private BuildingInterior interior;

    public BuildingInterior Interior => interior;

    public override BuildingPowerPanel Building => this;

    public BuiltStructure Structure => GetComponentInParent<BuiltStructure>();

    // Output socket (orange port)
    public BuildingOutlet Outlet => GetComponentInChildren<BuildingOutlet>();

    // The two ports that can be cabled: the input (green) and the output socket (orange)
    public override EnergyDevice ClosestPort(Vector3 point)
    {
        EnergyDevice outlet = Outlet != null ? Outlet.Output : null;

        if (Input == null) return outlet;
        if (outlet == null) return Input;

        return Vector3.Distance(point, Input.ConnectionPoint) <= Vector3.Distance(point, outlet.ConnectionPoint) ? (EnergyDevice)Input : outlet;
    }

    // The devices of the building itself (its ports) are not "inside" it
    public bool IsPartOfBuilding(EnergyDevice device)
    {
        Transform root = Structure != null ? Structure.transform : transform.root;
        return device != null && device.transform.IsChildOf(root);
    }
}
