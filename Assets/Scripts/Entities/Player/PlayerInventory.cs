using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInventory : ItemStorage
{
    private PlayerInputActions inputActions;
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
        inputActions.Actions.Enable();

        inputActions.Actions.Inventory.performed += ctx => {
            IsOpen = !IsOpen;
        };
        inputActions.Actions.Drop.performed += ctx => DropDraggingSlot();
        inputActions.Actions.ChangeHandedItem.performed += ctx => ChangeHandedItem(ctx.control);
        inputActions.Actions.Use1.performed += ctx => Use1();
        inputActions.Actions.Use2.performed += ctx => Use2();
    }

    private void OnDisable()
    {
        inputActions.Actions.Disable();
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


    public void DropDraggingSlot()
    {
        if (IsOpen && draggingSlot != null)
        {
            draggingSlot.Drop(draggingSlot.Quantity, gameObject); 
        }
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