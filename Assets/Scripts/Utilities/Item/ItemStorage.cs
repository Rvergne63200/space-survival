using System.Collections.Generic;
using UnityEngine;

public class ItemStorage : MonoBehaviour, ISaveableComponent
{
    [SerializeField]
    protected ItemCollection<Item> _content;
    public ItemCollection<Item> Content { get => _content; private set => _content = value; }

    [System.Serializable]
    private class SaveState
    {
        public List<ItemSlotSaveData> slots = new List<ItemSlotSaveData>();
    }

    public string SaveKey => "itemStorage";

    // Number of slots created at Awake when the content has none (prefab without slots: crate, death bag...)
    [SerializeField] protected int size = 0;

    // Not called by PlayerInventory (its override does not call it): the player inventory is saved with the player
    protected virtual void Awake()
    {
        if (size > 0 && (_content == null || _content.Slots == null || _content.Slots.Count == 0))
        {
            _content = new ItemCollection<Item>(size);
        }

        Content = Content;

        // Every storage of the world (chests...) keeps its content in the save
        if (GetComponent<SaveableObject>() == null)
        {
            gameObject.AddComponent<SaveableObject>();
        }
    }

    public string CaptureState()
    {
        return JsonUtility.ToJson(new SaveState { slots = Content.CaptureSlots() });
    }

    public void RestoreState(string json)
    {
        if (SaveManager.Instance == null) return;

        SaveState state = JsonUtility.FromJson<SaveState>(json);
        Content.RestoreSlots(state.slots, SaveManager.Instance.GetItem);
    }

    public virtual int Add(Item item, int count = 1)
    {
        return Content.Add(item, count);
    }

    public virtual int Remove(Item item, int count = 1)
    {
        return Content.RemoveItem(item, count);
    }

    public virtual int Count(ItemData item)
    {
        return Content.Count(item);
    }
}
