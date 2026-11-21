using System.Collections;
using UnityEngine;

/// <summary>
/// Partie visuelle des modèles de stockage (Resources/Models/Crate_Small, Crate_Large, Cabinet).
/// Script commun : les trois modèles ont exactement la même logique, une paire de clips d'ouverture / fermeture
/// (Lid_Open / Lid_Close pour les caissons, Doors_Open / Doors_Close pour l'armoire), détectée dans le controller.
/// À placer sur la racine de l'instance, à côté de l'Animator (le .controller du modèle, une couche).
/// Empilage (caissons) : la racine du caisson du dessus se place sur <see cref="StackPoint"/> (enfant "StackPoint",
/// même rotation). Un caisson qui en porte un autre ne doit pas s'ouvrir : c'est au gameplay de le vérifier
/// (voir <see cref="CanOpen"/>).
/// </summary>
[RequireComponent(typeof(Animator))]
public class StorageAnimator : MonoBehaviour
{
    [SerializeField] bool startOpen = false;
    [Tooltip("Ouverture refusée tant que ce champ est faux (ex. un caisson est posé dessus).")]
    [SerializeField] bool canOpen = true;

    Animator animator;
    string openClip, closeClip;
    float openLength = 1f, closeLength = 1f;
    Coroutine sequence;

    /// <summary>Ouvert, ou en train de s'ouvrir.</summary>
    public bool IsOpen { get; private set; }
    /// <summary>Le couvercle ou les portes sont en mouvement.</summary>
    public bool IsBusy => sequence != null;
    /// <summary>Autorise l'ouverture (à passer à faux quand un caisson est empilé dessus). Fermer reste toujours possible.</summary>
    public bool CanOpen { get => canOpen; set => canOpen = value; }
    /// <summary>Point où poser la racine du caisson du dessus (null pour l'armoire).</summary>
    public Transform StackPoint { get; private set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == "StackPoint") StackPoint = t;
        if (animator.runtimeAnimatorController != null)
            foreach (var c in animator.runtimeAnimatorController.animationClips)
            {
                if (c.name.EndsWith("_Open")) { openClip = c.name; openLength = c.length; }
                else if (c.name.EndsWith("_Close")) { closeClip = c.name; closeLength = c.length; }
            }
        if (openClip == null || closeClip == null)
        {
            Debug.LogWarning($"{name} : clips *_Open / *_Close introuvables dans l'Animator Controller.", this);
            enabled = false;
            return;
        }
        IsOpen = startOpen && canOpen;
        animator.Play(IsOpen ? openClip : closeClip, 0, 1f);   // pose initiale sans animation
    }

    // ---------------- API ----------------
    /// <summary>Ouvre (sans effet si <see cref="CanOpen"/> est faux).</summary>
    public void Open() { if (canOpen) Run(true); }
    /// <summary>Ferme.</summary>
    public void Close() => Run(false);
    /// <summary>Bascule ouvert / fermé.</summary>
    public void Toggle() { if (IsOpen) Close(); else Open(); }

    // ---------------- séquence ----------------
    void Run(bool open)
    {
        if (!enabled) return;
        bool interrupted = sequence != null;
        if (interrupted) StopCoroutine(sequence);
        sequence = StartCoroutine(Move(open, interrupted));
    }

    IEnumerator Move(bool open, bool interrupted)
    {
        string clip = open ? openClip : closeClip;
        float length = open ? openLength : closeLength;
        float start = 0f;
        if (open != IsOpen)
        {
            if (interrupted)
            {
                // les clips d'ouverture et de fermeture sont symétriques : on repart de la position atteinte
                start = 1f - Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                animator.Play(clip, 0, start);
            }
            else animator.CrossFadeInFixedTime(clip, 0.05f);
            IsOpen = open;
        }
        else if (interrupted) start = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        else { sequence = null; yield break; }
        yield return new WaitForSeconds(length * (1f - start));
        sequence = null;
    }
}
