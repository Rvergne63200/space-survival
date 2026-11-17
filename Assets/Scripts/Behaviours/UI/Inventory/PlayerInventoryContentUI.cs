using System;
using UnityEngine.Events;

// Inventory of the player (Inventory.uxml), opened and closed with the Inventory key (PlayerInventory.IsOpen)
public class PlayerInventoryContentUI : ItemCollectionUI
{
    private UnityEvent<bool, Action> ev_UpdateIsOpen;
    private PlayerInventory inventory;

    public override void OnUpdateManager(UIManager manager)
    {
        if (manager == null)
        {
            return;
        }

        ev_UpdateIsOpen?.RemoveListener(SetVisible);

        inventory = manager.PlayerInventory;

        ev_UpdateIsOpen = inventory.ev_UpdateIsOpen;
        ev_UpdateIsOpen.AddListener(SetVisible);
    }

    protected override void OnDestroy()
    {
        ev_UpdateIsOpen?.RemoveListener(SetVisible);
        base.OnDestroy();
    }

    private void SetVisible(bool visible, Action hideInterfaceAction)
    {
        ac_OnClearInterface = hideInterfaceAction;

        if (visible)
        {
            OpenPanel(inventory.Content);
        }
        else
        {
            ClosePanel();
        }
    }
}
