using System;
using System.Collections.Generic;
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

    // Restores the selected tool bar slot (save)
    public void SetHandedIndex(int index)
    {
        HandedIndex = index;
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

            if (!value)
            {
                openers.Clear();
                openedByPlayer = false;
            }

            ev_UpdateIsOpen.Invoke(_isOpen, () => { IsOpen = false; });
        }
    }

    // Interfaces that opened the inventory with them (storage, crafting station): it closes with the last of them,
    // unless the player opened it with the Inventory key
    private readonly HashSet<object> openers = new HashSet<object>();
    private bool openedByPlayer;

    public void OpenWith(object opener)
    {
        if (!HasPlayer) return;

        openers.Add(opener);

        if (!IsOpen)
        {
            IsOpen = true;
        }
    }

    public void CloseWith(object opener)
    {
        if (!openers.Remove(opener)) return;

        if (openers.Count == 0 && IsOpen && !openedByPlayer)
        {
            IsOpen = false;
        }
    }

    // Does not call ItemStorage.Awake: the player inventory is not a saveable object of the world (saved with the player)
    protected override void Awake()
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
        inputActions.Actions.Use1.canceled += OnUse1Canceled;
        inputActions.Actions.Use2.canceled += OnUse2Canceled;

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
        inputActions.Actions.Use1.canceled -= OnUse1Canceled;
        inputActions.Actions.Use2.canceled -= OnUse2Canceled;

        StopHolds(null);
        Unequip();
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

    // This component is on the PlayerManager, which persists in the main menu: no inventory without player
    private bool HasPlayer => PlayerManager.Instance != null && PlayerManager.Instance.Player != null;

    private void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        if (!HasPlayer) return;

        IsOpen = !IsOpen;
        openedByPlayer = IsOpen;
    }

    private void OnDropPerformed(InputAction.CallbackContext ctx)
    {
        if (IsOpen && HasPlayer)
        {
            DropDraggingSlot();
        }
    }

    private void OnChangeHandedItemPerformed(InputAction.CallbackContext ctx)
    {
        if (!HasPlayer) return;

        // Not while an interface (inventory, control panel...) or a menu is open: the wheel belongs to it (zoom, lists)
        if (InterfaceManager.GetInstance().HasOpenInterface) return;
        if (ModeManager.Instance != null && (ModeManager.Instance.Mode & (PlayingMode.Interface | PlayingMode.Menu)) != 0) return;

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

    private void OnUse1Canceled(InputAction.CallbackContext ctx)
    {
        StopHolds(true);
    }

    private void OnUse2Canceled(InputAction.CallbackContext ctx)
    {
        StopHolds(false);
    }


    // A slot already holding the item has priority (tool bar, then inventory), then the empty slots (tool bar, then inventory)
    public override int Add(Item item, int count = 1)
    {
        int rest = ToolBar.AddToStacks(item, count);
        rest = Content.AddToStacks(item, rest);
        rest = ToolBar.AddToEmptySlots(item, rest);
        rest = Content.AddToEmptySlots(item, rest);

        return rest;
    }

    // From the inventory, then from the tool bar for what is missing (only where the item is)
    public override int Remove(Item item, int count = 1)
    {
        int rest = count;

        if (Content.Count(item) > 0)
        {
            rest = base.Remove(item, rest);
        }

        if (rest > 0 && ToolBar.Count(item) > 0)
        {
            rest = ToolBar.RemoveItem(item, rest);
        }

        return Mathf.Max(0, rest);
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

        // A horizontal scroll has y == 0 (Mathf.Sign(0) would return 1)
        if (scroll.y == 0f) return;

        HandedIndex -= (int)Mathf.Sign(scroll.y);
    }


    // Primary use of the item in hand (left click): the primary actions of a tool (ToolData)
    public void Use1()
    {
        UseHandedItem(true);
    }

    // Secondary use of the item in hand (right click): the secondary actions of a tool, or the consumption of a ConsummableData
    public void Use2()
    {
        if (UseHandedItem(false)) return;

        if (!HandedSlot.IsEmpty() && HandedSlot.Item.Data is ConsummableData)
        {
            ConsummableData data = (ConsummableData) HandedSlot.Item.Data;
            data.Consume(gameObject);

            HandedSlot.Remove(1);
        }
    }

    private bool UseHandedItem(bool primary)
    {
        ItemSlot<Item> slot = HandedSlot;

        // Only the tools have actions
        if (slot.IsEmpty() || !(slot.Item.Data is ToolData tool)) return false;

        // The item in hand may have changed since the last frame
        UpdateEquipped();

        // Empty: unusable until refilled (a worn item is already destroyed). Only when it has an action on this button
        if (slot.Item.IsGaugeEmpty && tool.Actions(primary).Count > 0)
        {
            NotificationUI.Show(tool.Name + " " + tool.Gauge.EmptyText + ".", NotificationType.Warning);
            return true;
        }

        ItemUseContext context = CreateUseContext(slot);
        List<HoldItemAction> startedHolds = new List<HoldItemAction>();

        bool done = tool.Use(primary, context, startedHolds);

        foreach (HoldItemAction action in startedHolds)
        {
            holds.Add(new HeldAction { Action = action, Primary = primary, Context = context, StartTime = Time.time });
        }

        if (done && tool.ConsumptionMode == ToolConsumptionMode.PerUse && slot.Item != null && slot.Item.HasGauge)
        {
            ConsumeGauge(slot, tool.Consumption);
        }

        return done;
    }

    private ItemUseContext CreateUseContext(ItemSlot<Item> slot)
    {
        Camera view = CameraManager.Instance != null && CameraManager.Instance.Camera != null
            ? CameraManager.Instance.Camera.GetComponentInChildren<Camera>()
            : null;

        return new ItemUseContext
        {
            Manager = PlayerManager.Instance,
            Player = PlayerManager.Instance != null ? PlayerManager.Instance.Player : null,
            Slot = slot,
            View = view != null ? view.transform : null
        };
    }

    // ---------- Item in hand: toggles and holds ----------

    private class HeldAction
    {
        public HoldItemAction Action;
        public bool Primary;
        public ItemUseContext Context;
        public float StartTime;
    }

    // Hold actions running while their button is held
    private readonly List<HeldAction> holds = new List<HeldAction>();

    // Data of the item in hand the last time it was checked, with the context of its use (for OnUnequip)
    private ItemData equippedData;
    private Item equippedItem;
    private ItemUseContext equippedContext;

    private void Update()
    {
        UpdateEquipped();
        UpdateGauge();

        if (holds.Count == 0) return;

        // An interface opened, or the construction mode started, while the button was held
        if (ModeManager.Instance.IsEnabled(OPTION_USE_DISABLED))
        {
            StopHolds(null);
            return;
        }

        for (int i = holds.Count - 1; i >= 0; i--)
        {
            HeldAction held = holds[i];

            if (!held.Action.Hold(held.Context, Time.time - held.StartTime))
            {
                holds.RemoveAt(i);
                held.Action.End(held.Context, true);
            }
        }
    }

    // ---------- Bar of the item in hand (ItemGauge) ----------

    // Tool with a continuous consumption: while it is active (a toggle action on, a hold running), its bar goes down
    private void UpdateGauge()
    {
        ItemSlot<Item> slot = HandedSlot;

        if (equippedData == null || slot == null || slot.IsEmpty() || !slot.Item.HasGauge) return;
        if (!(slot.Item.Data is ToolData tool) || tool.ConsumptionMode != ToolConsumptionMode.Continuous) return;
        if (holds.Count == 0 && !tool.IsToggledOn()) return;

        ConsumeGauge(slot, tool.Gauge.PerSecond(tool.Consumption) * Time.deltaTime);
    }

    // At 0: a rechargeable item stops (its actions are switched off), a worn item is destroyed
    private void ConsumeGauge(ItemSlot<Item> slot, float amount)
    {
        if (amount <= 0f) return;

        Item item = slot.Item;
        item.ConsumeGauge(amount);
        slot.NotifyChanged();

        if (!item.IsGaugeEmpty) return;

        StopHolds(null);

        if (item.GaugeSettings.Rechargeable)
        {
            (item.Data as ToolData)?.Unequip(equippedContext != null ? equippedContext : CreateUseContext(slot));
            NotificationUI.Show(item.Data.Name + " " + item.GaugeSettings.EmptyText + ".", NotificationType.Warning);
        }
        else
        {
            NotificationUI.Show(item.Data.Name + " s'est cassé.", NotificationType.Warning);
            slot.Remove(1);
        }
    }

    // When the item in hand changes (other slot, slot emptied or filled, player gone), the previous one is unequipped
    private void UpdateEquipped()
    {
        ItemSlot<Item> slot = HandedSlot;
        ItemData data = HasPlayer && slot != null && !slot.IsEmpty() ? slot.Item.Data : null;
        Item item = data != null ? slot.Item : null;

        // Also another item of the same kind (two flashlights: each has its own charge)
        if (data == equippedData && item == equippedItem) return;

        StopHolds(null);
        Unequip();

        equippedData = data;
        equippedItem = item;
        equippedContext = data != null ? CreateUseContext(slot) : null;
    }

    private void Unequip()
    {
        if (equippedData is ToolData tool)
        {
            tool.Unequip(equippedContext ?? CreateUseContext(HandedSlot));
        }

        equippedData = null;
        equippedItem = null;
        equippedContext = null;
    }

    // primary: the holds of this button only, null: all of them
    private void StopHolds(bool? primary)
    {
        for (int i = holds.Count - 1; i >= 0; i--)
        {
            HeldAction held = holds[i];

            if (primary.HasValue && held.Primary != primary.Value) continue;

            holds.RemoveAt(i);
            held.Action.End(held.Context, false);
        }
    }


    // Double click: the items of the same kind as the one held at the cursor are gathered into it, up to a full stack,
    // from the inventory, the tool bar and the searched storage
    public void GatherToCursor(ItemCollection<Item> storage)
    {
        if (draggingSlot.IsEmpty()) return;

        ItemData data = draggingSlot.Item.Data;
        List<ItemSlot<Item>> sameItemSlots = new List<ItemSlot<Item>>();

        foreach (ItemCollection<Item> collection in new[] { Content, ToolBar, storage })
        {
            if (collection == null) continue;

            foreach (ItemSlot<Item> slot in collection.Slots)
            {
                if (slot != null && !slot.IsEmpty() && slot.Item.Data.Id == data.Id)
                {
                    sameItemSlots.Add(slot);
                }
            }
        }

        // Smallest stacks first (stable sort: inventory, tool bar, then storage for the same size)
        List<ItemSlot<Item>> ordered = new List<ItemSlot<Item>>(sameItemSlots);
        ordered.Sort((a, b) => a.Quantity != b.Quantity ? a.Quantity.CompareTo(b.Quantity) : sameItemSlots.IndexOf(a).CompareTo(sameItemSlots.IndexOf(b)));

        foreach (ItemSlot<Item> slot in ordered)
        {
            if (draggingSlot.IsFull()) return;

            int taken = Mathf.Min(data.StackMax - draggingSlot.Quantity, slot.Quantity);

            slot.Remove(taken);
            draggingSlot.Add(draggingSlot.Item, taken);
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

        // From the storage: same order as Add (stacks of the item first, then tool bar, then inventory)
        bool fromStorage = storage != null && storage.Slots.Contains(slot);
        ItemCollection<Item> target;

        if (fromStorage)
        {
            target = null;
        }
        else if (ToolBar.Slots.Contains(slot))
        {
            target = storage ?? Content;
        }
        else if (Content.Slots.Contains(slot))
        {
            target = storage ?? ToolBar;
        }
        else
        {
            return;
        }

        Item item = slot.Item;
        int count = slot.Quantity;

        slot.Clear();

        count = fromStorage ? Add(item, count) : target.Add(item, count);

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