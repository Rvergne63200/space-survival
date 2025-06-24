using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToolBarUI : ItemCollectionUI<Item>
{
    protected override void Awake()
    {
        base.Awake();

        PlayerInventory inventory = parentUI.playerInventory;
        this.Inventory = inventory.ToolBar;
    }
}
