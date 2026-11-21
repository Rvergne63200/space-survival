using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partie visuelle de la foreuse (modèle Resources/Models/Drill) : derrick à trépied, tête à molettes poussée par deux vérins.
/// À placer sur la racine de l'instance, à côté de l'Animator (Drill.controller, 2 couches indépendantes) :
///   "Feed" : Drill_Lower / Drill_Raise (le chariot descend la tête dans le sol, ou la remonte)
///   "Spin" : Drill_Spin (rotation de la tête, 1 tour en 2 s à vitesse 1, paramètre SpinSpeed) / Idle
/// Forage : la tête accélère, puis le chariot descend ; à l'arrêt, le chariot remonte puis la tête ralentit.
/// Voyant (matériau *_Status, sur le pupitre) : orange à l'arrêt, bleu en forage, clignotant pendant les transitions.
/// Écran holographique (objet "Hologram", enfant "Hologram_Screen" avec UV 0..1) : visible quand la foreuse est
/// alimentée (<see cref="Powered"/>), pour y afficher plus tard une interface.
/// Poussière : ParticleSystem optionnel sous l'enfant "DrillDust" (point de forage, Z local vers le haut),
/// joué pendant le forage.
/// </summary>
[RequireComponent(typeof(Animator))]
public class DrillAnimator : MonoBehaviour
{
    [SerializeField] bool drillOnStart = false;
    [Tooltip("Vitesse de rotation de la tête en forage (1 = un tour en 2 s).")]
    [SerializeField, Range(0f, 6f)] float spinSpeed = 3f;
    [Tooltip("Accélération / décélération de la tête (vitesse par seconde).")]
    [SerializeField] float spinAcceleration = 2f;
    [Tooltip("Écran holographique visible quand la foreuse est alimentée.")]
    [SerializeField] bool powered = true;

    [Header("Voyant")]
    [SerializeField] Color stoppedColor = new Color(1.0f, 0.3f, 0.0f);
    [SerializeField] Color drillingColor = new Color(0.05f, 0.6f, 1.0f);
    [SerializeField] float statusIntensity = 2.5f;
    [SerializeField] float blinkSpeed = 4f;

    [Tooltip("Poussière optionnelle. Si vide, cherche un ParticleSystem sous l'enfant DrillDust.")]
    [SerializeField] ParticleSystem dust;

    const int FeedLayer = 0, SpinLayer = 1;
    static readonly int SpinSpeedId = Animator.StringToHash("SpinSpeed");
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColor = Shader.PropertyToID("_Color");

    Animator animator;
    GameObject hologram;
    readonly List<Material> statusMats = new List<Material>();
    Coroutine sequence;
    float currentSpin;
    bool spinning, transition, lowered;

    /// <summary>La tête est descendue et tourne.</summary>
    public bool IsDrilling { get; private set; }
    /// <summary>Une transition (démarrage ou arrêt) est en cours.</summary>
    public bool IsBusy => sequence != null;
    /// <summary>Point de forage (son axe Z local pointe vers le haut).</summary>
    public Transform DustPoint { get; private set; }
    /// <summary>Alimentation : affiche ou masque l'écran holographique. Couper l'alimentation arrête le forage.</summary>
    public bool Powered
    {
        get => powered;
        set
        {
            powered = value;
            if (hologram != null) hologram.SetActive(value);
            if (!value && (IsDrilling || IsBusy)) StopDrilling();
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Hologram") hologram = t.gameObject;
            else if (t.name == "DrillDust") DustPoint = t;
        }
        if (hologram == null) Debug.LogWarning($"{name} : objet Hologram introuvable.", this);
        if (DustPoint == null) Debug.LogWarning($"{name} : point de forage DrillDust introuvable.", this);
        if (dust == null && DustPoint != null) dust = DustPoint.GetComponentInChildren<ParticleSystem>(true);
        if (dust != null) dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        CollectStatusMaterials();
        animator.Play("Drill_Raise", FeedLayer, 1f);
        animator.Play("Drill_Spin", SpinLayer, 0f);      // la vitesse 0 fige la tête
        animator.SetFloat(SpinSpeedId, 0f);
        if (hologram != null) hologram.SetActive(powered);
        SetStatus(stoppedColor);
    }

    void Start() { if (drillOnStart) StartDrilling(); }

    // ---------------- API ----------------
    /// <summary>Lance la tête puis la descend dans le sol.</summary>
    public void StartDrilling() { if (powered) Run(StartRoutine()); }
    /// <summary>Remonte la tête puis l'arrête.</summary>
    public void StopDrilling() => Run(StopRoutine());
    /// <summary>Bascule forage / arrêt.</summary>
    public void Toggle() { if (IsDrilling || (IsBusy && spinning)) StopDrilling(); else StartDrilling(); }

    // ---------------- séquences ----------------
    void Run(IEnumerator r) { if (sequence != null) StopCoroutine(sequence); sequence = StartCoroutine(r); }

    IEnumerator StartRoutine()
    {
        transition = true;
        spinning = true;
        while (currentSpin < spinSpeed * 0.95f) yield return null;          // la tête tourne avant de toucher le sol
        if (!IsLowered()) yield return Feed("Drill_Lower");
        IsDrilling = true;
        if (dust != null) dust.Play(true);
        transition = false;
        sequence = null;
    }

    IEnumerator StopRoutine()
    {
        transition = true;
        IsDrilling = false;
        if (dust != null) dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (IsLowered()) yield return Feed("Drill_Raise");
        spinning = false;
        while (currentSpin > 0.01f) yield return null;
        transition = false;
        sequence = null;
    }

    IEnumerator Feed(string clip)
    {
        // Drill_Lower et Drill_Raise sont symétriques : si l'autre clip est en cours, on repart de la position atteinte
        var st = animator.GetCurrentAnimatorStateInfo(FeedLayer);
        float start = 0f;
        string other = clip == "Drill_Lower" ? "Drill_Raise" : "Drill_Lower";
        if (st.IsName(other) && st.normalizedTime < 1f) start = 1f - Mathf.Clamp01(st.normalizedTime);
        lowered = clip == "Drill_Lower";                 // cible mémorisée dès le départ (interruption possible)
        animator.Play(clip, FeedLayer, start);
        yield return new WaitForSeconds(Length(clip) * (1f - start));
    }

    /// <summary>Le chariot est descendu, ou en train de descendre.</summary>
    bool IsLowered() => lowered;

    // ---------------- frame ----------------
    void Update()
    {
        currentSpin = Mathf.MoveTowards(currentSpin, spinning ? spinSpeed : 0f, spinAcceleration * Time.deltaTime);
        animator.SetFloat(SpinSpeedId, currentSpin);
        if (transition) SetStatus(stoppedColor * Mathf.Lerp(0.15f, 1f, Mathf.PingPong(Time.time * blinkSpeed, 1f)));
        else SetStatus(IsDrilling ? drillingColor : stoppedColor);
    }

    // ---------------- interne ----------------
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

    void SetStatus(Color c)
    {
        foreach (var m in statusMats)
        {
            m.SetColor(EmissionColor, c * statusIntensity);
            if (m.HasProperty(BaseColor)) m.SetColor(BaseColor, Color.Lerp(Color.white, c, 0.6f));
        }
    }

    float Length(string clip)
    {
        if (animator.runtimeAnimatorController != null)
            foreach (var c in animator.runtimeAnimatorController.animationClips)
                if (c.name == clip) return c.length;
        return 1f;
    }
}
