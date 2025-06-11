using UnityEngine;

public class PlayerInventoryContentUI : InventoryContentUI
{
    void Awake()
    {
        gameObject.SetActive(false);

        PlayerInventory inventory = parentUI.playerInventory;
        inventory.ev_UpdateIsOpen.AddListener(SetVisible);

        this.inventory = inventory;
    }
}
