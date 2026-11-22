using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partie visuelle du panneau de contrôle (modèle Resources/Models/ControlPanel) : pupitre holographique.
/// À placer sur la racine de l'instance, à côté de l'Animator (ControlPanel.controller, une couche) :
///   Power_On : la barre du projecteur (objet "Projector") sort de son logement à l'arrière du pupitre, puis l'écran
///              holographique (objet "Hologram") se déploie depuis la lentille
///   Power_Off : l'écran se replie dans la lentille, puis la barre redescend (état par défaut : écran invisible)
/// L'apparition de l'écran est dans les clips (échelle de "Hologram") : le script ne le masque pas lui-même.
/// Son enfant "Hologram_Screen" (UV 0..1, face avant vers le joueur) pourra afficher la carte du réseau.
/// Voyant (matériau *_Status, sur le plan de travail) : orange éteint, bleu allumé, clignotant pendant les transitions.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ControlPanelAnimator : MonoBehaviour
{
    [SerializeField] bool startPowered = false;

    [Header("Voyant")]
    [SerializeField] Color offColor = new Color(1.0f, 0.3f, 0.0f);
    [SerializeField] Color onColor = new Color(0.05f, 0.6f, 1.0f);
    [SerializeField] float statusIntensity = 2.5f;
    [SerializeField] float blinkSpeed = 4f;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColor = Shader.PropertyToID("_Color");

    Animator animator;
    readonly List<Material> statusMats = new List<Material>();
    Coroutine sequence;

    /// <summary>Allumé (ou en train de s'allumer).</summary>
    public bool IsPowered { get; private set; }
    /// <summary>Le projecteur sort ou rentre.</summary>
    public bool IsBusy => sequence != null;
    /// <summary>Surface vierge de l'écran holographique (UV 0..1), pour y afficher une interface.</summary>
    public Transform HologramScreen { get; private set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == "Hologram_Screen") { HologramScreen = t; break; }
        if (HologramScreen == null) Debug.LogWarning($"{name} : objet Hologram_Screen introuvable.", this);

        CollectStatusMaterials();
        IsPowered = startPowered;
        animator.Play(startPowered ? "Power_On" : "Power_Off", 0, 1f);   // pose finale, sans jouer l'anim
        SetStatus(startPowered ? onColor : offColor);
    }

    // ---------------- API ----------------

    /// <summary>Sort le projecteur puis déploie l'écran holographique (clip Power_On).</summary>
    public void PowerOn()
    {
        if (IsPowered) return;
        IsPowered = true;
        Run(PowerRoutine("Power_On"));
    }

    /// <summary>Replie l'écran holographique puis rentre le projecteur (clip Power_Off).</summary>
    public void PowerOff()
    {
        if (!IsPowered) return;
        IsPowered = false;
        Run(PowerRoutine("Power_Off"));
    }

    /// <summary>Allume ou éteint selon <paramref name="powered"/> (sans effet si l'état est déjà le bon).</summary>
    public void SetPowered(bool powered)
    {
        if (powered) PowerOn(); else PowerOff();
    }

    public void Toggle() => SetPowered(!IsPowered);

    // ---------------- séquences ----------------

    void Run(IEnumerator r)
    {
        if (sequence != null) StopCoroutine(sequence);
        sequence = StartCoroutine(r);
    }

    IEnumerator PowerRoutine(string clip)
    {
        // Power_On et Power_Off sont symétriques : si l'autre clip est en cours, on repart de la position atteinte
        var st = animator.GetCurrentAnimatorStateInfo(0);
        string other = clip == "Power_On" ? "Power_Off" : "Power_On";
        float start = 0f;
        if (st.IsName(other) && st.normalizedTime < 1f) start = 1f - Mathf.Clamp01(st.normalizedTime);
        animator.Play(clip, 0, start);
        yield return new WaitForSeconds(Length(clip) * (1f - start));

        SetStatus(IsPowered ? onColor : offColor);
        sequence = null;
    }

    void Update()
    {
        if (IsBusy) SetStatus(onColor * Mathf.Lerp(0.15f, 1f, Mathf.PingPong(Time.time * blinkSpeed, 1f)));
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
