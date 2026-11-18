using UnityEngine;

// Output of a distribution panel: a generator for the panel's circuit, giving what the input receives
// (DistributionPanel.OutputSupply, set by the EnergyNetworkManager). It is served after the other generators of the
// circuit, and nothing goes back upstream
public class DistributionPanelOutput : EnergyGenerator
{
    [SerializeField] private DistributionPanel panel;

    public override DistributionPanel Panel => panel != null ? panel : (panel = GetComponentInParent<DistributionPanel>());
    public override string PortId => "output";
    public override bool CanSwitch => false;
    public override float Generation => Panel != null ? Panel.OutputSupply : 0f;
    public override string DisplayName => base.DisplayName + " (sortie)";

    public override string GetInfo()
    {
        return Panel != null ? Panel.GetInfo() : base.GetInfo();
    }
}
