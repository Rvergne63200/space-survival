using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partie visuelle de la capsule d'atterrissage (modèle Resources/Models/LandingCapsule), premier abri du joueur.
/// À placer sur la racine de l'instance, à côté de l'Animator (LandingCapsule.controller, une couche) :
///   Door_Open / Door_Close : la porte de la facette avant se rabat vers l'extérieur et devient la rampe d'accès.
/// Voyant (matériau *_Status, au-dessus de la porte) : bleu porte fermée, orange porte ouverte, clignotant pendant le mouvement.
/// L'intérieur est vide : l'aménagement se fait dans Unity (sol de la cabine à 0,72 m au-dessus de l'origine).
/// </summary>
[RequireComponent(typeof(Animator))]
public class LandingCapsuleAnimator : MonoBehaviour
{
    [SerializeField] bool startOpen = false;
    [SerializeField] Color closedColor = new Color(0.05f, 0.6f, 1.0f);
    [SerializeField] Color openColor = new Color(1.0f, 0.3f, 0.0f);
    [SerializeField] float statusIntensity = 2.5f;
    [SerializeField] float blinkSpeed = 4f;

    /// <summary>Hauteur du sol de la cabine au-dessus de l'origine du modèle (m).</summary>
    public const float CabinFloorHeight = 0.72f;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColor = Shader.PropertyToID("_Color");

    Animator animator;
    readonly List<Material> statusMats = new List<Material>();
    Coroutine sequence;

    /// <summary>La porte est ouverte (rampe au sol), ou en train de s'ouvrir.</summary>
    public bool IsOpen { get; private set; }
    /// <summary>La porte est en mouvement.</summary>
    public bool IsBusy => sequence != null;

    void Awake()
    {
        animator = GetComponent<Animator>();
        CollectStatusMaterials();
        IsOpen = startOpen;
        animator.Play(IsOpen ? "Door_Open" : "Door_Close", 0, 1f);   // pose initiale sans animation
        SetStatus(IsOpen ? openColor : closedColor);
    }

    // ---------------- API ----------------
    /// <summary>Ouvre la porte (elle se rabat en rampe jusqu'au sol).</summary>
    public void Open() => Run(true);
    /// <summary>Ferme la porte.</summary>
    public void Close() => Run(false);
    /// <summary>Bascule ouverte / fermée.</summary>
    public void Toggle() { if (IsOpen) Close(); else Open(); }
    /// <summary>Pose la porte ouverte ou fermée sans animation (chargement d'une sauvegarde).</summary>
    public void SetOpenImmediate(bool open)
    {
        if (sequence != null) { StopCoroutine(sequence); sequence = null; }
        IsOpen = open;
        animator.Play(IsOpen ? "Door_Open" : "Door_Close", 0, 1f);
        SetStatus(IsOpen ? openColor : closedColor);
    }

    // ---------------- séquence ----------------
    void Run(bool open)
    {
        bool interrupted = sequence != null;      // une porte en mouvement repart vers la nouvelle cible
        if (interrupted) StopCoroutine(sequence);
        sequence = StartCoroutine(Move(open, interrupted));
    }

    IEnumerator Move(bool open, bool force)
    {
        if (open != IsOpen || force)
        {
            string clip = open ? "Door_Open" : "Door_Close";
            float start = 0f;
            if (force && open != IsOpen)
            {
                // Door_Open et Door_Close sont symétriques : on repart de la position atteinte
                start = 1f - Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                animator.Play(clip, 0, start);
            }
            else if (open != IsOpen) animator.CrossFadeInFixedTime(clip, 0.1f);
            else start = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);   // même cible : on attend la fin
            IsOpen = open;
            float length = Length(clip), t = start * length;
            while (t < length)
            {
                t += Time.deltaTime;
                SetStatus(openColor * Mathf.Lerp(0.15f, 1f, Mathf.PingPong(Time.time * blinkSpeed, 1f)));
                yield return null;
            }
        }
        SetStatus(IsOpen ? openColor : closedColor);
        sequence = null;
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
