using UnityEngine;

// Produces power (W) for the networks it is linked to. It always runs at full power and only gives each network what it
// needs (batteries included): the rest is lost
public class EnergyGenerator : EnergyDevice
{
    [SerializeField] private EnergyValue generation = new EnergyValue(1000f);
    public virtual float Generation => generation.Value;

    public override bool LinksNetworks => false;

    // Power actually given to the networks during the last update (EnergyNetworkManager)
    public float Output { get; set; }

    public override string GetInfo()
    {
        return DisplayName + " : " + EnergyValue.Format(Output, "W") + " / " + EnergyValue.Format(Generation, "W");
    }
}
