using UnityEngine;

// Solar panel (placed item 29): a generator whose power follows the sun. The model (Resources/Models/SolarPanel) is
// moved by its SolarPanelAnimator, on the model child:
// - On: it deploys, then its head turns toward the sun (sun of the DayNightCycle). Off (or held off by a distribution
//   panel): it folds and its head goes back to its rest pose, toward the front of the object
// - Power: "generation" (the max, 1000 W by default) x the light of the sun x the incidence on the cells:
//   - light of the sun: its height over its height at noon, so 0 at night and at sunrise / sunset, full at noon
//   - incidence: cosine between the cells (tilted "panelTilt" degrees toward the front of the head) and the sun.
//     Facing the sun at noon, the cells are square to it: full power. Folded, turned away or still turning: less
//   - deployment: nothing until the panel is fully deployed
// No weather for now
public class SolarPanel : EnergyGenerator
{
    // Tilt of the cells from the horizontal, toward the front of the head, once deployed (the Panel_Deploy clip)
    [SerializeField] private float panelTilt = 30f;

    private SolarPanelAnimator panelAnimator;
    private float factor;
    private bool restored;
    private bool wasOn;
    private bool started;

    // Power it can give right now (the max x the sun)
    public override float Generation => base.Generation * factor;

    private void Awake()
    {
        panelAnimator = GetComponentInChildren<SolarPanelAnimator>();
    }

    private void Start()
    {
        DayNightCycle cycle = DayNightCycle.Instance;
        Light sun = cycle != null ? cycle.Sun : RenderSettings.sun;

        if (panelAnimator != null && sun != null) panelAnimator.SetSun(sun);
    }

    private void Update()
    {
        if (panelAnimator == null)
        {
            factor = 0f;
            return;
        }

        // First frame: loaded with the world, it is already in its pose. Just placed, it deploys
        if (!started)
        {
            started = true;
            wasOn = !IsOn;

            if (restored && IsOn) panelAnimator.SetDeployedImmediate(true);
        }

        if (IsOn != wasOn)
        {
            wasOn = IsOn;

            if (IsOn)
            {
                // Follows the sun again (the sun of SetSun)
                panelAnimator.StopTracking();
                panelAnimator.Deploy();
            }
            else
            {
                panelAnimator.Fold();
                panelAnimator.ReturnToRest();
            }
        }

        factor = IsOn ? SunFactor() : 0f;
    }

    // 0 to 1: light of the sun x incidence on the cells x deployment
    private float SunFactor()
    {
        float deployed = panelAnimator.DeployProgress;

        if (deployed < 1f) return 0f;

        Vector3 toSun;
        float noonHeight;
        DayNightCycle cycle = DayNightCycle.Instance;

        if (cycle != null)
        {
            toSun = cycle.SunDirection;
            noonHeight = cycle.NoonSunHeight;
        }
        else
        {
            if (RenderSettings.sun == null || !RenderSettings.sun.enabled) return 0f;
            toSun = -RenderSettings.sun.transform.forward;
            noonHeight = 1f;
        }

        float light = Mathf.Clamp01(toSun.y / Mathf.Max(noonHeight, 0.01f));

        if (light <= 0f) return 0f;

        float tilt = panelTilt * Mathf.Deg2Rad;
        Vector3 cells = (panelAnimator.PanelUp * Mathf.Cos(tilt) + panelAnimator.PanelFront * Mathf.Sin(tilt)).normalized;
        float incidence = Mathf.Clamp01(Vector3.Dot(cells, toSun.normalized));

        return light * incidence;
    }

    public override string GetInfo()
    {
        return DisplayName + " : " + OffText(false) + EnergyValue.Format(Output, "W") + " / " + EnergyValue.Format(Generation, "W")
            + " (max " + EnergyValue.Format(base.Generation, "W") + ")";
    }

    public override void RestoreState(string json)
    {
        base.RestoreState(json);
        restored = true;
    }
}
