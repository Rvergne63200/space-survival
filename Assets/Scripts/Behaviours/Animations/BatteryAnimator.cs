using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Jauge et voyant des batteries (modèles statiques Battery_Wall, Battery_Cabinet et Battery_Bank : pas d'Animator).
/// Jauge : les objets "Gauge_1" (en bas) à "Gauge_N" (en haut), en matériau *_Neon. Un segment allumé brille en bleu,
/// un segment éteint est graphite. En charge, le segment qui se remplit clignote.
/// Voyant (matériau *_Status) : bleu quand la batterie marche et contient de l'énergie, orange quand elle est vide,
/// éteint quand la batterie est éteinte (la jauge aussi).
/// Le gameplay le pilote par <see cref="SetState"/> (voir BatteryIndicator).
/// </summary>
public class BatteryAnimator : MonoBehaviour
{
    [Tooltip("Couleur d'un segment éteint (sans émission).")]
    [SerializeField] Color offSegmentColor = new Color(0.035f, 0.045f, 0.06f);
    [Tooltip("Durée d'un clignotement complet du segment en charge (s).")]
    [SerializeField] float blinkPeriod = 1.2f;

    [Header("Voyant")]
    [SerializeField] Color okColor = new Color(0.05f, 0.6f, 1.0f);
    [SerializeField] Color alertColor = new Color(1.0f, 0.3f, 0.0f);
    [SerializeField] Color offColor = new Color(0.04f, 0.04f, 0.04f);
    [SerializeField] float statusIntensity = 2.5f;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColor = Shader.PropertyToID("_Color");

    readonly List<Material> segments = new List<Material>();
    readonly List<Material> statusMats = new List<Material>();
    Color segmentOnColor, segmentOnEmission;

    int lit = -1;          // segments allumés au dernier affichage (-1 : rien encore affiché)
    bool blinkOn;

    /// <summary>Niveau de charge affiché, de 0 à 1.</summary>
    public float Level { get; private set; }
    public bool IsCharging { get; private set; }
    public bool IsPowered { get; private set; }

    void Awake()
    {
        CollectSegments();
        CollectStatusMaterials();
        Apply(true);
    }

    // ---------------- API ----------------

    /// <summary>Affiche l'état de la batterie : niveau (0..1), en charge, allumée.</summary>
    public void SetState(float level, bool charging, bool powered)
    {
        level = Mathf.Clamp01(level);
        bool changed = powered != IsPowered || charging != IsCharging || SegmentsFor(level) != SegmentsFor(Level);

        Level = level;
        IsCharging = charging;
        IsPowered = powered;

        if (changed) Apply(true);
    }

    // ---------------- interne ----------------

    void Update()
    {
        if (!IsPowered || !IsCharging) return;

        bool on = Mathf.Repeat(Time.time, blinkPeriod) < blinkPeriod * 0.5f;
        if (on != blinkOn)
        {
            blinkOn = on;
            Apply(false);
        }
    }

    // Segments allumés pour un niveau : une batterie qui n'est pas vide en allume au moins un
    int SegmentsFor(float level)
    {
        if (segments.Count == 0 || level <= 0.001f) return 0;
        return Mathf.Clamp(Mathf.CeilToInt(level * segments.Count - 0.001f), 1, segments.Count);
    }

    void Apply(bool force)
    {
        int count = IsPowered ? SegmentsFor(Level) : 0;
        // En charge : le segment qui se remplit (le premier éteint, ou le dernier allumé s'ils le sont tous) clignote
        int blinking = IsPowered && IsCharging ? Mathf.Min(count, segments.Count - 1) : -1;

        if (!force && count == lit && blinking < 0) return;
        lit = count;

        for (int i = 0; i < segments.Count; i++)
        {
            bool on = i < count;
            if (i == blinking) on = blinkOn;
            segments[i].SetColor(BaseColor, on ? segmentOnColor : offSegmentColor);
            segments[i].SetColor(EmissionColor, on ? segmentOnEmission : Color.black);
        }

        Color c = !IsPowered ? offColor : count == 0 ? alertColor : okColor;
        foreach (var m in statusMats)
        {
            m.SetColor(EmissionColor, IsPowered ? c * statusIntensity : Color.black);
            if (m.HasProperty(BaseColor)) m.SetColor(BaseColor, IsPowered ? Color.Lerp(Color.white, c, 0.6f) : c);
        }
    }

    void CollectSegments()
    {
        var found = new SortedDictionary<int, Renderer>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.StartsWith("Gauge_") || !int.TryParse(t.name.Substring(6), out int index)) continue;
            var r = t.GetComponent<Renderer>();
            if (r != null) found[index] = r;
        }

        foreach (var r in found.Values)
        {
            Material m = r.material;                       // instance : l'asset partagé n'est pas modifié
            m.EnableKeyword("_EMISSION");
            if (segments.Count == 0)
            {
                segmentOnColor = m.GetColor(BaseColor);
                segmentOnEmission = m.GetColor(EmissionColor);
            }
            segments.Add(m);
        }

        if (segments.Count == 0) Debug.LogWarning($"{name} : aucun segment Gauge_N trouvé.", this);
    }

    void CollectStatusMaterials()
    {
        var cache = new Dictionary<Material, Material>();
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            var shared = r.sharedMaterials;
            bool any = false;
            foreach (var m in shared) any |= m != null && m.name.EndsWith("_Status");
            if (!any) continue;
            var inst = r.materials;
            for (int i = 0; i < inst.Length; i++)
            {
                if (shared[i] == null || !shared[i].name.EndsWith("_Status")) continue;
                if (!cache.TryGetValue(shared[i], out var m)) { m = inst[i]; m.EnableKeyword("_EMISSION"); cache[shared[i]] = m; statusMats.Add(m); }
                inst[i] = m;
            }
            r.materials = inst;
        }
        if (statusMats.Count == 0) Debug.LogWarning($"{name} : aucun matériau *_Status trouvé.", this);
    }
}
