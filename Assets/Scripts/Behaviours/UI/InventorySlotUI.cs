using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public abstract class InventorySlotUI<T> : ParentedUI where T : Item
{
    protected ItemSlot<T> slot;

    public Image sprite;
    public TextMeshProUGUI count;

    public Sprite transparent;

    protected UnityAction<ItemSlot<T>> ac_update;

    private void Awake()
    {
        ac_update = new UnityAction<ItemSlot<T>>(UpdateSlot);
    }

    public void setSlot(ItemSlot<T> slot)
    {
        if (this.slot != null && this.slot != slot)
        {
            this.slot.ev_update.RemoveListener(new UnityAction<ItemSlot<T>>(UpdateSlot));
        }

        this.slot = slot;

        if (slot != null)
        {
            slot.ev_update.AddListener(new UnityAction<ItemSlot<T>>(UpdateSlot));
        }

        UpdateSlot(slot);
    }

    public void UpdateSlot(ItemSlot<T> slot)
    {     
        if(slot == null || slot.Item == null || slot.Quantity <= 0)
        {
            sprite.sprite = transparent;
            count.gameObject.SetActive(false);
            return;
        }

        sprite.sprite = slot.Item.Data.Sprite;

        count.text = slot.Quantity.ToString();
        count.gameObject.SetActive(slot.Item.Data.StackMax > 1);
    }
}
