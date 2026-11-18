using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ItemSlot<T> where T : Item
{
    [SerializeField]
    private UnityEvent<ItemSlot<T>> _ev_update;
    public UnityEvent<ItemSlot<T>> ev_update { get => _ev_update; set => _ev_update = value; }


    [SerializeField]
    private T _item;
    public T Item { get => _item; private set => _item = value; }

    [SerializeField]
    private int _quantity;
    public int Quantity { get => _quantity; private set => _quantity = value; }

    public ItemSlot()
    {
        ev_update = new UnityEvent<ItemSlot<T>>();
        Clear();
    }

    public void Clear()
    {
        Quantity = 0;
        Item = null;

        ev_update.Invoke(this);
    }

    public int Remove(int count)
    {
        Quantity -= count;

        if(Quantity <= 0)
        {
            Clear();
            return Mathf.Abs(Quantity);
        }
        else
        {
            ev_update.Invoke(this);
        }

        return 0;
    }

    public int Add(T item, int count)
    {
        if(item == null || count <= 0)
        {
            return 0;
        }

        // Not only "Item == null": Unity serialization creates an Item with null Data in empty slots
        if(IsEmpty())
        {
            // The bar of an item is its own: a copy per slot, the value moves with it (switch, drag)
            Item = item.HasGauge ? (T)item.Copy() : item;
            Quantity = 0;
        }

        if(Item?.Data?.Id != item?.Data?.Id)
        {
            return count;
        }

        if(Quantity + count > Item.Data.StackMax)
        {
            int rest = Quantity + count - Item.Data.StackMax;
            Quantity = Item.Data.StackMax;

            ev_update.Invoke(this);
            return rest;
        }

        Quantity += count;

        ev_update.Invoke(this);
        return 0;
    }

    // The item changed without being added or removed (value of its bar)
    public void NotifyChanged()
    {
        ev_update.Invoke(this);
    }

    public bool IsFull()
    {
        if(IsEmpty())
        {
            return false;
        }

        return Quantity >= Item.Data.StackMax;
    }

    public bool IsEmpty()
    {
        return Item == null || Item.Data == null || Quantity <= 0;
    }

    public void Switch(ItemSlot<T> slot)
    {
        T item = slot?.Item;
        int count = slot?.Quantity ?? 0;
        slot.Clear();

        T currentItem = this.Item;
        int currentCount = this.Quantity;
        this.Clear();

        if (currentItem != null && currentCount > 0) slot.Add(currentItem, currentCount);

        if (item != null && count > 0) this.Add(item, count);
    }

    public void Drop(int count, GameObject source)
    {
        if(Item == null || Quantity <= 0)
        {
            return;
        }

        int quantityToDrop = Mathf.Clamp(count, 0, Quantity);

        // Only remove the items once they actually exist in the world
        if (Pickupable.Spawn(Item, quantityToDrop, source.transform) != null)
        {
            Remove(quantityToDrop);
        }
    }
}
