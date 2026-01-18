using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventoryContentUI : ItemCollectionUI<Item>
{
    private UnityEvent<bool, Action> ev_UpdateIsOpen;

    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
    }

    public override void OnUpdateManager(UIManager manager)
    {   
        if(manager == null)
        {
            return;
        }

        ev_UpdateIsOpen?.RemoveListener(SetVisible);

        PlayerInventory inventory = manager.PlayerInventory;

        ev_UpdateIsOpen = inventory.ev_UpdateIsOpen;
        ev_UpdateIsOpen.AddListener(SetVisible);

        DisplayInventory(inventory.IsOpen);

        this.Inventory = inventory.Content;
    }
}
