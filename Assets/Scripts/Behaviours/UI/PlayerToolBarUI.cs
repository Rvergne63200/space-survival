using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PlayerToolBarUI : ItemCollectionUI<Item>
{
    public override void OnUpdateManager(UIManager manager)
    {
        if (manager == null)
        {
            return;
        }

        this.Inventory = manager?.PlayerInventory?.ToolBar;
    }
}
