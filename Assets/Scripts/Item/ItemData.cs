using System;
using UnityEngine;


[CreateAssetMenu()]
[Serializable]
public class ItemData : ScriptableObject
{
    [SerializeField]
    private int _id;
    public int Id { get => _id; protected set => _id = value; }

    [SerializeField]
    private int _stackMax = 99;
    public int StackMax { get => _stackMax; protected set => _stackMax = value; }

    [SerializeField]
    private string _name;
    public string Name { get => _name; protected set => _name = value; }

    [SerializeField]
    private string _description;
    public string Description { get => _description; protected set => _description = value; }

    [SerializeField]
    private Sprite _sprite;
    public Sprite Sprite { get => _sprite; protected set => _sprite = value; }
}