using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partie visuelle du générateur à charbon (modèle Resources/Models/CoalGenerator).
/// À placer sur la racine de l'instance, à côté de l'Animator (CoalGenerator.controller, 3 couches indépendantes) :
///   "Hatch" : Hatch_Open / Hatch_Close (trappe de la trémie à charbon)
///   "Power" : Power_On / Power_Off (levier de marche sur la façade)
///   "Fan"   : Fan_Spin (ventilateur du flanc droit, 1 tour en 2 s à vitesse 1, paramètre FanSpeed) / Idle
/// En marche : le foyer (matériaux *_Fire) s'allume et vacille, le ventilateur accélère, le voyant (*_Status)
/// passe de l'orange au bleu, et la fumée (ParticleSystem placé sous l'enfant "Smoke") est jouée.
/// Cheminée réglable en hauteur (générateur posé dans un dôme) : SetChimneyHeight() étire Chimney_Shaft
/// et remonte Chimney_Head ; SetCollarHeight() place le collier d'étanchéité là où la cheminée traverse le dôme,
/// SetCollarRotation() l'oriente selon la normale de la coque. Le collier est un disque plein : penché, il est
/// étiré en ellipse (rayon fût / cos(inclinaison) + débord dans le sens de la pente, rayon fût + débord en travers).
/// Les transforms de la cheminée ne sont animés par aucun clip : le script peut les régler librement.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CoalGeneratorAnimator : MonoBehaviour
{
    [Header("Démarrage")]
    [SerializeField] bool runOnStart = false;

    [Header("Ventilateur")]
    [Tooltip("Vitesse du ventilateur en marche (1 = un tour en 2 s).")]
    [SerializeField, Range(0f, 6f)] float fanRunningSpeed = 3f;
    [Tooltip("Accélération / décélération du ventilateur (vitesse par seconde).")]
    [SerializeField] float fanAcceleration = 1.5f;

    [Header("Foyer")]
    [SerializeField] Color fireColor = new Color(1.0f, 0.28f, 0.02f);
    [SerializeField] float fireIntensity = 3f;
    [Tooltip("Lueur résiduelle à l'arrêt (braises), entre 0 et 1.")]
    [SerializeField, Range(0f, 1f)] float emberLevel = 0.06f;
    [Tooltip("Vitesse d'allumage / d'extinction du foyer (par seconde).")]
    [SerializeField] float fireFade = 0.5f;
    [SerializeField, Range(0f, 1f)] float flickerAmount = 0.35f;
    [SerializeField] float flickerSpeed = 6f;

    [Header("Voyant")]
    [SerializeField] Color stoppedColor = new Color(1.0f, 0.3f, 0.0f);
    [SerializeField] Color runningColor = new Color(0.05f, 0.6f, 1.0f);
    [SerializeField] float statusIntensity = 2.5f;
    [SerializeField] float blinkSpeed = 4f;

    [Header("Cheminée")]
    [Tooltip("Hauteur totale de la cheminée (m, depuis le sol du modèle). Appliquée au démarrage.")]
    [SerializeField] float chimneyHeight = DefaultChimneyHeight;
    [Tooltip("Hauteur du collier (m, depuis le sol du modèle). Négatif : collier masqué.")]
    [SerializeField] float collarHeight = -1f;
    [Tooltip("Débord du collier autour du fût (m), quelle que soit son inclinaison.")]
    [SerializeField] float collarOverhang = 0.10f;
    [Tooltip("Épaisseur du collier (m). Le disque du FBX fait CollarMeshThickness : il est étiré selon sa normale.")]
    [SerializeField] float collarThickness = CollarMeshThickness;
    [Tooltip("Fumée optionnelle. Si vide, cherche un ParticleSystem sous l'enfant Smoke.")]
    [SerializeField] ParticleSystem smoke;

    // dimensions du modèle Blender (cg_build.py)
    public const float ChimneyBaseHeight = 1.95f;     // haut du socle de cheminée = origine de Chimney_Shaft
    public const float DefaultShaftLength = 0.85f;    // longueur du fût dans le FBX (échelle 1)
    public const float HeadHeight = 0.30f;            // hauteur de la tête (jusqu'au sommet de la grille)
    public const float DefaultChimneyHeight = ChimneyBaseHeight + DefaultShaftLength + HeadHeight;   // 3.10 m
    public const float ShaftRadius = 0.19f;           // rayon du fût (aux sommets des 12 facettes)
    public const float CollarMeshRadius = 0.29f;      // rayon du disque du collier dans le FBX (échelle 1)
    public const float CollarMeshThickness = 0.06f;   // épaisseur du disque du collier dans le FBX (échelle 1)
    const float MinCollarCos = 0.0175f;               // garde-fou : cos(89°), évite une division par zéro
    const float MinShaftLength = 0.30f;
    const float CollarMargin = 0.20f;                 // le collier reste à 20 cm du socle et de la tête

    const int HatchLayer = 0, PowerLayer = 1, FanLayer = 2;
    static readonly int FanSpeedId = Animator.StringToHash("FanSpeed");
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColor = Shader.PropertyToID("_Color");

    Animator animator;
    Transform shaft, head, collar;
    Vector3 shaftRestPos, shaftRestScale, upPerMeter;  // upPerMeter : 1 m vers le haut, dans l'espace du parent
    Vector3 collarRestScale = Vector3.one;
    Quaternion collarRotation = Quaternion.identity;  // rotation du collier relative à la racine (normale = rot * up)
    readonly List<Material> fireMats = new List<Material>();
    readonly List<Material> statusMats = new List<Material>();
    Coroutine powerRoutine, hatchRoutine;
    float fanSpeed, fireLevel;
    bool starting;

    /// <summary>Le générateur produit (levier baissé, foyer allumé).</summary>
    public bool IsRunning { get; private set; }
    /// <summary>La trappe de la trémie est ouverte.</summary>
    public bool IsHatchOpen { get; private set; }
    /// <summary>Une séquence (levier ou trappe) est en cours.</summary>
    public bool IsBusy => powerRoutine != null || hatchRoutine != null;
    /// <summary>Hauteur totale actuelle de la cheminée (m, depuis le sol du modèle).</summary>
    public float ChimneyHeight => chimneyHeight;
    /// <summary>Hauteur minimale de la cheminée (fût le plus court).</summary>
    public static float MinChimneyHeight => ChimneyBaseHeight + MinShaftLength + HeadHeight;
    /// <summary>Rotation monde actuelle du collier (sa normale = CollarRotation * Vector3.up).</summary>
    public Quaternion CollarRotation => transform.rotation * collarRotation;
    /// <summary>Point de sortie de la fumée (son axe Z local pointe vers le haut).</summary>
    public Transform SmokePoint { get; private set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Chimney_Shaft") shaft = t;
            else if (t.name == "Chimney_Head") head = t;
            else if (t.name == "Chimney_Collar") collar = t;
            else if (t.name == "Smoke") SmokePoint = t;
        }
        if (shaft == null || head == null) Debug.LogWarning($"{name} : Chimney_Shaft ou Chimney_Head introuvable.", this);
        if (collar == null) Debug.LogWarning($"{name} : Chimney_Collar introuvable.", this);
        if (SmokePoint == null) Debug.LogWarning($"{name} : point de fumée Smoke introuvable.", this);
        if (smoke == null && SmokePoint != null) smoke = SmokePoint.GetComponentInChildren<ParticleSystem>(true);

        if (shaft != null && head != null)
        {
            shaftRestPos = shaft.localPosition;
            shaftRestScale = shaft.localScale;
            // la tête est posée en haut du fût : leur écart au repos donne la verticale du modèle (axes Blender conservés)
            upPerMeter = (head.localPosition - shaftRestPos) / DefaultShaftLength;
        }
        if (collar != null) collarRestScale = collar.localScale;

        CollectMaterials();
        animator.Play("Hatch_Close", HatchLayer, 1f);
        animator.Play("Power_Off", PowerLayer, 1f);
        animator.Play("Fan_Spin", FanLayer, 0f);
        animator.SetFloat(FanSpeedId, 0f);
        fireLevel = emberLevel;
        ApplyFire(0f);
        SetStatus(stoppedColor);
        if (smoke != null) smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        SetChimneyHeight(chimneyHeight);
        if (collarHeight >= 0f) SetCollarHeight(collarHeight); else HideCollar();
    }

    void Start() { if (runOnStart) TurnOn(); }

    // ---------------- API : marche / arrêt ----------------
    /// <summary>Baisse le levier puis allume le foyer et lance le ventilateur.</summary>
    public void TurnOn() => RunPower(TurnOnRoutine());
    /// <summary>Relève le levier, éteint le foyer et laisse le ventilateur ralentir.</summary>
    public void TurnOff() => RunPower(TurnOffRoutine());
    /// <summary>Bascule marche / arrêt.</summary>
    public void Toggle() { if (IsRunning || starting) TurnOff(); else TurnOn(); }

    // ---------------- API : trappe ----------------
    /// <summary>Ouvre la trappe de la trémie (chargement du charbon).</summary>
    public void OpenHatch() => RunHatch(HatchRoutine(true));
    /// <summary>Ferme la trappe de la trémie.</summary>
    public void CloseHatch() => RunHatch(HatchRoutine(false));
    /// <summary>Bascule ouverte / fermée.</summary>
    public void ToggleHatch() { if (IsHatchOpen) CloseHatch(); else OpenHatch(); }

    // ---------------- API : cheminée ----------------
    /// <summary>
    /// Règle la hauteur totale de la cheminée (m, depuis le sol du modèle, sommet de la tête compris).
    /// Le fût s'étire, la tête et la fumée suivent. Valeur bornée à <see cref="MinChimneyHeight"/>.
    /// </summary>
    public void SetChimneyHeight(float totalHeight)
    {
        chimneyHeight = Mathf.Max(MinChimneyHeight, totalHeight);
        if (shaft == null || head == null) return;
        float length = chimneyHeight - ChimneyBaseHeight - HeadHeight;
        var s = shaftRestScale; s.z *= length / DefaultShaftLength;          // fût : axe vertical = Z local (axes Blender)
        shaft.localScale = s;
        head.localPosition = shaftRestPos + upPerMeter * length;
        if (collar != null && collar.gameObject.activeSelf) SetCollarHeight(collarHeight);   // reste sur le fût
    }

    /// <summary>Règle la hauteur totale de la cheminée pour que son sommet atteigne une altitude monde donnée.</summary>
    public void SetChimneyTopWorldHeight(float worldY) => SetChimneyHeight(ToModelHeight(worldY));

    /// <summary>
    /// Affiche le collier d'étanchéité à une hauteur donnée (m, depuis le sol du modèle), par exemple
    /// là où la cheminée traverse la coque du dôme. Borné au fût visible.
    /// </summary>
    public void SetCollarHeight(float height)
    {
        if (collar == null || shaft == null) return;
        float lo = ChimneyBaseHeight + CollarMargin, hi = chimneyHeight - HeadHeight - CollarMargin;
        collarHeight = Mathf.Clamp(height, lo, Mathf.Max(lo, hi));
        collar.localPosition = shaftRestPos + upPerMeter * (collarHeight - ChimneyBaseHeight);
        collar.gameObject.SetActive(true);
        ApplyCollarShape();
    }

    /// <summary>
    /// Oriente le collier : <paramref name="worldRotation"/> est une rotation monde dont <c>worldRotation * Vector3.up</c>
    /// donne la normale voulue (par exemple Quaternion.FromToRotation(Vector3.up, normaleDuDôme)).
    /// Le collier est ensuite étiré en ellipse pour déborder de <c>collarOverhang</c> tout autour du fût.
    /// </summary>
    public void SetCollarRotation(Quaternion worldRotation)
    {
        collarRotation = Quaternion.Inverse(transform.rotation) * worldRotation;
        ApplyCollarShape();
    }

    /// <summary>Place le collier à une altitude monde (point de traversée du dôme).</summary>
    public void SetCollarWorldHeight(float worldY) => SetCollarHeight(ToModelHeight(worldY));

    /// <summary>Règle l'épaisseur du collier (m, selon sa normale), par exemple celle de la structure qu'il cache.</summary>
    public void SetCollarThickness(float thickness)
    {
        collarThickness = Mathf.Max(0.01f, thickness);
        ApplyCollarShape();
    }

    /// <summary>Masque le collier (générateur posé dehors).</summary>
    public void HideCollar()
    {
        collarHeight = -1f;
        if (collar != null) collar.gameObject.SetActive(false);
    }

    // ---------------- séquences ----------------
    void RunPower(IEnumerator r) { if (powerRoutine != null) StopCoroutine(powerRoutine); powerRoutine = StartCoroutine(r); }
    void RunHatch(IEnumerator r) { if (hatchRoutine != null) StopCoroutine(hatchRoutine); hatchRoutine = StartCoroutine(r); }

    IEnumerator TurnOnRoutine()
    {
        if (!IsRunning)
        {
            starting = true;
            animator.CrossFadeInFixedTime("Power_On", 0.1f, PowerLayer);
            yield return new WaitForSeconds(Length("Power_On"));
            IsRunning = true;
            if (smoke != null) smoke.Play(true);
            while (fireLevel < 0.95f) yield return null;                     // le voyant clignote pendant l'allumage
            starting = false;
        }
        powerRoutine = null;
    }

    IEnumerator TurnOffRoutine()
    {
        starting = false;
        if (IsRunning || animator.GetCurrentAnimatorStateInfo(PowerLayer).IsName("Power_On"))
        {
            IsRunning = false;
            if (smoke != null) smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            animator.CrossFadeInFixedTime("Power_Off", 0.1f, PowerLayer);
            yield return new WaitForSeconds(Length("Power_Off"));
        }
        powerRoutine = null;
    }

    IEnumerator HatchRoutine(bool open)
    {
        if (open != IsHatchOpen)
        {
            IsHatchOpen = open;
            string clip = open ? "Hatch_Open" : "Hatch_Close";
            animator.CrossFadeInFixedTime(clip, 0.1f, HatchLayer);
            yield return new WaitForSeconds(Length(clip));
        }
        hatchRoutine = null;
    }

    // ---------------- frame ----------------
    void Update()
    {
        fanSpeed = Mathf.MoveTowards(fanSpeed, IsRunning ? fanRunningSpeed : 0f, fanAcceleration * Time.deltaTime);
        animator.SetFloat(FanSpeedId, fanSpeed);

        fireLevel = Mathf.MoveTowards(fireLevel, IsRunning ? 1f : emberLevel, fireFade * Time.deltaTime);
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.37f) - 0.5f;
        ApplyFire(noise * flickerAmount * fireLevel);

        if (starting) SetStatus(stoppedColor * Mathf.Lerp(0.15f, 1f, Mathf.PingPong(Time.time * blinkSpeed, 1f)));
        else SetStatus(IsRunning ? runningColor : stoppedColor);
    }

    // ---------------- interne ----------------
    /// <summary>
    /// Pose le collier dans le plan de la coque et l'étire : la coupe d'un fût vertical de rayon r par un plan
    /// incliné de θ est une ellipse de demi-axes r / cos θ (dans le sens de la pente) et r (en travers).
    /// </summary>
    void ApplyCollarShape()
    {
        if (collar == null || shaft == null) return;
        Vector3 axis = shaft.TransformDirection(Vector3.forward).normalized;   // axe du fût (Z local, axes Blender)
        Vector3 n = (CollarRotation * Vector3.up).normalized;
        if (n.sqrMagnitude < 1e-6f) n = axis;
        float cos = Mathf.Max(Mathf.Abs(Vector3.Dot(axis, n)), MinCollarCos);

        // X local du collier = ligne de plus grande pente (projection de l'axe du fût sur le plan du collier)
        Vector3 slope = Vector3.ProjectOnPlane(axis, n);
        if (slope.sqrMagnitude < 1e-8f) slope = Vector3.ProjectOnPlane(transform.right, n);
        if (slope.sqrMagnitude < 1e-8f) slope = Vector3.ProjectOnPlane(transform.forward, n);
        slope.Normalize();
        // LookRotation : Z local -> n (normale du disque), et right = Cross(up, forward) = slope
        collar.rotation = Quaternion.LookRotation(n, Vector3.Cross(n, slope));

        float along = ShaftRadius / cos + collarOverhang;
        float across = ShaftRadius + collarOverhang;
        collar.localScale = new Vector3(collarRestScale.x * along / CollarMeshRadius,
                                        collarRestScale.y * across / CollarMeshRadius,
                                        collarRestScale.z * collarThickness / CollarMeshThickness);
    }

    float ToModelHeight(float worldY)
    {
        float scale = Mathf.Max(1e-4f, transform.lossyScale.y);
        return (worldY - transform.position.y) / scale;                     // instance supposée verticale
    }

    void CollectMaterials()
    {
        var cache = new Dictionary<Material, Material>();
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            var shared = r.sharedMaterials;
            bool any = false;
            foreach (var m in shared) any |= m != null && (m.name.EndsWith("_Fire") || m.name.EndsWith("_Status"));
            if (!any) continue;
            var inst = r.materials;
            for (int i = 0; i < inst.Length; i++)
            {
                if (shared[i] == null) continue;
                bool fire = shared[i].name.EndsWith("_Fire"), status = shared[i].name.EndsWith("_Status");
                if (!fire && !status) continue;
                if (!cache.TryGetValue(shared[i], out var m))
                {
                    m = inst[i]; m.EnableKeyword("_EMISSION"); cache[shared[i]] = m;
                    (fire ? fireMats : statusMats).Add(m);
                }
                inst[i] = m;
            }
            r.materials = inst;
        }
        if (fireMats.Count == 0) Debug.LogWarning($"{name} : aucun matériau *_Fire trouvé.", this);
    }

    void ApplyFire(float flicker)
    {
        float k = Mathf.Max(0f, fireLevel + flicker);
        foreach (var m in fireMats)
        {
            m.SetColor(EmissionColor, fireColor * fireIntensity * k);
            if (m.HasProperty(BaseColor)) m.SetColor(BaseColor, Color.Lerp(new Color(0.08f, 0.05f, 0.04f), new Color(1f, 0.35f, 0.08f), Mathf.Clamp01(k)));
        }
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
