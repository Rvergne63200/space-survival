using UnityEngine;

// Distribution panel built into a building, on the power socket of its prefab (interaction: the same interface as a
// distribution panel, DistributionPanelUI, and the same settings). Ports:
// - input (DistributionPanelInput, green): cabled from outside, it feeds the building
// - output (DistributionPanelOutput, hidden, port "interior"): feeds without cable everything placed inside the building
//   (BuildingInterior, linked by the EnergyNetworkManager). Switched off, the building switches off what it holds
// - output socket (BuildingOutlet, orange): what is cabled after the building, fed with the surplus only
// The inputs of two buildings cannot be cabled together (CablePlacer): buildings are also linked by corridors (F26)
public class BuildingPowerPanel : DistributionPanel
{
    [SerializeField] private BuildingInterior interior;

    public BuildingInterior Interior => interior;

    // Output socket (orange port): what is cabled after the building, fed with the surplus only
    public BuildingOutlet Outlet => GetComponentInChildren<BuildingOutlet>();

    // The two ports that can be cabled: the input (green) and the output socket (orange). The output of this panel
    // feeds the inside without cable
    public override EnergyDevice ClosestPort(Vector3 point)
    {
        EnergyDevice outlet = Outlet != null ? Outlet.Output : null;

        if (Input == null) return outlet;
        if (outlet == null) return Input;

        return Vector3.Distance(point, Input.ConnectionPoint) <= Vector3.Distance(point, outlet.ConnectionPoint) ? (EnergyDevice)Input : outlet;
    }

    // Linked to another building by a finished corridor (its input is then linked to the input of the other building)
    public bool HasCorridor => Corridor.HasBuiltCorridor(GetComponentInParent<BuiltStructure>());

    // The devices of the building itself (its ports) are not "inside" it
    public bool IsPartOfBuilding(EnergyDevice device)
    {
        return device != null && device.transform.IsChildOf(BuildingRoot);
    }

    private Transform BuildingRoot
    {
        get
        {
            BuiltStructure structure = GetComponentInParent<BuiltStructure>();
            return structure != null ? structure.transform : transform.root;
        }
    }
}
