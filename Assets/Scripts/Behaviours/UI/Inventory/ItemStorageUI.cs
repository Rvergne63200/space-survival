using System;

public class ItemStorageUI : ItemCollectionUI<Item>
{
    public void SetItemStorage(ItemStorage inventory)
    {
        this.Inventory = inventory?.Content;
        DisplayInventory(inventory?.Content != null);
        UpdateUI();
    }

    public void SetInterfaceClearAction(Action clearInterfaceAction)
    {
        ac_OnClearInterface = clearInterfaceAction;
    }
}
