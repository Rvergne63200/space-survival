using UnityEngine;
using UnityEngine.Events;

// Container left where the player died, with its inventory. Searched like a chest (Searchable),
// destroyed once empty. Created in game: saved as a runtime object (SaveManager.runtimePrefabs)
public class DeathBag : ItemStorage
{
    [SerializeField] private int size = 40;

    private UnityAction<ItemSlot<Item>> onSlotUpdated;
    private bool checkEmpty;

    protected override void Awake()
    {
        // The content is created here: the prefab has no slots
        if (_content == null || _content.Slots == null || _content.Slots.Count == 0)
        {
            _content = new ItemCollection<Item>(size);
        }

        base.Awake();

        // Taking an item (click, shift click...) changes the slots directly, without the collection event
        onSlotUpdated = slot => checkEmpty = true;

        foreach (ItemSlot<Item> slot in _content.Slots)
        {
            slot?.ev_update.AddListener(onSlotUpdated);
        }

        _content.ev_Update.AddListener(OnContentUpdated);
    }

    private void OnDestroy()
    {
        if (_content == null) return;

        foreach (ItemSlot<Item> slot in _content.Slots)
        {
            slot?.ev_update.RemoveListener(onSlotUpdated);
        }

        _content.ev_Update.RemoveListener(OnContentUpdated);
    }

    private void OnContentUpdated()
    {
        checkEmpty = true;
    }

    // Checked at the end of the frame: a slot is briefly empty during a transfer, or while the save restores the content
    private void LateUpdate()
    {
        if (!checkEmpty) return;

        checkEmpty = false;

        foreach (ItemSlot<Item> slot in _content.Slots)
        {
            if (slot != null && !slot.IsEmpty()) return;
        }

        // Empty: close it if the player is searching it, then remove it
        Searcher searcher = PlayerManager.Instance != null && PlayerManager.Instance.Player != null
            ? PlayerManager.Instance.Player.GetComponent<Searcher>()
            : null;

        if (searcher != null && searcher.CurrentSearchedStorage == this)
        {
            searcher.ClearCurrentSearchedStorage();
        }

        Destroy(gameObject);
    }
}
