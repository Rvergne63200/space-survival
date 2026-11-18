using System;
using System.Collections.Generic;
using UnityEngine;


// How the use of a tool empties its bar, whatever the unit of the bar
public enum ToolConsumptionMode
{
    // Per second while the tool is active (toggle action on, hold action running)
    Continuous,
    // A fixed amount on each use (click)
    PerUse
}

// A usable item used from the hand: primary actions (left click) and secondary actions (right click)
[CreateAssetMenu(menuName = "Item/Tool")]
[Serializable]
public class ToolData : UsableItemData
{
    [SerializeField]
    private ToolConsumptionMode _consumptionMode = ToolConsumptionMode.PerUse;
    public ToolConsumptionMode ConsumptionMode => _consumptionMode;

    // Continuous: per second (W for a charge, L/s or units/s otherwise). Per use: Wh, L or units
    [SerializeField]
    private float _consumption;
    public float Consumption => _consumption;

    [SerializeReference, SubclassPicker]
    private List<ItemAction> _primaryActions = new List<ItemAction>();
    public List<ItemAction> PrimaryActions => _primaryActions;

    [SerializeReference, SubclassPicker]
    private List<ItemAction> _secondaryActions = new List<ItemAction>();
    public List<ItemAction> SecondaryActions => _secondaryActions;

    public List<ItemAction> Actions(bool primary) => primary ? _primaryActions : _secondaryActions;

    // Returns true when at least one action was done. The hold actions that started are added to startedHolds
    public bool Use(bool primary, ItemUseContext context, List<HoldItemAction> startedHolds = null)
    {
        bool done = false;

        foreach (ItemAction action in Actions(primary))
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

    // At least one toggle action is on
    public bool IsToggledOn()
    {
        foreach (List<ItemAction> actions in new[] { _primaryActions, _secondaryActions })
        {
            foreach (ItemAction action in actions)
            {
                if (action is ToggleItemAction toggle && toggle.IsOn) return true;
            }
        }

        return false;
    }

    // The tool leaves the hand: every action stops what it does
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
