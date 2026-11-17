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

    // Camera of the player: where they look from
    public Transform View;
}

// How the click triggers an action
public enum ItemActionMode
{
    // Once per click (weapon that shoots, consumable)
    Press,
    // On / off on each click, off when the item leaves the hand (flashlight)
    Toggle,
    // As long as the button is held (pickaxe)
    Hold
}

// An action of an item, done with the primary use (left click) or the secondary use (right click) when it is in hand
// (ItemData primary and secondary actions). New tools and weapons add their own subclasses, from ItemAction (Press),
// ToggleItemAction or HoldItemAction.
// The actions belong to the ItemData asset: their runtime state is shared by all the items of this kind
[Serializable]
public abstract class ItemAction
{
    public virtual ItemActionMode Mode => ItemActionMode.Press;

    // Called on the click. Returns true when the action was done
    public abstract bool Execute(ItemUseContext context);

    // The item leaves the hand (other slot selected, slot emptied, player gone): stop what the action does
    public virtual void OnUnequip(ItemUseContext context) { }
}

// Switched on and off by the click (PlayerInventory), switched off when the item leaves the hand
[Serializable]
public abstract class ToggleItemAction : ItemAction
{
    public override ItemActionMode Mode => ItemActionMode.Toggle;

    [NonSerialized] private bool isOn;
    public bool IsOn => isOn;

    public override bool Execute(ItemUseContext context)
    {
        isOn = !isOn;
        SetOn(context, isOn);

        return true;
    }

    public override void OnUnequip(ItemUseContext context)
    {
        if (!isOn) return;

        isOn = false;
        SetOn(context, false);
    }

    protected abstract void SetOn(ItemUseContext context, bool on);
}

// Done while the button is held (PlayerInventory): Execute starts it, Hold is called each frame until it returns false,
// the button is released or the item leaves the hand, then End
[Serializable]
public abstract class HoldItemAction : ItemAction
{
    public override ItemActionMode Mode => ItemActionMode.Hold;

    // Returns true when the hold starts
    public override bool Execute(ItemUseContext context)
    {
        return true;
    }

    // heldTime: seconds since the start. Returns false to stop
    public abstract bool Hold(ItemUseContext context, float heldTime);

    // completed: stopped by Hold (true) or by the release / the item leaving the hand (false)
    public virtual void End(ItemUseContext context, bool completed) { }
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

// Flashlight: a spot light in front of the camera, switched on and off by the click
[Serializable]
public class FlashlightAction : ToggleItemAction
{
    public Color color = new Color(1f, 0.95f, 0.85f);
    public float intensity = 1.5f;
    public float range = 30f;
    [Range(1f, 179f)] public float spotAngle = 60f;

    [NonSerialized] private Light light;

    protected override void SetOn(ItemUseContext context, bool on)
    {
        // Destroyed with the camera (respawned): created again
        if (light == null)
        {
            if (!on || context.View == null) return;

            GameObject lightObject = new GameObject("Flashlight");
            lightObject.transform.SetParent(context.View, false);

            light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.shadows = LightShadows.Soft;
        }

        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = spotAngle;
        light.enabled = on;
    }
}
