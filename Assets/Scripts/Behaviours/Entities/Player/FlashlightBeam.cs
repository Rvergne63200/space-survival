using UnityEngine;

// Light of the flashlight (FlashlightAction), child of the camera, created at the first switch on:
// - Beam: a spot held a little to the right and below the eyes (the shadows of the objects are visible, the light
//   does not come from the eyes), aimed like the view. Its cookie (generated) gives a bright hotspot, a faint ring of
//   the reflector and a wide dim spill, instead of a uniform disc
public class FlashlightBeam : MonoBehaviour
{
    private const int CookieSize = 256;

    private Light spot;
    private Texture2D cookie;

    private float cookieHotspot = -1f;
    private float cookieSpill = -1f;

    public static FlashlightBeam Create(Transform view)
    {
        GameObject beamObject = new GameObject("Flashlight");
        beamObject.transform.SetParent(view, false);

        FlashlightBeam beam = beamObject.AddComponent<FlashlightBeam>();

        beam.spot = beamObject.AddComponent<Light>();
        beam.spot.type = LightType.Spot;
        beam.spot.shadows = LightShadows.Soft;
        beam.spot.shadowNearPlane = 0.1f;

        return beam;
    }

    private void OnDestroy()
    {
        if (cookie != null) Destroy(cookie);
    }

    public void Configure(FlashlightAction settings)
    {
        spot.color = settings.color;
        spot.intensity = settings.intensity;
        spot.range = settings.range;
        spot.spotAngle = settings.spotAngle;

        // Held in the hand: placed from the eyes, aimed like the view (child of the camera: no delay)
        transform.localPosition = settings.handOffset;
        transform.localRotation = Quaternion.identity;

        if (cookie == null || cookieHotspot != settings.hotspot || cookieSpill != settings.spill)
        {
            cookieHotspot = settings.hotspot;
            cookieSpill = settings.spill;
            BuildCookie();
        }

        spot.cookie = cookie;
    }

    public void SetOn(bool on)
    {
        spot.enabled = on;
    }

    // Brightness from the center of the beam (0) to its edge (1): hotspot, ring of the reflector, spill. Black edge
    private void BuildCookie()
    {
        if (cookie == null)
        {
            cookie = new Texture2D(CookieSize, CookieSize, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "Flashlight Cookie"
            };
        }

        Color32[] pixels = new Color32[CookieSize * CookieSize];
        float hotspot = Mathf.Max(cookieHotspot, 0.01f);

        for (int y = 0; y < CookieSize; y++)
        {
            for (int x = 0; x < CookieSize; x++)
            {
                float dx = (x + 0.5f) / CookieSize * 2f - 1f;
                float dy = (y + 0.5f) / CookieSize * 2f - 1f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);

                float hot = Mathf.Exp(-2.5f * (r / hotspot) * (r / hotspot));
                float ringDistance = (r - hotspot * 1.25f) / 0.05f;
                float ring = 0.1f * Mathf.Exp(-ringDistance * ringDistance);
                float spill = cookieSpill * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.5f, 1f, r)));
                float edge = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.9f, 1f, r));

                byte value = (byte)Mathf.RoundToInt(Mathf.Clamp01((hot + ring + spill) * edge) * 255f);
                pixels[y * CookieSize + x] = new Color32(value, value, value, value);
            }
        }

        cookie.SetPixels32(pixels);
        cookie.Apply(false, false);
    }
}
