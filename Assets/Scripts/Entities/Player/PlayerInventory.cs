using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : ItemStorage
{
    private PlayerInputActions inputActions;
    public UnityEvent<bool, Action> ev_UpdateIsOpen;
    public ItemSlot<Item> draggingSlot;

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
    }

    private void OnEnable()
    {
        inputActions.Actions.Enable();
        inputActions.Actions.Inventory.performed += ctx => {
            IsOpen = !IsOpen;
        };
    }

    private void OnDisable()
    {
        inputActions.Actions.Disable();
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