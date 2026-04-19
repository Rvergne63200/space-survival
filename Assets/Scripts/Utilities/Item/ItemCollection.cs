using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ItemCollection<T> where T : Item
{
    [SerializeField]
    public UnityEvent ev_Update;

    [SerializeField]
    private List<ItemSlot<T>> _slots;
    public List<ItemSlot<T>> Slots
    {
        get => _slots;
        private set
        {
            List<ItemSlot<T>> slots = new List<ItemSlot<T>>();

            for (int i = 0; i < MaxSize; i++)
            {
                ItemSlot<T> slot;

                if (value != null && i < value.Count && value[i] != null)
                {
                    slot = value[i];
                }
                else
                {
                    slot = new ItemSlot<T>();
                }

                slots.Insert(i, slot);
            }

            _slots = slots;
            ev_Update.Invoke();
        }
    }

    [SerializeField]
    private int _maxSize;
    public int MaxSize
    {
        get => _maxSize;
        private set
        {
            _maxSize = value;
            Slots = Slots;
        }
    }

    public ItemCollection(int maxSize = 0, List<ItemSlot<T>> slots = null, UnityEvent ev = null)
    {
        ev_Update = (ev == null) ? new UnityEvent() : ev;

        MaxSize = maxSize;
        Slots = slots;
    }

    public int Add(T item, int count)
    {       
        if (item == null)
        {
            Debug.Log("Inutile d'ajouter un élément vide à la liste");
            return count;
        }

        ItemSlot<T> slot = GetFirstSlotNotFull(item);

        if(slot == null)
        {
            int index = GetFirstEmptySlot();

            if(index < 0 || index >= MaxSize)
            {
                ev_Update.Invoke();
                return count;
            }

            if (index >= Slots.Count || Slots[index] == null)
            {
                slot = new ItemSlot<T>();
                SetSlot(slot, index);
            }
            else
            {
                slot = Slots[index];
            }
        }

        int rest = slot.Add(item, count);

        if (rest > 0)
        {
            rest = Add(item, rest);
        }
        else
        {
            ev_Update.Invoke();
        }

        return rest;
    }

    public int RemoveItem(T item, int count) => RemoveItem(item?.Data, count);

    public int RemoveItem(ItemData item, int count)
    {
        if (item == null)
        {
            Debug.Log("On ne peut pas supprimer un élément inéxistant");
            return count;
        }

        int index = GetLastIndex(item);

        if (index < 0)
        {
            Debug.Log("Objet introuvable");
            return count;
        }

        count = RemoveFromSlot(index, count);

        if(count > 0)
        {
            count = RemoveItem(item, count);
        }

        return count;
    }


    public int Count(T item) => Count(item?.Data);
    public int Count(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("Invalid item data");
            return -1;
        }

        int count = 0;

        foreach (var slot in Slots)
        {
            if(slot.Item?.Data?.Id == item.Id)
            {
                count += slot.Quantity;
            }
        }

        return count;
    }


    private int GetLastIndex(ItemData item)
    {
        int index = -1;
        int i = 0;

        if(item == null)
        {
            return -2;
        }

        foreach(ItemSlot<T> slot in Slots)
        {
            if(slot?.Item?.Data?.Id == item.Id)
            {
                index = i;
            }

            i++;
        }

        return index;
    }



    private ItemSlot<T> GetFirstSlotNotFull(T item) => GetFirstSlotNotFull(item?.Data);

    private ItemSlot<T> GetFirstSlotNotFull(ItemData item)
    {
        if (item == null)
        {
            return null;
        }

        foreach (ItemSlot<T> slot in Slots)
        {
            if (slot?.Item?.Data?.Id == item.Id && !slot.IsFull())
            {
                return slot;
            }
        }

        return null;
    }


    private int GetFirstEmptySlot()
    {
        for(int i = 0; i < MaxSize; i++)
        {
            if(i >= Slots.Count)
            {
                return i;
            }

            if (Slots[i] == null)
            {
                return i;
            }

            if (Slots[i].Quantity <= 0 || Slots[i].Item == null)
            {
                Slots[i].Clear();
                return i;
            }
        }

        return -1;
    }

    private int RemoveFromSlot(int index, int count)
    {
        if (index < 0 || index > MaxSize)
        {
            Debug.Log("Slot invalide");
            return count;
        }

        if (Slots[index] == null)
        {
            Debug.Log("Slot vide");
            return count;
        }

        return Slots[index].Remove(count);
    }

    private void SetSlot(ItemSlot<T> slot, int pos)
    {
        if (pos >= MaxSize)
        {
            Debug.Log("Cet emplacement n'existe pas");
            return;
        }

        if (pos >= Slots.Count)
        {
            Slots.Insert(pos, slot);
        }
        else
        {
            Slots[pos] = slot;
        }

        ev_Update.Invoke();
    }
}
