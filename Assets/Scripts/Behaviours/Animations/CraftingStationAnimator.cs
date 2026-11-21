using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partie visuelle de la station de craft (modèle Resources/Models/CraftStation) :
/// allumage / extinction (animation + fondu des néons) et fabrication (bras + laser + anneau de scan).
/// À placer sur la racine de l'instance du modèle, à côté de l'Animator (CraftStation.controller).
/// Les hologrammes (Hologram, Hologram_Screen, Hologram_Ring) apparaissent / disparaissent via les animations.
/// Le contenu de l'écran se met sur <see cref="HologramScreen"/> (Hologram_Screen : UV 0..1, face avant vers le joueur).
/// </summary>
[RequireComponent(typeof(Animator))]
public class CraftingStationAnimator : MonoBehaviour
{
    [Header("Néons")]
    [Tooltip("Matériaux émissifs pilotés par l'alimentation (suffixe du nom de matériau).")]
    [SerializeField] string[] emissiveSuffixes = { "_Neon", "_Orange", "_Glow" };
    [SerializeField] float fadeInDuration = 1.2f;
    [SerializeField] float fadeOutDuration = 0.6f;
    [SerializeField] bool startPowered = false;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    Animator animator;
    readonly List<(Material mat, Color emission)> emissives = new List<(Material, Color)>();
    Coroutine fade, printRoutine;
    float glow;

    public bool IsPowered { get; private set; }
    public bool IsPrinting { get; private set; }
    /// <summary>Surface vierge de l'écran holographique (pour y afficher l'UI de craft).</summary>
    public Transform HologramScreen { get; private set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == "Hologram_Screen") { HologramScreen = t; break; }

        // instances de matériaux : l'asset partagé n'est pas modifié
        var cache = new Dictionary<Material, Material>();
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            var shared = r.sharedMaterials;
            var inst = r.materials;
            for (int i = 0; i < inst.Length; i++)
            {
                if (!IsEmissive(shared[i])) continue;
                if (!cache.TryGetValue(shared[i], out var m))
                {
                    m = inst[i];
                    m.EnableKeyword("_EMISSION");
                    cache[shared[i]] = m;
                    emissives.Add((m, m.GetColor(EmissionColor)));
                }
                inst[i] = m;
            }
            r.materials = inst;
        }

        IsPowered = startPowered;
        SetGlow(startPowered ? 1f : 0f);
        animator.Play(startPowered ? "Power_On" : "Power_Off", 0, 1f); // pose finale, sans jouer l'anim
    }

    bool IsEmissive(Material m)
    {
        if (m == null || !m.HasProperty(EmissionColor)) return false;
        string n = m.name.Replace(" (Instance)", "");
        foreach (var s in emissiveSuffixes) if (n.EndsWith(s)) return true;
        return false;
    }

    // ---------------- API ----------------

    public void PowerOn()
    {
        if (IsPowered) return;
        IsPowered = true;
        animator.CrossFadeInFixedTime("Power_On", 0.1f);
        StartFade(1f, fadeInDuration);
    }

    public void PowerOff()
    {
        if (!IsPowered) return;
        IsPrinting = false;
        if (printRoutine != null) { StopCoroutine(printRoutine); printRoutine = null; }
        IsPowered = false;
        animator.CrossFadeInFixedTime("Power_Off", 0.1f);
        StartFade(0f, fadeOutDuration);
    }

    /// <summary>Démarre la fabrication : Print_Start enchaîne automatiquement sur Print_Loop.</summary>
    public void StartPrint()
    {
        if (!IsPowered || IsPrinting) return;
        IsPrinting = true;
        animator.CrossFadeInFixedTime("Print_Start", 0.15f);
    }

    public void StopPrint()
    {
        if (!IsPrinting) return;
        IsPrinting = false;
        animator.CrossFadeInFixedTime("Print_End", 0.25f);
    }

    /// <summary>Fabrique pendant <paramref name="seconds"/> secondes puis s'arrête.</summary>
    public void PrintFor(float seconds)
    {
        if (printRoutine != null) StopCoroutine(printRoutine);
        printRoutine = StartCoroutine(PrintForRoutine(seconds));
    }

    IEnumerator PrintForRoutine(float seconds)
    {
        StartPrint();
        yield return new WaitForSeconds(seconds);
        StopPrint();
        printRoutine = null;
    }

    // ---------------- néons ----------------

    void StartFade(float target, float duration)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeRoutine(target, duration));
    }

    IEnumerator FadeRoutine(float target, float duration)
    {
        float from = glow, t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float v = Mathf.Lerp(from, target, k);
            if (target > from && k < 0.5f && Random.value < 0.35f) v *= 0.2f; // scintillement à l'allumage
            SetGlow(v);
            yield return null;
        }
        SetGlow(target);
        fade = null;
    }

    void SetGlow(float v)
    {
        glow = v;
        foreach (var (mat, emission) in emissives)
            mat.SetColor(EmissionColor, emission * v);
    }
}
