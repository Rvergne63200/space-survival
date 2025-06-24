using UnityEngine;

public class PlayerInventoryContentUI : ItemCollectionUI<Item>
{
    protected override void Awake()
    {
        base.Awake();

        gameObject.SetActive(false);

        PlayerInventory inventory = parentUI.playerInventory;
        inventory.ev_UpdateIsOpen.AddListener(SetVisible);

        this.Inventory = inventory.Content;
    }
}
