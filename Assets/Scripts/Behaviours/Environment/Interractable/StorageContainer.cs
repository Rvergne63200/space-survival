using System;
using UnityEngine;

// Placed storage (items 8 Large Crate, 31 Small Crate, 32 Cabinet; models Resources/Models/Crate_Large, Crate_Small,
// Cabinet, moved by their StorageAnimator on the model child):
// - Interaction: opens its storage next to the inventory (Searcher) and opens the lid or the doors; closing the interface
//   closes them
// - Stacking (crates, "stackable"): a crate can be placed on a crate of the same size or larger (Builder), its root on
//   the StackPoint of the one below. A crate carrying another one does not open (StorageAnimator.CanOpen) and cannot be
//   destroyed before the one on it. If the one below disappears anyway, the one on it is put down on the ground
// - Save: the id of the crate below (found again like the cables)
[RequireComponent(typeof(ItemStorage))]
public class StorageContainer : MonoBehaviour, IInterractable, ISaveableComponent
{
    [SerializeField] private bool stackable;

    // Crates of a lower size (or the same) can be stacked on it
    [SerializeField] private int stackSize;

    private ItemStorage storage;
    private StorageAnimator storageAnimator;
    private Searcher searcher;

    private StorageContainer below;
    private string belowId;

    public StorageContainer Above { get; private set; }
    public StorageContainer Below => below;
    public bool Stackable => stackable;
    public int StackSize => stackSize;
    public ItemStorage Storage => storage;

    // Where the root of a crate stacked on it goes (StackPoint of the model), null when it cannot carry one
    public Transform StackPoint => stackable && storageAnimator != null ? storageAnimator.StackPoint : null;

    private void Awake()
    {
        storage = GetComponent<ItemStorage>();
        storageAnimator = GetComponentInChildren<StorageAnimator>();
    }

    private void Update()
    {
        // Loaded from the save: the crate below registers after or before this one
        if (below == null && !string.IsNullOrEmpty(belowId) && SaveManager.Instance != null)
        {
            SaveableObject saveable = SaveManager.Instance.GetLiveObject(belowId);
            StorageContainer container = saveable != null ? saveable.GetComponent<StorageContainer>() : null;
            if (container != null) StackOn(container);
        }

        if (storageAnimator != null) storageAnimator.CanOpen = Above == null;

        // The interface is closed (or shows another storage): the lid or the doors close
        if (searcher != null && searcher.CurrentSearchedStorage != storage)
        {
            searcher = null;
            if (storageAnimator != null) storageAnimator.Close();
        }
    }

    // ---------- Stacking ----------

    // This crate can carry the other one: free on top, stackable, the other not larger
    public bool CanCarry(StorageContainer other)
    {
        return stackable && other != null && other.stackable && Above == null && other.stackSize <= stackSize && StackPoint != null;
    }

    // Placed on the crate below (the pose is the one of its StackPoint, set by the placement)
    public void StackOn(StorageContainer container)
    {
        if (container == null || container == this) return;

        below = container;
        belowId = IdOf(container);
        container.Above = this;
    }

    private static string IdOf(Component component)
    {
        SaveableObject saveable = component.GetComponent<SaveableObject>();
        return saveable != null ? saveable.Id : null;
    }

    private void OnDestroy()
    {
        if (below != null && below.Above == this) below.Above = null;

        // The crate below disappears: the one on it is put down on what is under it
        if (Above != null && Above.below == this && gameObject.scene.isLoaded)
        {
            StorageContainer upper = Above;
            upper.below = null;
            upper.belowId = null;
            upper.PutDown(this);
        }
    }

    private void PutDown(StorageContainer ignored)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 50f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.GetComponentInParent<StorageContainer>() == this || hit.collider.GetComponentInParent<StorageContainer>() == ignored) continue;

            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            return;
        }
    }

    // ---------- Interaction ----------

    public string GetInfo()
    {
        return Above != null ? "Une caisse est posée dessus" : "Ouvrir";
    }

    public void Interract(GameObject interractor)
    {
        if (Above != null)
        {
            NotificationUI.Show("Enlevez d'abord la caisse posée dessus.", NotificationType.Warning);
            return;
        }

        Searcher interfaceSearcher = interractor.GetComponent<Searcher>();

        if (interfaceSearcher == null) return;

        interfaceSearcher.Search(storage);

        if (interfaceSearcher.CurrentSearchedStorage == storage)
        {
            searcher = interfaceSearcher;
            if (storageAnimator != null) storageAnimator.Open();
        }
        else if (storageAnimator != null)
        {
            storageAnimator.Close();
        }
    }

    // ---------- Save ----------

    [Serializable]
    private class State
    {
        public string belowId;
    }

    public string SaveKey => "storageContainer";

    public string CaptureState() => JsonUtility.ToJson(new State { belowId = belowId });

    public void RestoreState(string json)
    {
        State state = JsonUtility.FromJson<State>(json);
        if (state != null) belowId = state.belowId;
    }
}
