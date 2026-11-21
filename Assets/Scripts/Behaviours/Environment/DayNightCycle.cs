using UnityEngine;

// Day / night cycle of the world (on the "Directional Light" of 0_World, which is the sun). Saved with the game
// (SaveData.time, see SaveManager).
// - Clock: the sun rises at 6:00 and sets at 18:00. The day (6:00 -> 18:00) lasts "dayDuration" real seconds, the night
//   (18:00 -> 6:00) "nightDuration": the clock runs faster at night. A new game starts on day 1 at "startHour"
// - Sun: turns at a constant angular speed per game hour, from the east (sunrise) to the west (sunset), "sunTilt"
//   degrees away from the zenith at noon. Its light fades and turns orange near the horizon
// - Moon: a second directional light (created at runtime), opposite the sun. Each night has its own brightness,
//   drawn from the world seed: most nights are very dark (flashlight needed), some are bright
// - Sky, ambient light and reflections: dimmed from the day values (those of the scene) to the brightness of the night
public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    public const float SunriseHour = 6f;
    public const float SunsetHour = 18f;

    [Header("Clock")]
    // Real seconds from 6:00 to 18:00, and from 18:00 to 6:00
    [SerializeField] private float dayDuration = 900f;
    [SerializeField] private float nightDuration = 600f;
    [SerializeField] private float startHour = 8f;
    // Speed of the clock, for testing (1: normal)
    [SerializeField] private float speed = 1f;
    // Current time, editable in the Inspector during play
    [SerializeField] private int day = 1;
    [Range(0f, 24f)] [SerializeField] private float hour = 8f;

    [Header("Sun")]
    [SerializeField] private Light sun;
    [SerializeField] private float sunIntensity = 1f;
    [SerializeField] private Color horizonColor = new Color(1f, 0.5f, 0.25f);
    // Yaw of the east (direction of the sunrise), and angle between the zenith and the sun at noon
    [SerializeField] private float sunYaw = 0f;
    [Range(0f, 80f)] [SerializeField] private float sunTilt = 30f;

    [Header("Night")]
    [SerializeField] private Color moonColor = new Color(0.55f, 0.65f, 1f);
    // Moon light, ambient light and sky of the darkest and of the brightest nights (fraction of the day ones for the
    // ambient light and the sky)
    [SerializeField] private float darkMoonIntensity = 0.01f;
    [SerializeField] private float brightMoonIntensity = 0.3f;
    [SerializeField] private float darkAmbient = 0.02f;
    [SerializeField] private float brightAmbient = 0.2f;
    [SerializeField] private float darkSky = 0.01f;
    [SerializeField] private float brightSky = 0.12f;
    // The higher, the rarer the bright nights (brightness = random ^ rarity)
    [SerializeField] private float brightNightRarity = 4f;

    private Light moon;
    private Color noonColor;
    private Material sky;
    private Material sceneSky;
    private float skyExposure = 1f;
    private float sceneAmbient = 1f;
    private float sceneReflection = 1f;

    private int brightnessNight = int.MinValue;
    private float nightBrightness;

    public int Day => day;
    public float Hour => hour;
    public bool IsDay => hour >= SunriseHour && hour < SunsetHour;

    // The light of the sun, and the direction toward it (below the horizon at night)
    public Light Sun => sun;
    public Vector3 SunDirection { get; private set; } = Vector3.up;

    // Height of the sun at noon (y of SunDirection, 0 to 1): the highest it goes
    public float NoonSunHeight => Mathf.Cos(sunTilt * Mathf.Deg2Rad);

    // Light of the sun on the ground, 0 (night) to 1 (sun at its highest), for the solar panels and the plants
    public float Sunlight { get; private set; }

    // Brightness of the current (or coming) night, 0 (darkest) to 1 (brightest)
    public float NightBrightness => nightBrightness;

    private void Awake()
    {
        Instance = this;

        if (sun == null) sun = GetComponent<Light>();
        if (sun != null) noonColor = sun.color;

        moon = new GameObject("Moon").AddComponent<Light>();
        moon.transform.SetParent(transform, false);
        moon.type = LightType.Directional;
        moon.color = moonColor;
        moon.shadows = LightShadows.Soft;
        moon.enabled = false;

        // A copy of the sky, to dim it without changing the material asset
        sceneSky = RenderSettings.skybox;

        if (sceneSky != null)
        {
            sky = new Material(sceneSky);
            RenderSettings.skybox = sky;

            if (sky.HasProperty("_Exposure")) skyExposure = sky.GetFloat("_Exposure");
        }

        sceneAmbient = RenderSettings.ambientIntensity;
        sceneReflection = RenderSettings.reflectionIntensity;

        SetTime(1, startHour);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (sky != null)
        {
            if (RenderSettings.skybox == sky) RenderSettings.skybox = sceneSky;
            Destroy(sky);
        }
    }

    private void Update()
    {
        float duration = IsDay ? dayDuration : nightDuration;

        // 12 game hours per half of the cycle
        hour += Time.deltaTime * speed * 12f / Mathf.Max(duration, 0.01f);

        while (hour >= 24f)
        {
            hour -= 24f;
            day++;
        }

        hour = Mathf.Max(hour, 0f);
        Apply();
    }

    public void SetTime(int newDay, float newHour)
    {
        day = Mathf.Max(newDay, 1);
        hour = Mathf.Repeat(newHour, 24f);
        Apply();
    }

    public WorldTimeSaveData Capture() => new WorldTimeSaveData { day = day, hour = hour };

    // A save without time (new game, older save) starts on day 1 at "startHour"
    public void Restore(WorldTimeSaveData data)
    {
        // The world seed is known now
        brightnessNight = int.MinValue;

        if (data == null || data.hour < 0f)
        {
            SetTime(1, startHour);
        }
        else
        {
            SetTime(data.day, data.hour);
        }
    }

    private void Apply()
    {
        // Angle of the sun on its path: 0 at sunrise, 180 at sunset, below the horizon from 180 to 360
        float angle = (hour - SunriseHour) / 12f * Mathf.PI;
        Quaternion yaw = Quaternion.Euler(0f, sunYaw, 0f);
        Vector3 east = yaw * Vector3.right;
        Vector3 noon = Mathf.Cos(sunTilt * Mathf.Deg2Rad) * Vector3.up + Mathf.Sin(sunTilt * Mathf.Deg2Rad) * (yaw * Vector3.back);
        Vector3 toSun = Mathf.Cos(angle) * east + Mathf.Sin(angle) * noon;
        SunDirection = toSun;

        float sunHeight = toSun.y;
        float moonHeight = -toSun.y;

        UpdateNightBrightness();

        // Sun: full a little above the horizon, orange near it
        Sunlight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.25f, sunHeight));

        if (sun != null)
        {
            sun.transform.rotation = Quaternion.LookRotation(-toSun);
            sun.intensity = sunIntensity * Sunlight;
            sun.color = Color.Lerp(horizonColor, noonColor, Mathf.InverseLerp(0f, 0.4f, sunHeight));
            sun.enabled = sun.intensity > 0.0001f;
        }

        // Moon: opposite the sun, as bright as the night
        float moonLight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.25f, moonHeight));
        moon.transform.rotation = Quaternion.LookRotation(toSun);
        moon.color = moonColor;
        moon.intensity = Mathf.Lerp(darkMoonIntensity, brightMoonIntensity, nightBrightness) * moonLight;
        moon.enabled = moon.intensity > 0.0001f && (sun == null || !sun.enabled);

        // Twilight: the sky and the ambient light fade from a little above to a little below the horizon
        float daylight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.15f, 0.2f, sunHeight));
        float nightAmbient = Mathf.Lerp(darkAmbient, brightAmbient, nightBrightness);
        float nightSky = Mathf.Lerp(darkSky, brightSky, nightBrightness);

        RenderSettings.ambientIntensity = sceneAmbient * Mathf.Lerp(nightAmbient, 1f, daylight);
        RenderSettings.reflectionIntensity = sceneReflection * Mathf.Lerp(nightAmbient, 1f, daylight);

        if (sky != null && sky.HasProperty("_Exposure"))
        {
            sky.SetFloat("_Exposure", skyExposure * Mathf.Lerp(nightSky, 1f, daylight));
        }
    }

    // A night starts at sunset: before sunrise, it is still the night of the previous day
    private void UpdateNightBrightness()
    {
        int night = hour < SunriseHour ? day - 1 : day;

        if (night == brightnessNight) return;

        brightnessNight = night;

        int seed = ChunkManager.Instance != null ? ChunkManager.Instance.Seed : 0;
        System.Random random = new System.Random(unchecked(seed * 486187739 + night * 16777619));

        nightBrightness = Mathf.Pow((float)random.NextDouble(), Mathf.Max(brightNightRarity, 0.01f));
    }
}
