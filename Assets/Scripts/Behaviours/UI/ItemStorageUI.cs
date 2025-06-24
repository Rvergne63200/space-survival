using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemStorageUI : ItemCollectionUI<Item>
{
    public void SetItemStorage(ItemStorage inventory, Action clearInterfaceAction)
    {
        this.Inventory = inventory?.Content;
        DisplayInventory(inventory?.Content != null);
        ac_OnClearInterface = clearInterfaceAction;
        UpdateUI();
    }
}
