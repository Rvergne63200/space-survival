using System;
using System.Collections.Generic;
using UnityEngine;

// What an item use needs to know (PlayerInventory.Use1 / Use2)
public class ItemUseContext
{
    // Holds the inventory and the stats
    public PlayerManager Manager;

    // The body of the player in the world (position, view...)
    public GameObject Player;

    // Slot of the used item (the item in hand)
    public ItemSlot<Item> Slot;
}

// An action of an item, done with the primary use (left click) or the secondary use (right click) when it is in hand
// (ItemData primary and secondary actions). New tools and weapons add their own subclasses
[Serializable]
public abstract class ItemAction
{
    // Returns true when the action was done
    public abstract bool Execute(ItemUseContext context);
}

// Applies modifiers (stats...) to the player, then consumes one item
[Serializable]
public class ApplyModifiersAction : ItemAction
{
    [SerializeReference, SubclassPicker]
    public List<AbstractModifier> modifiers = new List<AbstractModifier>();

    public bool consume = true;

    public override bool Execute(ItemUseContext context)
    {
        if (context.Manager == null) return false;

        // The stats are on the PlayerManager
        foreach (AbstractModifier modifier in modifiers)
        {
            modifier?.Modify(context.Manager.gameObject);
        }

        if (consume)
        {
            context.Slot?.Remove(1);
        }

        return true;
    }
}
