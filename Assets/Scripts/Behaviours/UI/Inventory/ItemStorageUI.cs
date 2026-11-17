using System;

// Storage searched by the player (Storage.uxml): chest, death bag... (Searcher.ev_UpdateSearchStorage, see UIManager).
// Opens the player inventory with it
public class ItemStorageUI : ItemCollectionUI
{
    public void SetItemStorage(ItemStorage storage)
    {
        PlayerInventory inventory = parentUI != null && parentUI.UIManager != null ? parentUI.UIManager.PlayerInventory : null;

        if (storage != null && storage.Content != null)
        {
            OpenPanel(storage.Content);
            if (inventory != null) inventory.OpenWith(this);
        }
        else
        {
            ClosePanel();
            if (inventory != null) inventory.CloseWith(this);
        }
    }

    public void SetInterfaceClearAction(Action clearInterfaceAction)
    {
        ac_OnClearInterface = clearInterfaceAction;
    }
}
