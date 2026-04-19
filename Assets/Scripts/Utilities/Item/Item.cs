using System;
using UnityEngine;

[Serializable]
public class Item
{
    [SerializeField]
    private ItemData _data;
    public ItemData Data { get => _data; private set => _data = value; }


    private Item(ItemData data)
    {
        _data = data;
    }

    public static Item CreateFromData(ItemData data)
    {
        Item item = new Item(data);
        return item;
    }
}
