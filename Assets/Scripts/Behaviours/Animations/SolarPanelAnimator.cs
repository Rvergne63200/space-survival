using UnityEngine;

/// <summary>
/// Partie visuelle du panneau solaire au sol (modèle Resources/Models/SolarPanel).
/// À placer sur la racine de l'instance, à côté de l'Animator (SolarPanel.controller, 2 couches indépendantes) :
///   "Panel"    : Panel_Deploy / Panel_Fold (ailes qui se déplient puis inclinaison de 30° vers l'avant)
///   "Rotation" : Rotate_Loop (la tête tourne sur l'axe vertical, 1 tour en 4 s) / Idle (arrêt sur place)
///
/// Suivi du soleil : assigner une lumière dans "Sun" (ou appeler FaceDirection). Une fois DÉPLOYÉ, la tête
/// (Panel_Yaw) se tourne progressivement pour que la face des cellules regarde la lumière.
/// Le script garde son propre angle et l'impose à chaque frame (LateUpdate, après l'Animator) :
/// l'Animator réécrit sinon sa dernière valeur sur Panel_Yaw et annulerait la rotation.
/// Pose de repos : replié, orienté vers l'avant (+Z) de l'objet.
/// </summary>
[RequireComponent(typeof(Animator))]
public class SolarPanelAnimator : MonoBehaviour
{
    [SerializeField] bool deployOnStart = false;
    [Tooltip("Lumière à suivre une fois déployé (ta Directional Light). Le panneau se tourne face à elle.")]
    [SerializeField] Light sun;
    [Tooltip("Vitesse d'orientation vers le soleil (degrés / seconde).")]
    [SerializeField] float trackingSpeed = 20f;
    [Tooltip("Vitesse de Rotate_Loop (1 = un tour en 4 s ; négatif = sens inverse).")]
    [SerializeField] float rotationSpeed = 0.25f;

    const int PanelLayer = 0, RotationLayer = 1;
    static readonly int RotationSpeedId = Animator.StringToHash("RotationSpeed");

    Animator animator;
    Transform yaw;               // tête pivotante (Panel_Yaw)
    Quaternion yawRest;          // rotation locale de repos
    float yawAngle;              // angle courant imposé par le script (degrés, autour de l'axe vertical)
    bool scriptDrivesYaw = true; // false pendant Rotate_Loop
    Vector3? target;             // direction monde visée (FaceDirection)

    public bool IsDeployed { get; private set; }
    public bool IsRotating { get; private set; }
    public float HeadingAngle => yawAngle;

    void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == "Panel_Yaw") { yaw = t; break; }
        if (yaw != null) yawRest = yaw.localRotation;
        else Debug.LogWarning($"{name} : objet 'Panel_Yaw' introuvable (modèle SolarPanel attendu).", this);
        animator.Play("Panel_Fold", PanelLayer, 1f);
        animator.Play("Idle", RotationLayer, 0f);
        animator.SetFloat(RotationSpeedId, rotationSpeed);
    }

    void Start() { if (deployOnStart) Deploy(); }

    // ---------------- dépliage ----------------
    public void Deploy()
    {
        if (IsDeployed) return;
        IsDeployed = true;
        animator.CrossFadeInFixedTime("Panel_Deploy", 0.1f, PanelLayer);
    }

    public void Fold()
    {
        if (!IsDeployed) return;
        IsDeployed = false;
        animator.CrossFadeInFixedTime("Panel_Fold", 0.1f, PanelLayer);
    }

    public void Toggle() { if (IsDeployed) Fold(); else Deploy(); }

    // ---------------- rotation continue (clip) ----------------
    public void StartRotation()
    {
        target = null;
        IsRotating = true;
        scriptDrivesYaw = false;
        animator.SetFloat(RotationSpeedId, rotationSpeed);
        animator.Play("Rotate_Loop", RotationLayer, Mathf.Repeat(yawAngle / 360f, 1f));   // repart de l'angle actuel
    }

    /// <summary>Arrête la tête dans son orientation actuelle.</summary>
    public void StopRotation()
    {
        if (!IsRotating) return;
        IsRotating = false;
        animator.Play("Idle", RotationLayer);
        if (yaw != null) yawAngle = MeasureYaw();        // reprend la main à partir de l'angle atteint
        scriptDrivesYaw = true;
    }

    public void SetRotationSpeed(float speed) { rotationSpeed = speed; animator.SetFloat(RotationSpeedId, speed); }

    // ---------------- suivi ----------------
    /// <summary>Oriente progressivement la face du panneau vers une direction monde (seule la composante horizontale compte).</summary>
    public void FaceDirection(Vector3 worldDirection)
    {
        StopRotation();
        target = worldDirection;
    }

    /// <summary>Arrête le suivi (la tête reste où elle est). Le suivi de "Sun" reprend si une lumière est assignée.</summary>
    public void StopTracking() => target = null;

    public void SetSun(Light light) => sun = light;

    void LateUpdate()   // après l'Animator
    {
        if (yaw == null || !scriptDrivesYaw) return;

        Vector3? dir = target;
        if (dir == null && sun != null && IsDeployed) dir = -sun.transform.forward;   // direction VERS la lumière
        if (dir != null)
        {
            float goal = HeadingFor(dir.Value);
            if (!float.IsNaN(goal)) yawAngle = Mathf.MoveTowardsAngle(yawAngle, goal, trackingSpeed * Time.deltaTime);
        }
        ApplyYaw();   // imposé à chaque frame, même sans cible (sinon l'Animator remettrait son ancienne valeur)
    }

    // ---------------- géométrie ----------------
    // Panel_Yaw garde les axes Blender : axe vertical = Z local, avant du panneau = -Y local.
    void ApplyYaw() => yaw.localRotation = yawRest * Quaternion.AngleAxis(yawAngle, Vector3.forward);

    Vector3 RestFront()   // avant du panneau (monde) quand yawAngle = 0
    {
        Quaternion parent = yaw.parent != null ? yaw.parent.rotation : Quaternion.identity;
        return parent * yawRest * Vector3.down;
    }

    Vector3 VerticalAxis()
    {
        Quaternion parent = yaw.parent != null ? yaw.parent.rotation : Quaternion.identity;
        return parent * yawRest * Vector3.forward;
    }

    float HeadingFor(Vector3 worldDir)
    {
        Vector3 up = VerticalAxis();
        Vector3 a = Vector3.ProjectOnPlane(RestFront(), up), b = Vector3.ProjectOnPlane(worldDir, up);
        if (a.sqrMagnitude < 1e-6f || b.sqrMagnitude < 1e-6f) return float.NaN;   // soleil au zénith : on garde l'angle
        return Vector3.SignedAngle(a, b, up);
    }

    float MeasureYaw()
    {
        Vector3 up = VerticalAxis();
        Vector3 now = yaw.rotation * Vector3.down;
        return Vector3.SignedAngle(Vector3.ProjectOnPlane(RestFront(), up), Vector3.ProjectOnPlane(now, up), up);
    }
}
