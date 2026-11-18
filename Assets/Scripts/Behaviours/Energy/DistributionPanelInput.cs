using UnityEngine;

// Input of a distribution panel: a consumer of the upstream network, asking what the panel's circuit needs
// (DistributionPanel.InputDemand, set by the EnergyNetworkManager)
public class DistributionPanelInput : EnergyConsumer
{
    [SerializeField] private DistributionPanel panel;

    public override DistributionPanel Panel => panel != null ? panel : (panel = GetComponentInParent<DistributionPanel>());
    public override string PortId => "input";
    public override float Consumption => Panel != null ? Panel.InputDemand : 0f;
    public override string DisplayName => base.DisplayName + " (entrée)";

    public override string GetInfo()
    {
        return Panel != null ? Panel.GetInfo() : base.GetInfo();
    }
}
