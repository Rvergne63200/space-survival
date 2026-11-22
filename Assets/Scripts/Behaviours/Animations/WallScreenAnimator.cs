using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partie visuelle de l'écran mural (modèle Resources/Models/WallScreen) : télévision incurvée accrochée au mur plein
/// des dômes. Modèle statique, sans Animator : à placer sur la racine de l'instance.
/// Origine du modèle : point de contact avec le mur, au milieu de la hauteur de l'écran ; le dos est contre le mur,
/// la face avant vers l'intérieur du dôme (+Z de la racine).
/// Écran (objet "Screen", matériau *_Glow, UV 0..1 sur la face avant) : allumé, il brille ; éteint, il est presque noir.
/// L'interface pourra s'afficher sur <see cref="ScreenSurface"/>.
/// Voyant (matériau *_Status, en bas à droite) : orange éteint, bleu allumé.
/// </summary>
public class WallScreenAnimator : MonoBehaviour
{
    [SerializeField] bool startPowered = false;
    [SerializeField] float fadeInDuration = 0.6f;
    [SerializeField] float fadeOutDuration = 0.3f;
    [Tooltip("Couleur de la dalle éteinte (sans émission).")]
    [SerializeField] Color offScreenColor = new Color(0.02f, 0.03f, 0.05f);

    [Header("Voyant")]
    [SerializeField] Color offColor = new Color(1.0f, 0.3f, 0.0f);
    [SerializeField] Color onColor = new Color(0.05f, 0.6f, 1.0f);
    [SerializeField] float statusIntensity = 2.5f;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    static readonly int BaseColor = Shader.PropertyToID("_Color");

    readonly List<Material> statusMats = new List<Material>();
    Material screenMat;
    Color screenOnColor, screenOnEmission;
    Coroutine fade;
    float glow;

    public bool IsPowered { get; private set; }
    /// <summary>Dalle de l'écran (UV 0..1 sur la face avant), pour y afficher une interface.</summary>
    public Transform ScreenSurface { get; private set; }

    void Awake()
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == "Screen") { ScreenSurface = t; break; }

        var r = ScreenSurface != null ? ScreenSurface.GetComponent<Renderer>() : null;
        if (r != null)
        {
            screenMat = r.material;                       // instance : l'asset partagé n'est pas modifié
            screenMat.EnableKeyword("_EMISSION");
            screenOnColor = screenMat.GetColor(BaseColor);
            screenOnEmission = screenMat.GetColor(EmissionColor);
        }
        else Debug.LogWarning($"{name} : objet Screen introuvable.", this);

        CollectStatusMaterials();
        IsPowered = startPowered;
        SetGlow(startPowered ? 1f : 0f);
    }

    // ---------------- API ----------------

    public void PowerOn()
    {
        if (IsPowered) return;
        IsPowered = true;
        StartFade(1f, fadeInDuration);
    }

    public void PowerOff()
    {
        if (!IsPowered) return;
        IsPowered = false;
        StartFade(0f, fadeOutDuration);
    }

    /// <summary>Allume ou éteint selon <paramref name="powered"/> (sans effet si l'état est déjà le bon).</summary>
    public void SetPowered(bool powered)
    {
        if (powered) PowerOn(); else PowerOff();
    }

    public void Toggle() => SetPowered(!IsPowered);

    // ---------------- interne ----------------

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
            SetGlow(Mathf.Lerp(from, target, Mathf.Clamp01(t / duration)));
            yield return null;
        }
        SetGlow(target);
        fade = null;
    }

    void SetGlow(float v)
    {
        glow = v;
        if (screenMat != null)
        {
            screenMat.SetColor(BaseColor, Color.Lerp(offScreenColor, screenOnColor, v));
            screenMat.SetColor(EmissionColor, screenOnEmission * v);
        }
        Color c = v > 0.5f ? onColor : offColor;
        foreach (var m in statusMats)
        {
            m.SetColor(EmissionColor, c * statusIntensity);
            if (m.HasProperty(BaseColor)) m.SetColor(BaseColor, Color.Lerp(Color.white, c, 0.6f));
        }
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
