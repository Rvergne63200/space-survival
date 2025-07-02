using UnityEngine;

public class PlayerInventoryContentUI : ItemCollectionUI<Item>
{
    protected override void Start()
    {
        base.Start();
        gameObject.SetActive(false);
    }


    public override void BeforeUpdatePlayer()
    {
        base.BeforeUpdatePlayer();

        PlayerInventory inventory = parentUI.PlayerInventory;

        if (inventory != null)
        {
            inventory.ev_UpdateIsOpen.RemoveListener(SetVisible);
        }
    }

    public override void AfterUpdatePlayer()
    {
        base.AfterUpdatePlayer();

        PlayerInventory inventory = parentUI.PlayerInventory;

        if (inventory != null)
        {
            inventory.ev_UpdateIsOpen.AddListener(SetVisible);
        }

        this.Inventory = inventory?.Content;
    }
}
