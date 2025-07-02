using UnityEngine;

public class PlayerToolBarUI : ItemCollectionUI<Item>
{
    public override void AfterUpdatePlayer()
    {
        base.AfterUpdatePlayer();

        PlayerInventory inventory = parentUI.PlayerInventory;

        this.Inventory = inventory?.ToolBar;
    }
}
