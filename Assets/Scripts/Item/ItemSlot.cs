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

        if(Item == null)
        {
            Item = item;
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

    public bool IsFull()
    {
        if(Item == null)
        {
            return false;
        }

        return Quantity >= Item.Data.StackMax;
    }

    public bool IsEmpty()
    {
        return Item == null || Quantity <= 0;
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
        GameObject objectToInstantiate = Item.Data.Model;
        Item itemToDrop = Item;

        Remove(quantityToDrop);
        GameObject instance = GameObject.Instantiate(objectToInstantiate, source.transform.position + source.transform.forward * 1f, source.transform.rotation, null);
        Pickupable pickupable = instance.GetComponent<Pickupable>();

        pickupable.item = itemToDrop;
        pickupable.count = quantityToDrop;
        pickupable.identified = true;

        Rigidbody rb = instance.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.AddForce(source.transform.forward * 50f);
    }
}
