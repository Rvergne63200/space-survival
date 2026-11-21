using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partie visuelle du sas (modèle Resources/Models/Airlock) : portes coulissantes et cycle de pressurisation.
/// À placer sur la racine de l'instance du modèle, à côté de l'Animator (Airlock.controller).
/// Règle du sas : jamais les deux portes ouvertes en même temps (chaque clip ferme l'autre porte).
/// Les voyants (matériaux *_Status) sont orange côté extérieur / pendant le cycle, bleus quand le sas est pressurisé.
/// Pose : l'origine du sas se place sur l'enfant "Socket" d'un DomeConnector (même rotation).
/// </summary>
[RequireComponent(typeof(Animator))]
public class AirlockAnimator : MonoBehaviour
{
    [SerializeField] Color depressurizedColor = new Color(1.0f, 0.3f, 0.0f);
    [SerializeField] Color pressurizedColor = new Color(0.05f, 0.6f, 1.0f);
    [SerializeField] float statusIntensity = 2.5f;
    [SerializeField] float blinkSpeed = 4f;
    [SerializeField] bool startPressurized = true;
    [Tooltip("Vitesse du cycle de pressurisation / dépressurisation (1 = durée du clip Cycle).")]
    [SerializeField] float cycleSpeed = 2f;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColor = Shader.PropertyToID("_Color");

    Animator animator;
    readonly List<Material> statusMats = new List<Material>();
    readonly Dictionary<string, float> clipLength = new Dictionary<string, float>();
    readonly Queue<IEnumerator> queue = new Queue<IEnumerator>();
    Coroutine runner;
    bool blinking;

    public bool OuterOpen { get; private set; }
    public bool InnerOpen { get; private set; }
    public bool Pressurized { get; private set; }
    public bool IsBusy => runner != null;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator.runtimeAnimatorController != null)
            foreach (var c in animator.runtimeAnimatorController.animationClips) clipLength[c.name] = c.length;

        var cache = new Dictionary<Material, Material>();
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            var shared = r.sharedMaterials; var inst = r.materials;
            for (int i = 0; i < inst.Length; i++)
            {
                if (shared[i] == null || !shared[i].name.EndsWith("_Status")) continue;
                if (!cache.TryGetValue(shared[i], out var m)) { m = inst[i]; m.EnableKeyword("_EMISSION"); cache[shared[i]] = m; statusMats.Add(m); }
                inst[i] = m;
            }
            r.materials = inst;
        }
        Pressurized = startPressurized;
        SetStatus(Pressurized ? pressurizedColor : depressurizedColor);
        animator.Play("Inner_Close", 0, 1f);   // pose de repos : tout fermé
    }

    void Update()
    {
        if (!blinking) return;
        float k = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        SetStatus(depressurizedColor * Mathf.Lerp(0.15f, 1f, k));
    }

    // ---------------- API bas niveau (mises en file, jouées l'une après l'autre) ----------------
    public void OpenOuter()  => Enqueue(Door("Outer_Open",  outer: true,  inner: false, needDepressurized: true));
    public void CloseOuter() => Enqueue(Door("Outer_Close", outer: false, inner: false));
    public void OpenInner()  => Enqueue(Door("Inner_Open",  outer: false, inner: true,  needPressurized: true));
    public void CloseInner() => Enqueue(Door("Inner_Close", outer: false, inner: false));
    public void Pressurize()   => Enqueue(Cycle(true));
    public void Depressurize() => Enqueue(Cycle(false));

    // ---------------- séquences complètes ----------------
    /// <summary>Joueur dehors : ouvre la porte extérieure (dépressurise d'abord si besoin).</summary>
    public void RequestEnterFromOutside() { if (InnerOpen) CloseInner(); if (Pressurized) Depressurize(); OpenOuter(); }
    /// <summary>Joueur dans le sas, venant de dehors : ferme, pressurise, ouvre côté dôme.</summary>
    public void RequestPassInside() { CloseOuter(); Pressurize(); OpenInner(); }
    /// <summary>Joueur dans le dôme : ouvre la porte intérieure.</summary>
    public void RequestEnterFromInside() { if (OuterOpen) CloseOuter(); if (!Pressurized) Pressurize(); OpenInner(); }
    /// <summary>Joueur dans le sas, venant du dôme : ferme, dépressurise, ouvre dehors.</summary>
    public void RequestPassOutside() { CloseInner(); Depressurize(); OpenOuter(); }

    // ---------------- interne ----------------
    void Enqueue(IEnumerator step)
    {
        queue.Enqueue(step);
        if (runner == null) runner = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (queue.Count > 0) yield return StartCoroutine(queue.Dequeue());
        runner = null;
    }

    IEnumerator Door(string clip, bool outer, bool inner, bool needPressurized = false, bool needDepressurized = false)
    {
        if (needPressurized && !Pressurized) yield return Cycle(true);
        if (needDepressurized && Pressurized) yield return Cycle(false);
        bool already = (clip == "Outer_Open" && OuterOpen) || (clip == "Inner_Open" && InnerOpen) ||
                       (clip == "Outer_Close" && !OuterOpen) || (clip == "Inner_Close" && !InnerOpen);
        if (already) yield break;
        animator.CrossFadeInFixedTime(clip, 0.05f);
        OuterOpen = outer; InnerOpen = inner;
        yield return new WaitForSeconds(Length(clip));
    }

    IEnumerator Cycle(bool pressurize)
    {
        if (OuterOpen || InnerOpen)   // le cycle se fait portes fermées
        {
            animator.CrossFadeInFixedTime(OuterOpen ? "Outer_Close" : "Inner_Close", 0.05f);
            float l = Length(OuterOpen ? "Outer_Close" : "Inner_Close");
            OuterOpen = InnerOpen = false;
            yield return new WaitForSeconds(l);
        }
        blinking = true;
        animator.CrossFadeInFixedTime("Cycle", 0.05f);
        float speed = Mathf.Max(cycleSpeed, 0.01f);
        animator.speed = speed;                    // le cycle se fait portes fermées : seul ce clip joue
        yield return new WaitForSeconds(Length("Cycle") / speed);
        animator.speed = 1f;
        blinking = false;
        Pressurized = pressurize;
        SetStatus(Pressurized ? pressurizedColor : depressurizedColor);
    }

    float Length(string clip) => clipLength.TryGetValue(clip, out var l) ? l : 1f;

    void SetStatus(Color c)
    {
        foreach (var m in statusMats)
        {
            m.SetColor(EmissionColor, c * statusIntensity);
            if (m.HasProperty(BaseColor)) m.SetColor(BaseColor, Color.Lerp(Color.white, c, 0.6f));
        }
    }
}
