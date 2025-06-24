using System;
using UnityEngine;

[Serializable]
public class Item
{
    [SerializeField]
    private ItemData _data;
    public ItemData Data { get => _data; private set => _data = value; }
}
