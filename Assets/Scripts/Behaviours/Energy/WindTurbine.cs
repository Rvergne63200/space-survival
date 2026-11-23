using UnityEngine;

// Wind turbine (placed item 33, prefab Prefabs/Environment/Energy/WindTurbine): a generator whose power follows the wind
// of the WeatherSystem. The model (Resources/Models/WindTurbine) is moved by its WindTurbineAnimator, on the model child:
// - On: it deploys (mast, then blades), its nacelle turns to face the wind and its rotor turns with the wind. Off (or held
//   off by a distribution panel): it slows down, folds its blades and lowers its mast
// - Power: "generation" (the max) x a curve of the wind speed: nothing under "cutInSpeed", full from "ratedSpeed"
//   (smooth step between the two). Nothing until it is fully deployed
// - Too much wind (a sand storm): above "cutOutSpeed", it folds to protect itself, and deploys again once the wind is
//   back under "cutOutSpeed" - "resumeMargin". Not saved: decided again from the wind at the reload
// - Loaded with the world while on, it deploys again (the animator has no pose without animation)
public class WindTurbine : EnergyGenerator
{
    // Wind speeds (m/s)
    [SerializeField] private float cutInSpeed = 3f;
    [SerializeField] private float ratedSpeed = 12f;
    [SerializeField] private float cutOutSpeed = 20f;
    [SerializeField] private float resumeMargin = 3f;
    // Speed of the rotor animation at "ratedSpeed" (1: a turn in 2 s), proportional to the wind, at most 4
    [SerializeField] private float rotorSpeedAtRated = 1.5f;

    private WindTurbineAnimator turbineAnimator;
    private float factor;
    private bool protecting;
    // State last sent to the animator (null: nothing sent yet)
    private bool? deployed;

    // Power it can give right now (the max x the wind)
    public override float Generation => base.Generation * factor;

    // Folded because of the wind
    public bool Protecting => protecting;

    private void Awake()
    {
        turbineAnimator = GetComponentInChildren<WindTurbineAnimator>();

        if (turbineAnimator == null) Debug.LogWarning(name + ": no WindTurbineAnimator on the model, the wind turbine gives nothing", this);
    }

    private static float Wind => WeatherSystem.Instance != null ? WeatherSystem.Instance.WindSpeed : 0f;

    private void Update()
    {
        if (turbineAnimator == null)
        {
            factor = 0f;
            return;
        }

        float wind = Wind;

        if (wind > cutOutSpeed) protecting = true;
        else if (wind < cutOutSpeed - resumeMargin) protecting = false;

        bool deploy = IsOn && !protecting;

        if (deployed != deploy)
        {
            deployed = deploy;

            if (deploy) turbineAnimator.Deploy();
            else turbineAnimator.Fold();
        }

        if (WeatherSystem.Instance != null) turbineAnimator.SetWindDirection(WeatherSystem.Instance.WindFrom);

        turbineAnimator.WindStrength = Mathf.Min(4f, rotorSpeedAtRated * wind / Mathf.Max(ratedSpeed, 0.01f));

        factor = deploy && turbineAnimator.IsDeployed ? WindFactor(wind) : 0f;
    }

    // 0 to 1: nothing under the cut in speed, full from the rated speed
    private float WindFactor(float wind)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(cutInSpeed, ratedSpeed, wind));
    }

    public override string GetInfo()
    {
        string state = protecting && IsOn ? " (repliée : vent trop fort)" : "";

        return DisplayName + " : " + OffText(true) + EnergyValue.Format(Output, "W") + " / " + EnergyValue.Format(Generation, "W")
            + " (max " + EnergyValue.Format(base.Generation, "W") + "), vent " + Mathf.RoundToInt(Wind) + " m/s" + state;
    }
}
