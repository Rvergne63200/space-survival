using System;
using UnityEngine;

// Distribution panel: everything linked after its output (DistributionPanelOutput) is its circuit, fed with what its
// input (DistributionPanelInput) receives from the upstream network. One way only: nothing goes back upstream.
// Seen from upstream, the whole circuit is a single consumer asking what the circuit lacks once its own generators
// have given (EnergyNetworkManager). Settings, in its interface (DistributionPanelUI, interaction key):
// - on/off: off, it feeds nothing and switches off everything behind it (their own setting is kept)
// - power limit at 0: cuts the circuit (the former "cut" switch)
// - power limit: if the circuit asks more, only the panel trips (fuse), it is rearmed once the demand is under the limit
// - load shedding: cut when the upstream network lacks power (lowest priority first, rearmed once upstream could give
//   it again), or when the upstream batteries are under a threshold (rearmed 5 % above it)
// - delayed start: seconds to wait before feeding the circuit again after a cut
public class DistributionPanel : MonoBehaviour, IInterractable, ISaveableComponent
{
    public enum PanelState { Starting, On, Off, NoInput, Fused, Shed, Loop }
    public enum SheddingMode { None, Shortage, BatteryBelow }

    private const float Epsilon = 0.001f;
    private const float BatteryHysteresis = 5f;
    private const float ShedRearmDelay = 3f;

    [SerializeField] private bool switchedOn = true;
    [SerializeField] private EnergyValue powerLimit = new EnergyValue(0f, true);
    [SerializeField] private SheddingMode shedding = SheddingMode.None;
    [Range(0f, 100f)] [SerializeField] private float batteryThreshold = 20f;
    [SerializeField] private int priority;
    [SerializeField] private float startDelay;

    private DistributionPanelInput input;
    private DistributionPanelOutput output;

    public DistributionPanelInput Input => input != null ? input : (input = GetComponentInChildren<DistributionPanelInput>());
    public DistributionPanelOutput Output => output != null ? output : (output = GetComponentInChildren<DistributionPanelOutput>());

    // Its own on/off setting. Off, it feeds nothing and everything in its circuit is off (EnergyNetworkManager), their own
    // setting being kept for when it is switched on again
    public bool SwitchedOn => switchedOn;

    // Held off by a panel above it (its input is in the circuit of a switched off panel): its setting cannot be changed
    public bool ForcedOff => Input != null && Input.ForcedOff;
    public bool IsOn => switchedOn && !ForcedOff;
    public EnergyValue PowerLimit => powerLimit;
    public SheddingMode Shedding => shedding;
    public float BatteryThreshold => batteryThreshold;
    public int Priority => priority;
    public float StartDelay => startDelay;

    public PanelState State { get; private set; } = PanelState.Starting;
    public bool IsFeeding => State == PanelState.On;

    // Set by the EnergyNetworkManager: what the consumers of the circuit need (W, required: a lack upstream trips or
    // sheds), what its batteries could take (W, optional: only the upstream surplus), what the input asks upstream,
    // the room left for the charge, and what the output gives
    public float Request { get; set; }
    public float ChargeRequest { get; set; }
    public float InputDemand { get; set; }
    public float ChargeRoom { get; set; }
    public float OutputSupply { get; set; }

    // Given to the circuit during the last update (W), part of it to charge its batteries (W), and in total (Wh, saved)
    public float Supplied { get; private set; }
    public float ChargeSupplied { get; set; }
    public float ConsumedEnergy { get; private set; }

    private float timer;
    private bool shedByBattery;

    public EnergyDevice ClosestPort(Vector3 point)
    {
        if (Input == null) return Output;
        if (Output == null) return Input;

        return Vector3.Distance(point, Input.ConnectionPoint) <= Vector3.Distance(point, Output.ConnectionPoint) ? (EnergyDevice)Input : Output;
    }

    // ---------- Settings (DistributionPanelUI) ----------

    public void SetSwitchedOn(bool on)
    {
        if (!ForcedOff) switchedOn = on;
    }
    public void SetPowerLimit(float watts, bool infinite) => powerLimit = new EnergyValue(Mathf.Max(0f, watts), infinite);
    public void SetShedding(SheddingMode mode) => shedding = mode;
    public void SetBatteryThreshold(float percent) => batteryThreshold = Mathf.Clamp(percent, 0f, 100f);
    public void SetPriority(int value) => priority = value;
    public void SetStartDelay(float seconds) => startDelay = Mathf.Max(0f, seconds);

    // ---------- Update (EnergyNetworkManager) ----------

    // Before the distribution. batteryRatio: charge of the upstream batteries (0 to 1, 1 without battery).
    // loop: the output feeds the network of the input
    public void BeginUpdate(float deltaTime, float batteryRatio, bool loop)
    {
        if (!IsOn)
        {
            State = PanelState.Off;
            return;
        }

        if (loop)
        {
            State = PanelState.Loop;
            return;
        }

        if (State == PanelState.Off || State == PanelState.Loop)
        {
            Restart();
        }

        // Fuse: an infinite request only passes an infinite limit
        if (Request > powerLimit.Value + Epsilon)
        {
            State = PanelState.Fused;
            return;
        }

        if (State == PanelState.Fused)
        {
            Restart();
        }

        if (State == PanelState.Shed && shedByBattery)
        {
            if (shedding == SheddingMode.BatteryBelow && batteryRatio * 100f < batteryThreshold + BatteryHysteresis) return;

            Restart();
        }
        else if (shedding == SheddingMode.BatteryBelow && State != PanelState.Shed && batteryRatio * 100f < batteryThreshold)
        {
            State = PanelState.Shed;
            shedByBattery = true;
            return;
        }

        if (State == PanelState.Starting)
        {
            timer += deltaTime;

            if (timer >= startDelay)
            {
                State = PanelState.On;
            }
        }
    }

    // The upstream network cannot feed it (not linked, tripped)
    public void SetNoInput()
    {
        State = PanelState.NoInput;
    }

    // Load shedding: the upstream network lacks power
    public void Shed()
    {
        State = PanelState.Shed;
        shedByBattery = false;
        timer = 0f;
    }

    // After the distribution. upstreamAvailable: what the upstream network could still give
    public void EndUpdate(float deltaTime, float hours, bool inputPowered, float upstreamAvailable)
    {
        Supplied = Output != null ? Output.Output : 0f;

        if (!float.IsInfinity(Supplied))
        {
            ConsumedEnergy += Supplied * hours;
        }

        // Only once upstream could feed it: otherwise it would trip its input network again and again
        if (State == PanelState.NoInput && inputPowered && upstreamAvailable + Epsilon >= Request)
        {
            Restart();
        }
        else if (State == PanelState.Shed && !shedByBattery)
        {
            if (inputPowered && upstreamAvailable + Epsilon >= Request)
            {
                timer += deltaTime;

                if (timer >= ShedRearmDelay)
                {
                    Restart();
                }
            }
            else
            {
                timer = 0f;
            }
        }
    }

    private void Restart()
    {
        State = PanelState.Starting;
        timer = 0f;
    }

    public string StateText
    {
        get
        {
            switch (State)
            {
                case PanelState.On: return "en marche";
                case PanelState.Off: return ForcedOff ? "éteint par le tableau du dessus" : "éteint";
                case PanelState.NoInput: return "pas d'alimentation en entrée";
                case PanelState.Fused: return "sauté (demande au-dessus de la limite)";
                case PanelState.Shed: return shedByBattery ? "délesté (batteries en amont trop basses)" : "délesté (manque d'énergie en amont)";
                case PanelState.Loop: return "la sortie alimente sa propre entrée";
                default:
                    float remaining = Mathf.Max(0f, startDelay - timer);
                    return remaining > 0.05f ? "démarrage dans " + remaining.ToString("0.0") + " s" : "démarrage";
            }
        }
    }

    public string DisplayName
    {
        get
        {
            PlacedObject placed = GetComponent<PlacedObject>();
            return placed != null && placed.Data != null ? placed.Data.Name : gameObject.name;
        }
    }

    // ---------- Interaction ----------

    public void Interract(GameObject interractor)
    {
        if (DistributionPanelUI.Instance != null)
        {
            DistributionPanelUI.Instance.Open(this);
        }
    }

    public string GetInfo()
    {
        string text = DisplayName + " : " + StateText + " (" + EnergyValue.Format(Supplied, "W") + " / " + EnergyValue.Format(Request, "W");

        if (ChargeSupplied > 0.01f)
        {
            text += ", dont " + EnergyValue.Format(ChargeSupplied, "W") + " en recharge";
        }

        return text + ")";
    }

    // ---------- Save ----------

    [Serializable]
    private class SaveState
    {
        public bool switchedOn;
        public float powerLimit;
        public bool infiniteLimit;
        public int shedding;
        public float batteryThreshold;
        public int priority;
        public float startDelay;
        public float consumedEnergy;
    }

    public string SaveKey => "distributionPanel";

    public string CaptureState()
    {
        return JsonUtility.ToJson(new SaveState
        {
            switchedOn = switchedOn,
            powerLimit = powerLimit.value,
            infiniteLimit = powerLimit.infinite,
            shedding = (int)shedding,
            batteryThreshold = batteryThreshold,
            priority = priority,
            startDelay = startDelay,
            consumedEnergy = ConsumedEnergy
        });
    }

    public void RestoreState(string json)
    {
        SaveState state = JsonUtility.FromJson<SaveState>(json);

        if (state == null) return;

        switchedOn = state.switchedOn;
        powerLimit = new EnergyValue(state.powerLimit, state.infiniteLimit);
        shedding = (SheddingMode)state.shedding;
        batteryThreshold = state.batteryThreshold;
        priority = state.priority;
        startDelay = state.startDelay;
        ConsumedEnergy = state.consumedEnergy;
    }
}
