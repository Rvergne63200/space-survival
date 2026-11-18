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
    private readonly VisualElement gauge;
    private bool hovered;

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
        gauge = element.Q("slot-gauge");

        element.RegisterCallback<PointerDownEvent>(OnPointerDown);

        // Information of the item while the mouse is over the slot
        element.RegisterCallback<PointerEnterEvent>(evt =>
        {
            hovered = true;
            UpdateTooltip();
        });
        element.RegisterCallback<PointerLeaveEvent>(evt =>
        {
            hovered = false;
            ItemTooltip.Hide(Element);
        });
        element.RegisterCallback<DetachFromPanelEvent>(evt => hovered = false);
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
        Display(slot, icon, count, gauge);

        // Live value of the bar (charging, discharging) while hovered
        if (hovered) UpdateTooltip();
    }

    private void UpdateTooltip()
    {
        if (slot == null || slot.IsEmpty())
        {
            ItemTooltip.Hide(Element);
            return;
        }

        ItemTooltip.Show(Element, slot.Item);
    }

    // Also used for the item held at the cursor (HeldItemUI)
    public static void Display(ItemSlot<Item> slot, VisualElement icon, Label count, VisualElement gauge = null)
    {
        // IsEmpty also covers an item whose data is missing (asset not found): displayed as empty instead of failing
        if (slot == null || slot.IsEmpty())
        {
            if (icon != null) icon.style.backgroundImage = StyleKeyword.None;
            if (count != null) count.style.display = DisplayStyle.None;
            if (gauge != null) gauge.style.display = DisplayStyle.None;
            return;
        }

        DisplayGauge(slot.Item, gauge);

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

    // Bar at the bottom of the slot (element "slot-gauge", its child "slot-gauge-fill"): blue for a charge, green then
    // orange (50 %) then red (20 %) for a wear
    private static readonly Color ChargeColor = new Color(0.25f, 0.6f, 1f);
    private static readonly Color WearGoodColor = new Color(0.3f, 0.85f, 0.4f);
    private static readonly Color WearMediumColor = new Color(1f, 0.62f, 0.15f);
    private static readonly Color WearBadColor = new Color(0.95f, 0.25f, 0.25f);

    private static void DisplayGauge(Item item, VisualElement gauge)
    {
        if (gauge == null) return;

        if (item == null || !item.HasGauge)
        {
            gauge.style.display = DisplayStyle.None;
            return;
        }

        float ratio = Mathf.Clamp01(item.GaugeRatio);
        Color color = item.Data.Gauge.Rechargeable ? ChargeColor : ratio > 0.5f ? WearGoodColor : ratio > 0.2f ? WearMediumColor : WearBadColor;

        gauge.style.display = DisplayStyle.Flex;

        VisualElement fill = gauge.Q("slot-gauge-fill");

        if (fill != null)
        {
            fill.style.width = Length.Percent(ratio * 100f);
            fill.style.backgroundColor = color;
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
