using UnityEngine;

// Output socket of a building (orange port, on the power socket of the prefab): a hidden distribution panel inside the
// building, whose input is linked without cable to the inside of the building (EnergyNetworkManager) and whose output
// is the port cabled outside (another building, a machine...). It only sends the power "in excess": all its demand is
// optional (OptionalDemand), like the charge of batteries, served by the surplus upstream (within the power limit of
// the building). No settings nor icon of its own: the building (BuildingPowerPanel) is managed
public class BuildingOutlet : DistributionPanel
{
    public override bool OptionalDemand => true;

    public override string SaveKey => "buildingOutlet";

    public override BuildingPowerPanel Building => GetComponentInParent<BuildingPowerPanel>();
}
