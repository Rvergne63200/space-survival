using UnityEngine;
using UnityEngine.Events;

// Uses power from its network (W). Powered when its network gives all the power it asks for: a network that cannot
// give the power to all its consumers trips (EnergyNetworkManager)
public class EnergyConsumer : EnergyDevice
{
    [SerializeField] private EnergyValue consumption = new EnergyValue(100f);
    public float Consumption => consumption.Value;

    public override bool LinksNetworks => true;

    public bool IsPowered { get; private set; }

    // Why it is not powered (text at the pointer)
    public EnergyNetworkManager.NetworkState State { get; private set; } = EnergyNetworkManager.NetworkState.NotLinked;

    public UnityEvent<bool> ev_OnPoweredChanged = new UnityEvent<bool>();

    // Called by the EnergyNetworkManager
    public void SetState(EnergyNetworkManager.NetworkState state)
    {
        State = state;

        bool powered = state == EnergyNetworkManager.NetworkState.Powered;

        if (powered == IsPowered) return;

        IsPowered = powered;
        ev_OnPoweredChanged.Invoke(powered);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        SetState(EnergyNetworkManager.NetworkState.NotLinked);
    }

    public override string GetInfo()
    {
        string state = State switch
        {
            EnergyNetworkManager.NetworkState.Powered => "alimentée",
            EnergyNetworkManager.NetworkState.Tripped => "réseau disjoncté",
            _ => "non raccordée",
        };

        return DisplayName + " : " + state + " (" + EnergyValue.Format(Consumption, "W") + ")";
    }
}
