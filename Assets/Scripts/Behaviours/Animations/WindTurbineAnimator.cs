using System.Collections;
using UnityEngine;

/// <summary>
/// Partie visuelle de l'éolienne (modèle Resources/Models/WindTurbine).
/// À placer sur la racine de l'instance, à côté de l'Animator (WindTurbine.controller, 2 couches indépendantes) :
///   "Deploy" : Turbine_Deploy / Turbine_Fold (mât télescopique + pales qui se déplient / se replient)
///   "Rotor"  : Rotor_Spin (1 tour en 2 s à vitesse 1, paramètre RotorSpeed) / Idle
/// Orientation au vent : la nacelle se tourne (par script) face à la direction d'où vient le vent.
/// Avant de replier, le rotor ralentit puis s'aligne sur un multiple de 120° (aucune pale ne se replie sur le mât).
/// Quand le rotor ne tourne pas, le script impose son angle à chaque frame (LateUpdate, après l'Animator),
/// sinon l'Animator réécrirait sa dernière valeur.
/// </summary>
[RequireComponent(typeof(Animator))]
public class WindTurbineAnimator : MonoBehaviour
{
    [SerializeField] bool deployOnStart = false;
    [Tooltip("Vitesse du rotor une fois déployé (1 = un tour en 2 s).")]
    [SerializeField, Range(0f, 4f)] float windStrength = 1f;
    [Tooltip("Accélération / décélération du rotor (vitesse par seconde).")]
    [SerializeField] float rotorAcceleration = 0.6f;
    [Tooltip("Direction (monde) d'où vient le vent. La nacelle se tourne face à elle.")]
    [SerializeField] Vector3 windFrom = Vector3.forward;
    [SerializeField] float yawSpeed = 25f;

    const int DeployLayer = 0, RotorLayer = 1;
    static readonly int RotorSpeedId = Animator.StringToHash("RotorSpeed");

    Animator animator;
    Transform nacelle, rotor;
    Quaternion nacelleRest, rotorRest;
    float nacelleAngle, rotorAngle;   // angles imposés par le script (degrés)
    float rotorSpeed;                 // vitesse courante (rampée)
    bool spinning;                    // le clip Rotor_Spin pilote le rotor
    bool aligning;
    Coroutine sequence;

    public bool IsDeployed { get; private set; }
    public float WindStrength { get => windStrength; set => windStrength = Mathf.Max(0f, value); }

    void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Nacelle") nacelle = t;
            if (t.name == "Rotor") rotor = t;
        }
        if (nacelle != null) nacelleRest = nacelle.localRotation;
        if (rotor != null) rotorRest = rotor.localRotation;
        animator.Play("Turbine_Fold", DeployLayer, 1f);
        animator.Play("Idle", RotorLayer, 0f);
        animator.SetFloat(RotorSpeedId, 0f);
    }

    void Start() { if (deployOnStart) Deploy(); }

    // ---------------- API ----------------
    /// <summary>Sort le mât, déplie les pales, puis lance le rotor.</summary>
    public void Deploy() => Run(DeployRoutine());
    /// <summary>Ralentit et aligne le rotor, replie les pales puis rentre le mât.</summary>
    public void Fold() => Run(FoldRoutine());
    /// <summary>Direction monde d'où vient le vent.</summary>
    public void SetWindDirection(Vector3 fromDirection) => windFrom = fromDirection;

    // ---------------- séquences ----------------
    void Run(IEnumerator r) { if (sequence != null) StopCoroutine(sequence); sequence = StartCoroutine(r); }

    IEnumerator DeployRoutine()
    {
        if (!IsDeployed)
        {
            animator.CrossFadeInFixedTime("Turbine_Deploy", 0.1f, DeployLayer);
            yield return new WaitForSeconds(Length("Turbine_Deploy"));
        }
        IsDeployed = true;
        StartSpin();
        sequence = null;
    }

    IEnumerator FoldRoutine()
    {
        if (IsDeployed)
        {
            IsDeployed = false;                                 // la cible de vitesse passe à 0 (Update)
            while (rotorSpeed > 0.01f) yield return null;
            StopSpin();
            aligning = true;
            while (aligning) yield return null;
            animator.CrossFadeInFixedTime("Turbine_Fold", 0.1f, DeployLayer);
            yield return new WaitForSeconds(Length("Turbine_Fold"));
        }
        sequence = null;
    }

    void StartSpin()
    {
        if (spinning || rotor == null) return;
        spinning = true; aligning = false;
        animator.Play("Rotor_Spin", RotorLayer, Mathf.Repeat(-rotorAngle / 360f, 1f));   // Rotor_Spin : 0 -> -360°
    }

    void StopSpin()
    {
        if (!spinning) return;
        spinning = false;
        animator.Play("Idle", RotorLayer);
        if (rotor != null) rotorAngle = MeasureRotor();         // le script reprend à l'angle atteint
    }

    // ---------------- frame ----------------
    void Update()
    {
        float goal = IsDeployed && spinning ? windStrength : 0f;
        rotorSpeed = Mathf.MoveTowards(rotorSpeed, goal, rotorAcceleration * Time.deltaTime);
        animator.SetFloat(RotorSpeedId, rotorSpeed);
    }

    void LateUpdate()   // après l'Animator
    {
        if (nacelle != null)
        {
            if (IsDeployed && windFrom.sqrMagnitude > 1e-4f)
            {
                float goal = Heading(nacelle, nacelleRest, Vector3.down, Vector3.forward, windFrom);
                if (!float.IsNaN(goal)) nacelleAngle = Mathf.MoveTowardsAngle(nacelleAngle, goal, yawSpeed * Time.deltaTime);
            }
            nacelle.localRotation = nacelleRest * Quaternion.AngleAxis(nacelleAngle, Vector3.forward);   // axe vertical = Z local
        }
        if (rotor != null && !spinning)
        {
            if (aligning)
            {
                float target = Mathf.Round(rotorAngle / 120f) * 120f;
                rotorAngle = Mathf.MoveTowards(rotorAngle, target, 60f * Time.deltaTime);
                if (Mathf.Abs(rotorAngle - target) < 0.01f) { rotorAngle = target; aligning = false; }
            }
            rotor.localRotation = rotorRest * Quaternion.AngleAxis(rotorAngle, Vector3.up);             // axe du rotor = Y local
        }
    }

    // ---------------- géométrie (axes Blender conservés sous la racine du prefab) ----------------
    static float Heading(Transform t, Quaternion rest, Vector3 localFront, Vector3 localAxis, Vector3 worldDir)
    {
        Quaternion parent = t.parent != null ? t.parent.rotation : Quaternion.identity;
        Vector3 up = parent * rest * localAxis;
        Vector3 a = Vector3.ProjectOnPlane(parent * rest * localFront, up), b = Vector3.ProjectOnPlane(worldDir, up);
        if (a.sqrMagnitude < 1e-6f || b.sqrMagnitude < 1e-6f) return float.NaN;
        return Vector3.SignedAngle(a, b, up);
    }

    float MeasureRotor()
    {
        Vector3 axis = rotorRest * Vector3.up;
        Vector3 z0 = rotorRest * Vector3.forward, z = rotor.localRotation * Vector3.forward;
        return Vector3.SignedAngle(z0, z, axis);
    }

    float Length(string clip)
    {
        if (animator.runtimeAnimatorController != null)
            foreach (var c in animator.runtimeAnimatorController.animationClips)
                if (c.name == clip) return c.length;
        return 1f;
    }
}
