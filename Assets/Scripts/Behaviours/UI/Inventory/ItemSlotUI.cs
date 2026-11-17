using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// One item slot of an interface (element "slot" of ItemSlot.uxml): shows the item of an ItemSlot and handles the clicks.
// Left click: take / put back / swap with the item held at the cursor, Shift + left click: quick transfer,
// double click: gather the same items at the cursor, right click: take half / place one
public class ItemSlotUI
{
    public VisualElement Element { get; }

    private readonly ParentUI parentUI;
    private readonly VisualElement icon;
    private readonly Label count;

    private ItemSlot<Item> slot;

    // Double click: two left clicks on the same slot UI within this delay (seconds)
    private const float DoubleClickDelay = 0.35f;
    private static ItemSlotUI lastClickedSlot;
    private static float lastClickTime;

    public ItemSlotUI(VisualElement element, ParentUI parentUI)
    {
        Element = element;
        this.parentUI = parentUI;

        icon = element.Q("slot-icon");
        count = element.Q<Label>("slot-count");

        element.RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    public void SetSlot(ItemSlot<Item> slot)
    {
        // The slot UIs are reused on each collection update: only subscribe when the slot changes
        if (this.slot != slot)
        {
            this.slot?.ev_update.RemoveListener(UpdateSlot);

            this.slot = slot;

            slot?.ev_update.AddListener(UpdateSlot);
        }

        UpdateSlot(slot);
    }

    // The ItemSlot outlives this UI (it belongs to a persistent ItemCollection)
    public void Unbind()
    {
        slot?.ev_update.RemoveListener(UpdateSlot);
        slot = null;
    }

    private void UpdateSlot(ItemSlot<Item> slot)
    {
        Display(slot, icon, count);
    }

    // Also used for the item held at the cursor (HeldItemUI)
    public static void Display(ItemSlot<Item> slot, VisualElement icon, Label count)
    {
        // IsEmpty also covers an item whose data is missing (asset not found): displayed as empty instead of failing
        if (slot == null || slot.IsEmpty())
        {
            if (icon != null) icon.style.backgroundImage = StyleKeyword.None;
            if (count != null) count.style.display = DisplayStyle.None;
            return;
        }

        Sprite sprite = slot.Item.Data.Sprite;

        if (icon != null)
        {
            icon.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground(StyleKeyword.None);
        }

        if (count != null)
        {
            count.text = slot.Quantity.ToString();
            count.style.display = slot.Item.Data.StackMax > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        PlayerInventory inventory = parentUI != null && parentUI.UIManager != null ? parentUI.UIManager.PlayerInventory : null;

        if (inventory == null)
        {
            return;
        }

        if (slot == null)
        {
            SetSlot(new ItemSlot<Item>());
        }

        if (evt.button == 0)
        {
            bool doubleClick = IsDoubleClick();

            // The modifiers of the event are not always filled with the Input System: the keyboard is also read
            bool shift = evt.shiftKey || (Keyboard.current != null && Keyboard.current.shiftKey.isPressed);

            if (shift)
            {
                inventory.QuickTransfer(slot, GetSearchedStorage());
            }
            // Double click: the first click took the stack at the cursor, the second gathers the same items into it
            // (instead of putting the stack back in the slot)
            else if (doubleClick && !inventory.draggingSlot.IsEmpty())
            {
                inventory.GatherToCursor(GetSearchedStorage());
            }
            else
            {
                inventory.TryMerge(slot, Direction.Out);
            }

            evt.StopPropagation();
        }
        else if (evt.button == 1)
        {
            inventory.SplitOrPlaceOne(slot);
            evt.StopPropagation();
        }
    }

    // Not evt.clickCount: through the EventSystem it counts the clicks on the whole panel, whatever the slot,
    // so taking an item and putting it in another slot quickly was seen as a double click (the item stayed at the cursor)
    private bool IsDoubleClick()
    {
        float now = Time.unscaledTime;
        bool doubleClick = lastClickedSlot == this && now - lastClickTime <= DoubleClickDelay;

        // A third click starts a new sequence
        lastClickedSlot = doubleClick ? null : this;
        lastClickTime = now;

        return doubleClick;
    }

    // Content of the storage currently searched by the player, if any
    private ItemCollection<Item> GetSearchedStorage()
    {
        PlayerContext context = parentUI.UIManager.PlayerContext;

        if (context == null)
        {
            return null;
        }

        Searcher searcher = context.GetComponent<Searcher>();

        if (searcher == null || searcher.CurrentSearchedStorage == null)
        {
            return null;
        }

        return searcher.CurrentSearchedStorage.Content;
    }
}
