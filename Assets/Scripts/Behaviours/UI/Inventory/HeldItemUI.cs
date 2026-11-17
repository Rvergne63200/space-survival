using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Item held at the cursor in the interfaces (PlayerInventory.draggingSlot): element "held-item" of HeldItem.uxml,
// which follows the mouse over the other interfaces
public class HeldItemUI : ParentedUI
{
    [SerializeField] private UIDocument document;

    private ItemSlot<Item> draggingSlot;
    private VisualElement heldItem;

    public override void OnUpdateManager(UIManager manager)
    {
        draggingSlot?.ev_update.RemoveListener(OnUpdateDraggingSlot);
        draggingSlot = manager?.PlayerInventory?.draggingSlot;
        draggingSlot?.ev_update.AddListener(OnUpdateDraggingSlot);

        OnUpdateDraggingSlot(draggingSlot);
    }

    protected override void OnDestroy()
    {
        draggingSlot?.ev_update.RemoveListener(OnUpdateDraggingSlot);
        base.OnDestroy();
    }

    private VisualElement HeldItem
    {
        get
        {
            if (heldItem == null && document != null && document.rootVisualElement != null)
            {
                // Drawn over the interfaces: must never catch the clicks meant for their slots
                document.rootVisualElement.pickingMode = PickingMode.Ignore;
                heldItem = document.rootVisualElement.Q("held-item");
            }

            return heldItem;
        }
    }

    private void OnUpdateDraggingSlot(ItemSlot<Item> slot)
    {
        VisualElement element = HeldItem;

        if (element == null)
        {
            return;
        }

        bool empty = slot == null || slot.IsEmpty();
        element.EnableInClassList("hidden", empty);

        if (!empty)
        {
            ItemSlotUI.Display(slot, element.Q("slot-icon"), element.Q<Label>("slot-count"));
            Follow(element);
        }
    }

    private void Update()
    {
        VisualElement element = HeldItem;

        if (element != null && !element.ClassListContains("hidden"))
        {
            Follow(element);
        }
    }

    private void Follow(VisualElement element)
    {
        if (Mouse.current == null || element.panel == null)
        {
            return;
        }

        // Screen space has its origin at the bottom left, the panel at the top left
        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector2 position = RuntimePanelUtils.ScreenToPanel(element.panel, new Vector2(mouse.x, Screen.height - mouse.y));

        element.style.left = position.x;
        element.style.top = position.y;
    }
}
