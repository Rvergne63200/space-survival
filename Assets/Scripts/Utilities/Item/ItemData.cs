using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Item/Item")]
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

    [SerializeField]
    private GameObject _model;
    public GameObject Model { get => _model; protected set => _model = value; }

    // Done when the item is in hand: primary use (left click) and secondary use (right click)
    [SerializeReference, SubclassPicker]
    private List<ItemAction> _primaryActions = new List<ItemAction>();
    public List<ItemAction> PrimaryActions => _primaryActions;

    [SerializeReference, SubclassPicker]
    private List<ItemAction> _secondaryActions = new List<ItemAction>();
    public List<ItemAction> SecondaryActions => _secondaryActions;

    // Returns true when at least one action was done. The hold actions that started are added to startedHolds
    public bool Use(bool primary, ItemUseContext context, List<HoldItemAction> startedHolds = null)
    {
        bool done = false;

        foreach (ItemAction action in primary ? _primaryActions : _secondaryActions)
        {
            if (action != null && action.Execute(context))
            {
                done = true;

                if (action is HoldItemAction hold)
                {
                    startedHolds?.Add(hold);
                }
            }
        }

        return done;
    }

    // The item leaves the hand: every action stops what it does
    public void Unequip(ItemUseContext context)
    {
        foreach (ItemAction action in _primaryActions)
        {
            action?.OnUnequip(context);
        }

        foreach (ItemAction action in _secondaryActions)
        {
            action?.OnUnequip(context);
        }
    }
}