using System;
using UnityEngine;

// Stores energy (Wh): it takes the surplus of the generators of its network and gives what it lacks, within its
// input and output rates (W)
public class EnergyStorage : EnergyDevice
{
    [SerializeField] private EnergyValue capacity = new EnergyValue(1000f);
    [SerializeField] private EnergyValue inputRate = new EnergyValue(500f);
    [SerializeField] private EnergyValue outputRate = new EnergyValue(500f);

    // Energy stored (Wh), kept in the save
    [SerializeField] private EnergyValue stored = new EnergyValue(0f);

    public float Capacity => capacity.Value;
    public float InputRate => inputRate.Value;
    public float OutputRate => outputRate.Value;
    public float Stored => Mathf.Min(stored.Value, Capacity);

    // Power taken (charge) and given (discharge) during the last update (EnergyNetworkManager)
    public float Charge { get; set; }
    public float Discharge { get; set; }

    // Power it can take or give during an update of "hours" hours
    public float MaxCharge(float hours)
    {
        if (stored.infinite || !IsOn) return 0f;

        float room = Capacity - Stored;
        return Mathf.Min(InputRate, float.IsPositiveInfinity(room) ? room : room / hours);
    }

    public float MaxDischarge(float hours)
    {
        if (!IsOn) return 0f;

        return Mathf.Min(OutputRate, stored.infinite ? float.PositiveInfinity : Stored / hours);
    }

    public void Apply(float charge, float discharge, float hours)
    {
        Charge = charge;
        Discharge = discharge;

        if (stored.infinite) return;

        float delta = (charge - discharge) * hours;

        if (float.IsNaN(delta)) return;

        stored.value = Mathf.Clamp(Stored + delta, 0f, Capacity);
    }

    public override string GetInfo()
    {
        float flow = Charge - Discharge;
        string flowText = flow > 0.01f ? " (+" + EnergyValue.Format(flow, "W") + ")"
            : flow < -0.01f ? " (-" + EnergyValue.Format(-flow, "W") + ")"
            : "";

        string storedText = stored.infinite ? "∞" : EnergyValue.Format(Stored, "").Trim();

        return DisplayName + " : " + OffText(true) + storedText + " / " + EnergyValue.Format(Capacity, "Wh") + flowText;
    }

    // ---------- Save ----------

    [Serializable]
    private class State
    {
        public float stored;
        public bool switchedOn = true;
    }

    public override string SaveKey => "energyStorage";

    public override string CaptureState()
    {
        return JsonUtility.ToJson(new State { stored = stored.value, switchedOn = SwitchedOn });
    }

    public override void RestoreState(string json)
    {
        State state = JsonUtility.FromJson<State>(json);

        if (state != null)
        {
            stored.value = state.stored;
            RestoreSwitchedOn(state.switchedOn);
        }
    }
}
