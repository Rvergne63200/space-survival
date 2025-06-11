using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ItemCollection<T> where T : Item
{
    [SerializeField]
    UnityEvent ev_Update;

    [SerializeField]
    private List<ItemSlot<T>> _slots;
    public List<ItemSlot<T>> Slots
    {
        get => _slots;
        private set
        {
            if(value.Count > Size)
            {
                Debug.Log("Le tableau original est plus grand que la collection");
                return;
            }

            _slots = value;
            ev_Update.Invoke();
        }
    }

    [SerializeField]
    private int _size;
    public int Size
    {
        get => _size;
        private set
        {
            _size = value;
            Slots = Slots;
        }
    }

    public ItemCollection(int size = 0, List<ItemSlot<T>> slots = null, UnityEvent ev = null)
    {
        ev_Update = (ev == null) ? new UnityEvent() : ev;

        Size = size;
        Slots = slots;
    }

    public void Set(ItemSlot<T> slot, int pos)
    {
        if (pos >= Size)
        {
            Debug.Log("Cet emplacement n'existe pas");
            return;
        }

        if(pos >= Slots.Count)
        {
            Slots.Insert(pos, slot);
        }
        else
        {
            Slots[pos] = slot;
        }

        ev_Update.Invoke();
    }

    public int Add(T item, int count)
    {
        if (item == null)
        {
            Debug.Log("Inutile d'ajouter un élément vide à la liste");
            return count;
        }

        int index = GetLastIndex(item);

        if (index < 0)
        {
            index = GetFirstEmptySlot();

            if(index < 0)
            {
                Debug.Log("Aucun emplacement libre na été trouvé");
                return count;
            }
        }

        if (index >= Slots.Count || Slots[index] == null)
        {
            Set(new ItemSlot<T>(), index);
        }

        int rest = Slots[index].Add(item, count);

        ev_Update.Invoke();

        return rest;
    }

    public void RemoveFromSlot(int index, int count)
    {
        if (index < 0 || index > Size)
        {
            Debug.Log("Slot invalide");
            return;
        }

        if (Slots[index] == null)
        {
            Debug.Log("Slot vide");
            return;
        }

        Slots[index].Remove(count);
    }

    public void RemoveItem(T item, int count)
    {
        if (item == null)
        {
            Debug.Log("On ne peut pas supprimer un élément inéxistant");
            return;
        }

        int index = GetLastIndex(item);

        if (index < 0)
        {
            Debug.Log("Objet introuvable");
            return;
        }

        RemoveFromSlot(index, count);
    }

    public int GetLastIndex(T item)
    {
        int index = -1;
        int i = 0;

        if (item == null)
        {
            return -2;
        }

        foreach (ItemSlot<T> slot in Slots)
        {
            if (slot?.Item?.Data?.Id == item?.Data.Id)
            {
                index = i;
            }

            i++;
        }

        return index;
    }

    public int GetFirstEmptySlot()
    {
        for(int i = 0; i < Size; i++)
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
}
