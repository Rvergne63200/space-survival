using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInventory : ItemStorage
{
    // Clicks in an interface (inventory, storage) must not use the handed item
    public static string OPTION_USE_DISABLED = "player_item_use_disabled";

    private PlayerInputActions inputActions;
    private bool inInterface;
    public UnityEvent<bool, Action> ev_UpdateIsOpen;
    public UnityEvent<int> ev_UpdateHandedIndex;
    public ItemSlot<Item> draggingSlot;

    [SerializeField]
    protected ItemCollection<Item> _toolBar;
    public ItemCollection<Item> ToolBar { get => _toolBar; private set => _toolBar = value; }

    private int _handedIndex;
    public int HandedIndex { 
        get => _handedIndex;
        private set
        {
            _handedIndex = ((value % ToolBar.Slots.Count) + ToolBar.Slots.Count) % ToolBar.Slots.Count;
            ev_UpdateHandedIndex.Invoke(_handedIndex);
        }
    }

    public ItemSlot<Item> HandedSlot
    {
        get
        {
            return ToolBar.Slots[HandedIndex];
        }

        set
        {
            int index = ToolBar.Slots.IndexOf(value);
            HandedIndex = index != -1 ? index : HandedIndex;    
        }
    }

    private bool _isOpen;
    public bool IsOpen { 
        get { 
            return _isOpen; 
        }

        private set { 
            _isOpen = value;
            ev_UpdateIsOpen.Invoke(_isOpen, () => { IsOpen = false; });
        }
    }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        ToolBar = ToolBar;
        HandedIndex = 0;
    }

    private void OnEnable()
    {
        ModeManager.Instance.ev_OnUpdateOptions.AddListener(OnUpdateOptions);
        inInterface = ModeManager.Instance.IsEnabled(OPTION_USE_DISABLED);

        inputActions.Actions.Inventory.performed += OnInventoryPerformed;
        inputActions.Actions.Drop.performed += OnDropPerformed;
        inputActions.Actions.ChangeHandedItem.performed += OnChangeHandedItemPerformed;
        inputActions.Actions.Use1.performed += OnUse1Performed;
        inputActions.Actions.Use2.performed += OnUse2Performed;

        inputActions.Actions.Enable();
    }

    private void OnDisable()
    {
        ModeManager.Instance.ev_OnUpdateOptions.RemoveListener(OnUpdateOptions);

        inputActions.Actions.Disable();

        inputActions.Actions.Inventory.performed -= OnInventoryPerformed;
        inputActions.Actions.Drop.performed -= OnDropPerformed;
        inputActions.Actions.ChangeHandedItem.performed -= OnChangeHandedItemPerformed;
        inputActions.Actions.Use1.performed -= OnUse1Performed;
        inputActions.Actions.Use2.performed -= OnUse2Performed;
    }

    // When the last interface closes, the item held at the cursor is dropped instead of staying displayed
    private void OnUpdateOptions()
    {
        bool wasInInterface = inInterface;
        inInterface = ModeManager.Instance.IsEnabled(OPTION_USE_DISABLED);

        if (wasInInterface && !inInterface)
        {
            DropDraggingSlot();
        }
    }

    private void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        IsOpen = !IsOpen;
    }

    private void OnDropPerformed(InputAction.CallbackContext ctx)
    {
        if (IsOpen)
        {
            DropDraggingSlot();
        }
    }

    private void OnChangeHandedItemPerformed(InputAction.CallbackContext ctx)
    {
        ChangeHandedItem(ctx.control);
    }

    private void OnUse1Performed(InputAction.CallbackContext ctx)
    {
        if (ModeManager.Instance.IsEnabled(OPTION_USE_DISABLED)) return;

        Use1();
    }

    private void OnUse2Performed(InputAction.CallbackContext ctx)
    {
        if (ModeManager.Instance.IsEnabled(OPTION_USE_DISABLED)) return;

        Use2();
    }


    public override int Add(Item item, int count = 1)
    {
        int rest = base.Add(item, count);

        if (rest <= 0)
        {
            return rest;
        }

        return ToolBar.Add(item, rest);
    }

    public override int Remove(Item item, int count = 1)
    {
        int rest = base.Remove(item, count);

        if (rest <= 0)
        {
            return 0;
        }

        return ToolBar.RemoveItem(item, rest);
    }

    public override int Count(ItemData item)
    {
        return base.Count(item) + ToolBar.Count(item);
    }


    public void DropDraggingSlot()
    {
        if (draggingSlot == null || draggingSlot.IsEmpty())
        {
            return;
        }

        // This component lives on the PlayerManager: drop in front of the player, not at the manager position
        GameObject player = PlayerManager.Instance != null ? PlayerManager.Instance.Player : null;
        draggingSlot.Drop(draggingSlot.Quantity, player != null ? player : gameObject);
    }

    public void ChangeHandedItem(InputControl inputControl)
    {
        Vector2 scroll = (Vector2)inputControl.ReadValueAsObject();
        HandedIndex -= (int)(scroll.y / Mathf.Abs(scroll.y));
    }


    public void Use1()
    {

    }

    public void Use2()
    {
        ItemSlot<Item> slot = HandedSlot;

        if (!HandedSlot.IsEmpty() && HandedSlot.Item.Data is ConsummableData)
        {
            ConsummableData data = (ConsummableData) HandedSlot.Item.Data;
            data.Consume(gameObject);

            HandedSlot.Remove(1);
        }
    }


    // Right click: with an item held, place one unit on the slot; with nothing held, take half of the slot
    public void SplitOrPlaceOne(ItemSlot<Item> slot)
    {
        if (slot == null)
        {
            return;
        }

        if (!draggingSlot.IsEmpty())
        {
            if (!slot.IsEmpty() && slot.Item.Data.Id != draggingSlot.Item.Data.Id)
            {
                return;
            }

            // Add returns the rest: 0 means the unit fitted in the slot
            if (slot.Add(draggingSlot.Item, 1) == 0)
            {
                draggingSlot.Remove(1);
            }
        }
        else if (!slot.IsEmpty())
        {
            Item item = slot.Item;
            int half = Mathf.CeilToInt(slot.Quantity / 2f);

            // Only remove from the slot what the cursor actually took
            int rest = draggingSlot.Add(item, half);
            slot.Remove(half - rest);
        }
    }

    // Shift click: move the whole slot to the other side.
    // With a searched storage: storage -> tool bar (then inventory), tool bar or inventory -> storage.
    // Without: inventory <-> tool bar.
    public void QuickTransfer(ItemSlot<Item> slot, ItemCollection<Item> storage)
    {
        if (slot == null || slot.IsEmpty())
        {
            return;
        }

        ItemCollection<Item>[] targets;

        if (storage != null && storage.Slots.Contains(slot))
        {
            targets = new[] { ToolBar, Content };
        }
        else if (ToolBar.Slots.Contains(slot))
        {
            targets = new[] { storage ?? Content };
        }
        else if (Content.Slots.Contains(slot))
        {
            targets = new[] { storage ?? ToolBar };
        }
        else
        {
            return;
        }

        Item item = slot.Item;
        int count = slot.Quantity;

        slot.Clear();

        foreach (ItemCollection<Item> target in targets)
        {
            count = target.Add(item, count);

            if (count <= 0)
            {
                break;
            }
        }

        // What did not fit stays in the source slot
        if (count > 0)
        {
            slot.Add(item, count);
        }
    }

    public void TryMerge(ItemSlot<Item> slot, Direction dir)
    {
        if(slot?.Item?.Data?.Id == draggingSlot?.Item?.Data?.Id)
        {
            ItemSlot<Item> source;
            ItemSlot<Item> target;

            if(dir == Direction.Out)
            {
                source = draggingSlot;
                target = slot;
            }
            else
            {
                source = slot;
                target = draggingSlot;
            }

            int quantity = source.Quantity;
            Item item = source.Item;

            source.Clear();

            int rest = target.Add(item, quantity);

            if (rest > 0)
            {
                source.Add(item, rest);
            }
        }
        else
        {
            draggingSlot.Switch(slot);
        }
    }
}