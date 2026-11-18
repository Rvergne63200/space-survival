using System;
using UnityEngine;


// Base of every item. Subclasses: ConsummableData (right click), PlaceableItemData, UsableItemData (bar: ToolData,
// EquipmentData)
[CreateAssetMenu(menuName = "Item/Item")]
[Serializable]
public class ItemData : ScriptableObject
{
    [SerializeField]
    private string _id;
    public string Id { get => _id; protected set => _id = value; }

    [SerializeField]
    private int _stackMax = 99;
    public virtual int StackMax => _stackMax;

    [SerializeField]
    private string _name;
    public string Name { get => _name; protected set => _name = value; }

    [SerializeField]
    private string _description;
    public string Description { get => _description; protected set => _description = value; }

    [SerializeField]
    private Sprite _sprite;
    public Sprite Sprite { get => _sprite; protected set => _sprite = value; }

    [SerializeField]
    private GameObject _model;
    public GameObject Model { get => _model; protected set => _model = value; }
}
