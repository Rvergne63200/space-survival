using System.Collections;
using UnityEngine;

/// <summary>
/// Partie visuelle de l'antenne relais (modèle Resources/Models/RelayAntenna).
/// À placer sur la racine de l'instance, à côté de l'Animator (RelayAntenna.controller).
/// Le controller a 3 couches indépendantes :
///   "Dish"     : Dish_Deploy / Dish_Undeploy   (parabole qui se relève / se rabat)
///   "Mast"     : Antenna_Extend / Antenna_Retract (antenne télescopique)
///   "Rotation" : Rotate_Loop (toute la tête tourne sur l'axe vertical) / Idle (arrêt sur place)
/// Pose de repos : parabole rabattue, antenne rentrée.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RelayAntennaAnimator : MonoBehaviour
{
    [SerializeField] bool deployOnStart = false;
    [SerializeField] bool rotateWhenDeployed = true;
    [Tooltip("1 = un tour en 4 s ; négatif = sens inverse.")]
    [SerializeField] float rotationSpeed = 0.5f;

    const int DishLayer = 0, MastLayer = 1, RotationLayer = 2;
    static readonly int RotationSpeedId = Animator.StringToHash("RotationSpeed");
    Animator animator;
    Coroutine sequence;

    public bool IsDishDeployed { get; private set; }
    public bool IsAntennaExtended { get; private set; }
    public bool IsRotating { get; private set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.Play("Dish_Undeploy", DishLayer, 1f);
        animator.Play("Antenna_Retract", MastLayer, 1f);
        animator.Play("Idle", RotationLayer, 0f);
        animator.SetFloat(RotationSpeedId, rotationSpeed);
    }

    void Start() { if (deployOnStart) Deploy(); }

    // ---------------- les 5 animations ----------------
    public void ExtendAntenna()  { IsAntennaExtended = true;  animator.CrossFadeInFixedTime("Antenna_Extend", 0.1f, MastLayer); }
    public void RetractAntenna() { IsAntennaExtended = false; animator.CrossFadeInFixedTime("Antenna_Retract", 0.1f, MastLayer); }
    public void DeployDish()     { IsDishDeployed = true;     animator.CrossFadeInFixedTime("Dish_Deploy", 0.1f, DishLayer); }
    public void UndeployDish()   { IsDishDeployed = false;    animator.CrossFadeInFixedTime("Dish_Undeploy", 0.1f, DishLayer); }

    public void StartRotation()
    {
        IsRotating = true;
        animator.SetFloat(RotationSpeedId, rotationSpeed);
        animator.Play("Rotate_Loop", RotationLayer);
    }

    /// <summary>Arrête la tête dans son orientation actuelle.</summary>
    public void StopRotation()
    {
        IsRotating = false;
        animator.Play("Idle", RotationLayer);   // Write Defaults off : la tête garde son orientation
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
        animator.SetFloat(RotationSpeedId, speed);
    }

    // ---------------- séquences complètes ----------------
    /// <summary>Relève la parabole, sort l'antenne, puis lance la rotation (si activée).</summary>
    public void Deploy() => Run(DeployRoutine());
    /// <summary>Arrête la rotation, rentre l'antenne, puis rabat la parabole.</summary>
    public void Stow() => Run(StowRoutine());

    void Run(IEnumerator r) { if (sequence != null) StopCoroutine(sequence); sequence = StartCoroutine(r); }

    IEnumerator DeployRoutine()
    {
        DeployDish();
        yield return new WaitForSeconds(Length("Dish_Deploy") * 0.7f);
        ExtendAntenna();
        yield return new WaitForSeconds(Length("Antenna_Extend"));
        if (rotateWhenDeployed) StartRotation();
        sequence = null;
    }

    IEnumerator StowRoutine()
    {
        StopRotation();
        RetractAntenna();
        yield return new WaitForSeconds(Length("Antenna_Retract"));
        UndeployDish();
        yield return new WaitForSeconds(Length("Dish_Undeploy"));
        sequence = null;
    }

    float Length(string clip)
    {
        if (animator.runtimeAnimatorController != null)
            foreach (var c in animator.runtimeAnimatorController.animationClips)
                if (c.name == clip) return c.length;
        return 1f;
    }
}
