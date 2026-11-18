using UnityEngine;

// Produces power (W) for the networks it is linked to. It always runs at full power and only gives each network what it
// needs (batteries included): the rest is lost
public class EnergyGenerator : EnergyDevice
{
    [SerializeField] private EnergyValue generation = new EnergyValue(1000f);
    public virtual float Generation => generation.Value;

    // What it can give: nothing when it is off
    public float Production => IsOn ? Generation : 0f;

    public override bool LinksNetworks => false;

    // Power actually given to the networks during the last update (EnergyNetworkManager)
    public float Output { get; set; }

    public override string GetInfo()
    {
        return DisplayName + " : " + OffText(false) + EnergyValue.Format(Output, "W") + " / " + EnergyValue.Format(Generation, "W");
    }
}
