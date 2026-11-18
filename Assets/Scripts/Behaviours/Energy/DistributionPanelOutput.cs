using UnityEngine;

// Output of a distribution panel: a generator for the panel's circuit, giving what the input receives
// (DistributionPanel.OutputSupply, set by the EnergyNetworkManager). It is served after the other generators of the
// circuit, and nothing goes back upstream
public class DistributionPanelOutput : EnergyGenerator
{
    [SerializeField] private DistributionPanel panel;

    public override DistributionPanel Panel => panel != null ? panel : (panel = GetComponentInParent<DistributionPanel>());
    // Saved with the cables. Empty: "output". Set when an object holds several panels (building and its outlet)
    [SerializeField] private string portId;

    public override string PortId => string.IsNullOrEmpty(portId) ? "output" : portId;
    public override bool CanSwitch => false;
    public override float Generation => Panel != null ? Panel.OutputSupply : 0f;
    public override string DisplayName => base.DisplayName + " (sortie)";

    public override string GetInfo()
    {
        return Panel != null ? Panel.GetInfo() : base.GetInfo();
    }
}
