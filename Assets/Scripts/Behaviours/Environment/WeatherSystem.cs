using UnityEngine;

// Weather of the world (on the "Weather" object of 0_World, next to the DayNightCycle): the wind and the sand storms.
// Nothing is saved: the weather is drawn from the world seed and the time of the world (DayNightCycle), so the same time
// of the same world always has the same weather, and a reloaded game finds the weather it left.
// - Wind: its speed (m/s) and the direction it comes from drift slowly (Perlin noise over the game hours), from "calmWind"
//   to "strongWind". It rises before a storm and reaches "stormWind" during it, coming from the side of the storm.
//   Used by the wind turbines
// - Storms: each day from day 2 has a "stormChance" chance of a storm, at a random hour, lasting "minStormHours" to
//   "maxStormHours" game hours (a storm may go on past midnight). It is announced "warningHours" before (message to the
//   player, the haze thickens and the sky darkens a little), grows in "rampHours", then calms down in "rampHours"
// - During a storm: the solar panels keep "stormSolar" of their power, the sun, the sky and the ambient light dim, a
//   thick brown-orange fog cuts the view to a few meters and sand blows around the camera (outside only: inside a
//   building, the fog is lighter and there is no sand)
// - Test: "testStorm" in the Inspector (or the context menu) starts a storm now, with its warning. Not saved
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    public enum WeatherState
    {
        Clear,
        StormComing,
        Storm
    }

    [Header("Wind (m/s)")]
    [SerializeField] private float calmWind = 1f;
    [SerializeField] private float strongWind = 14f;
    [SerializeField] private float stormWind = 26f;
    // Game hours over which the wind speed and its direction change
    [SerializeField] private float windPeriod = 5f;
    [SerializeField] private float directionPeriod = 16f;
    // Share of the storm wind already blowing at the end of the warning
    [Range(0f, 1f)] [SerializeField] private float warningWind = 0.35f;

    [Header("Storms (game hours)")]
    [Range(0f, 1f)] [SerializeField] private float stormChance = 0.3f;
    [SerializeField] private float minStormHours = 1.5f;
    [SerializeField] private float maxStormHours = 4f;
    [SerializeField] private float rampHours = 0.5f;
    [SerializeField] private float warningHours = 1.5f;
    // Share of the power of the solar panels, of the sun light, of the sky and of the ambient light kept at the height of
    // a storm
    [Range(0f, 1f)] [SerializeField] private float stormSolar = 0.1f;
    [Range(0f, 1f)] [SerializeField] private float stormSunlight = 0.15f;
    [Range(0f, 1f)] [SerializeField] private float stormSky = 0.35f;
    [Range(0f, 1f)] [SerializeField] private float stormAmbient = 0.6f;
    // Share of the storm (fog, sky) already there at the end of the warning
    [Range(0f, 1f)] [SerializeField] private float warningLook = 0.15f;

    [Header("Fog (exponential squared)")]
    // Light dust haze of the clear days, and fog of the storm (0.07: almost nothing seen past 30 m)
    [SerializeField] private float clearFogDensity = 0.001f;
    [SerializeField] private float stormFogDensity = 0.07f;
    [SerializeField] private Color hazeColor = new Color(0.79f, 0.6f, 0.45f);
    [SerializeField] private Color stormColor = new Color(0.55f, 0.33f, 0.18f);
    // Share of the storm fog kept inside a building
    [Range(0f, 1f)] [SerializeField] private float indoorFog = 0.2f;

    [Header("Sky (procedural skybox)")]
    [SerializeField] private float stormAtmosphere = 3f;

    [Header("Blowing sand")]
    // "Legacy Shaders/Particles/Alpha Blended" (set here so that it is included in the builds)
    [SerializeField] private Shader dustShader;
    [SerializeField] private float dustRate = 900f;
    // Half size of the box around the camera where the sand appears
    [SerializeField] private float dustRadius = 10f;
    [SerializeField] private Color dustColor = new Color(0.85f, 0.62f, 0.4f, 0.55f);

    [Header("Test")]
    [SerializeField] private bool testStorm;

    // Read only, shown in the Inspector during play
    [Header("Now")]
    [SerializeField] private WeatherState state;
    [SerializeField] private float windSpeed;
    [Range(0f, 1f)] [SerializeField] private float storm;

    private struct Storm
    {
        public bool Exists;
        public double Start;
        public double End;
        public float FromAngle;
    }

    // Storm started from the Inspector (absolute game hours)
    private Storm testStormData;

    // Warning: 0 (not coming) to 1 (starting now)
    private float warning;
    private float windAngle;
    // Direction the wind of the current (or coming) storm comes from (NaN: no storm)
    private float stormFromAngle = float.NaN;
    private double lastTime = double.NaN;
    private bool notified;
    private float indoorBlend;

    // Scene values, given back when the world is unloaded
    private bool sceneFog;
    private FogMode sceneFogMode;
    private float sceneFogDensity;
    private Color sceneFogColor;

    private Material sky;
    private Color skyTint;
    private float skyAtmosphere;

    private ParticleSystem dust;
    private ParticleSystem.EmissionModule dustEmission;
    private ParticleSystem.VelocityOverLifetimeModule dustVelocity;
    private ParticleSystem.MainModule dustMain;
    private Material dustMaterial;
    private Texture2D dustTexture;

    public WeatherState State => state;
    public bool IsStorm => state == WeatherState.Storm;
    public bool IsStormComing => state == WeatherState.StormComing;

    // 0 (clear) to 1 (height of a storm)
    public float StormIntensity => storm;

    // Wind speed (m/s), direction (world, horizontal, normalized) it comes from, and velocity of the air (m/s)
    public float WindSpeed => windSpeed;
    public Vector3 WindFrom => Quaternion.Euler(0f, windAngle, 0f) * Vector3.forward;
    public Vector3 WindVelocity => -WindFrom * windSpeed;

    // Multipliers of the weather (1: clear): power of the solar panels, sun light, sky exposure, ambient light
    public float SolarFactor => Mathf.Lerp(1f, stormSolar, storm);
    public float SunFactor => Mathf.Lerp(1f, stormSunlight, Mathf.Max(storm, warning * warningLook));
    public float SkyFactor => Mathf.Lerp(1f, stormSky, Mathf.Max(storm, warning * warningLook));
    public float AmbientFactor => Mathf.Lerp(1f, stormAmbient, storm);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        sceneFog = RenderSettings.fog;
        sceneFogMode = RenderSettings.fogMode;
        sceneFogDensity = RenderSettings.fogDensity;
        sceneFogColor = RenderSettings.fogColor;

        CreateDust();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;

        RenderSettings.fog = sceneFog;
        RenderSettings.fogMode = sceneFogMode;
        RenderSettings.fogDensity = sceneFogDensity;
        RenderSettings.fogColor = sceneFogColor;

        if (sky != null)
        {
            if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", skyTint);
            if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", skyAtmosphere);
        }

        if (dustMaterial != null) Destroy(dustMaterial);
        if (dustTexture != null) Destroy(dustTexture);
    }

    [ContextMenu("Start a storm now")]
    private void StartTestStorm()
    {
        double now = CurrentTime();
        double start = now + warningHours;

        testStormData = new Storm
        {
            Exists = true,
            Start = start,
            End = start + maxStormHours,
            FromAngle = windAngle
        };
    }

    private void Update()
    {
        if (testStorm)
        {
            testStorm = false;
            StartTestStorm();
        }

        double now = CurrentTime();

        // A jump in time (save loaded, hour changed in the Inspector): the new weather, without messages
        bool jumped = double.IsNaN(lastTime) || now < lastTime || now - lastTime > 0.5;
        lastTime = now;

        UpdateStorm(now);
        UpdateWind(now);
        UpdateState(jumped);
        UpdateLook();
    }

    private void LateUpdate() => UpdateDust();

    // Absolute game hours since the start of day 1
    private static double CurrentTime()
    {
        DayNightCycle cycle = DayNightCycle.Instance;

        if (cycle == null) return 8.0;

        return (cycle.Day - 1) * 24.0 + cycle.Hour;
    }

    private static int Seed => ChunkManager.Instance != null ? ChunkManager.Instance.Seed : 0;

    // ---------- Storms ----------

    // The storm of a day, if it has one (none on day 1)
    private Storm StormOfDay(int day)
    {
        if (day < 2) return new Storm();

        System.Random random = new System.Random(unchecked(Seed * 73856093 + day * 19349663 + 7));

        if (random.NextDouble() >= stormChance) return new Storm();

        double start = (day - 1) * 24.0 + random.NextDouble() * 24.0;
        double duration = Mathf.Lerp(Mathf.Max(minStormHours, 2f * rampHours), Mathf.Max(maxStormHours, minStormHours), (float)random.NextDouble());

        return new Storm
        {
            Exists = true,
            Start = start,
            End = start + duration,
            FromAngle = (float)random.NextDouble() * 360f
        };
    }

    // Intensity of the storms around now, and the warning of the next one
    private void UpdateStorm(double now)
    {
        int day = (int)System.Math.Floor(now / 24.0) + 1;

        storm = 0f;
        warning = 0f;
        float stormAngle = float.NaN;
        float strongest = 0f;

        for (int d = day - 1; d <= day + 1; d++)
        {
            Apply(StormOfDay(d), now, ref strongest, ref stormAngle);
        }

        Apply(testStormData, now, ref strongest, ref stormAngle);

        stormFromAngle = stormAngle;
    }

    private void Apply(Storm s, double now, ref float strongest, ref float angle)
    {
        if (!s.Exists) return;

        float ramp = Mathf.Max(rampHours, 0.01f);
        float intensity = 0f;

        if (now >= s.Start && now < s.End)
        {
            float rise = Mathf.Clamp01((float)(now - s.Start) / ramp);
            float fall = Mathf.Clamp01((float)(s.End - now) / ramp);
            intensity = Mathf.SmoothStep(0f, 1f, Mathf.Min(rise, fall));
        }
        else if (now < s.Start && now >= s.Start - warningHours)
        {
            warning = Mathf.Max(warning, 1f - (float)(s.Start - now) / Mathf.Max(warningHours, 0.01f));
        }
        else
        {
            return;
        }

        storm = Mathf.Max(storm, intensity);

        // The wind comes from the side of the strongest (or the coming) storm
        float weight = Mathf.Max(intensity, warning * 0.5f);

        if (weight >= strongest)
        {
            strongest = weight;
            angle = s.FromAngle;
        }
    }

    // ---------- Wind ----------

    private void UpdateWind(double now)
    {
        float offset = Mathf.Repeat(Seed, 10000f) * 0.731f + 100f;
        float speedNoise = Mathf.Clamp01(Mathf.PerlinNoise(offset, (float)(now / Mathf.Max(windPeriod, 0.01f) % 10000.0)));
        float angleNoise = Mathf.PerlinNoise((float)(now / Mathf.Max(directionPeriod, 0.01f) % 10000.0), offset);

        float speed = Mathf.Lerp(calmWind, strongWind, Mathf.SmoothStep(0f, 1f, speedNoise));
        float stormShare = Mathf.Max(storm, warning * warningWind);
        windSpeed = Mathf.Lerp(speed, stormWind, stormShare);

        float angle = angleNoise * 720f;

        if (!float.IsNaN(stormFromAngle))
        {
            angle = Mathf.LerpAngle(angle, stormFromAngle, Mathf.Max(storm, warning));
        }

        windAngle = Mathf.Repeat(angle, 360f);
    }

    // ---------- Messages ----------

    private void UpdateState(bool jumped)
    {
        WeatherState now = storm > 0f ? WeatherState.Storm : warning > 0f ? WeatherState.StormComing : WeatherState.Clear;

        if (now == state && notified) return;

        WeatherState before = state;
        state = now;

        // First frame or jump: no message for the weather already there
        if (!notified || jumped)
        {
            notified = true;
            return;
        }

        if (now == WeatherState.StormComing)
        {
            NotificationUI.Show("Tempête de sable en approche : mettez-vous à l'abri", NotificationType.Warning);
        }
        else if (now == WeatherState.Storm)
        {
            NotificationUI.Show("La tempête de sable arrive", NotificationType.Warning);
        }
        else if (before == WeatherState.Storm)
        {
            NotificationUI.Show("La tempête de sable s'est calmée");
        }
    }

    // ---------- Look ----------

    private bool CameraIndoor(out Camera camera)
    {
        camera = Camera.main;

        return camera != null && BuildingInterior.AnyContains(camera.transform.position);
    }

    private void UpdateLook()
    {
        DayNightCycle cycle = DayNightCycle.Instance;
        float light = cycle != null ? cycle.LightLevel : 1f;
        float look = Mathf.Max(storm, warning * warningLook);

        indoorBlend = Mathf.MoveTowards(indoorBlend, CameraIndoor(out _) ? 1f : 0f, Time.deltaTime * 2f);

        float stormDensity = Mathf.Lerp(stormFogDensity, stormFogDensity * indoorFog, indoorBlend);
        float density = Mathf.Lerp(clearFogDensity, stormDensity, look * look);
        Color color = Color.Lerp(hazeColor, stormColor, look) * light * Mathf.Lerp(1f, AmbientFactor, look);
        color.a = 1f;

        RenderSettings.fog = density > 0.00001f;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = density;
        RenderSettings.fogColor = color;

        UpdateSky(look);
    }

    // The sky turns to the color of the dust (its exposure is lowered by the DayNightCycle, with SkyFactor)
    private void UpdateSky(float look)
    {
        if (sky == null)
        {
            DayNightCycle cycle = DayNightCycle.Instance;

            if (cycle == null || cycle.Sky == null) return;

            sky = cycle.Sky;
            if (sky.HasProperty("_SkyTint")) skyTint = sky.GetColor("_SkyTint");
            if (sky.HasProperty("_AtmosphereThickness")) skyAtmosphere = sky.GetFloat("_AtmosphereThickness");
        }

        if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", Color.Lerp(skyTint, stormColor, look));
        if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", Mathf.Lerp(skyAtmosphere, stormAtmosphere, look));
    }

    // ---------- Blowing sand ----------

    // Streaks of sand around the camera, carried by the wind (world space)
    private void CreateDust()
    {
        Shader shader = dustShader != null ? dustShader : Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        if (shader == null)
        {
            Debug.LogWarning("WeatherSystem: no particle shader (dustShader), no blowing sand during the storms", this);
            return;
        }

        // A soft round dot
        const int size = 32;
        dustTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        dustTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                dustTexture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d) * Mathf.Clamp01(1f - d)));
            }
        }

        dustTexture.Apply();

        dustMaterial = new Material(shader);
        dustMaterial.mainTexture = dustTexture;
        if (dustMaterial.HasProperty("_TintColor")) dustMaterial.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));

        GameObject dustObject = new GameObject("Blowing sand");
        dustObject.transform.SetParent(transform, false);
        dust = dustObject.AddComponent<ParticleSystem>();
        dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        dustMain = dust.main;
        dustMain.loop = true;
        dustMain.simulationSpace = ParticleSystemSimulationSpace.World;
        dustMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
        dustMain.startSpeed = 0f;
        dustMain.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.14f);
        dustMain.startColor = dustColor;
        dustMain.maxParticles = 3000;
        dustMain.playOnAwake = false;

        dustEmission = dust.emission;
        dustEmission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = dust.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = Vector3.one * dustRadius * 2f;

        dustVelocity = dust.velocityOverLifetime;
        dustVelocity.enabled = true;
        dustVelocity.space = ParticleSystemSimulationSpace.World;

        ParticleSystem.ColorOverLifetimeModule fade = dust.colorOverLifetime;
        fade.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;

        ParticleSystemRenderer particleRenderer = dustObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        particleRenderer.velocityScale = 0.04f;
        particleRenderer.lengthScale = 2f;
        particleRenderer.sharedMaterial = dustMaterial;
        particleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        particleRenderer.receiveShadows = false;

        dust.Play();
    }

    private void UpdateDust()
    {
        if (dust == null) return;

        bool indoor = CameraIndoor(out Camera camera);
        float amount = camera != null && !indoor ? storm * storm : 0f;

        dustEmission.rateOverTime = dustRate * amount;

        if (camera == null) return;

        // Centered on the camera, a little upwind so that the sand crosses the view
        Vector3 velocity = WindVelocity;
        dust.transform.position = camera.transform.position - velocity.normalized * dustRadius * 0.5f;

        dustVelocity.x = new ParticleSystem.MinMaxCurve(velocity.x * 0.8f, velocity.x * 1.2f);
        dustVelocity.y = new ParticleSystem.MinMaxCurve(-1f, 1.5f);
        dustVelocity.z = new ParticleSystem.MinMaxCurve(velocity.z * 0.8f, velocity.z * 1.2f);

        DayNightCycle cycle = DayNightCycle.Instance;
        float light = cycle != null ? Mathf.Max(cycle.LightLevel, 0.05f) : 1f;
        Color color = dustColor * light;
        color.a = dustColor.a;
        dustMain.startColor = color;
    }
}
