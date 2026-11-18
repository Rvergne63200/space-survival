using System;
using UnityEngine;

// What the bar of an item measures. It only changes what the player sees and how the item is refilled
public enum ItemGaugeUnit
{
    // Wear units (pickaxe...): starts full, cannot be refilled, the item is destroyed at 0
    Wear,
    // Wh (flashlight, jetpack...): starts empty, recharged at a charging station, unusable at 0
    Energy,
    // Litres of oxygen (oxygen tank...): starts empty, refilled by an oxygen refill device (to come), unusable at 0
    Oxygen
}

// Bar of a usable item (UsableItemData), its value is kept by each item (Item.Gauge). Such items do not stack.
// How it goes down depends on the item (ToolData: continuous or per use)
[Serializable]
public class ItemGauge
{
    // Unchecked: the item has no bar (no unit, no wear), whatever its consumption
    public bool enabled;

    public ItemGaugeUnit unit = ItemGaugeUnit.Wear;

    // In the unit of the bar (Wh, L or wear units)
    public float capacity = 100f;

    public bool Rechargeable => unit != ItemGaugeUnit.Wear;

    // Shown before the value (tooltip)
    public string Label => unit == ItemGaugeUnit.Energy ? "Charge" : unit == ItemGaugeUnit.Oxygen ? "Oxygène" : "Usure";

    // Shown after the name of the item when the bar is at 0 (a worn item is destroyed instead)
    public string EmptyText => unit == ItemGaugeUnit.Oxygen ? "est vide" : "est déchargé";

    // Gauge lost per second for a continuous consumption: W for a charge (the bar is in Wh), L/s or units/s otherwise
    public float PerSecond(float consumption)
    {
        return unit == ItemGaugeUnit.Energy ? consumption / 3600f : consumption;
    }

    public string Format(float value)
    {
        switch (unit)
        {
            case ItemGaugeUnit.Energy:
                return EnergyValue.Format(value, "").Trim() + " / " + EnergyValue.Format(capacity, "Wh");
            case ItemGaugeUnit.Oxygen:
                return value.ToString("0.#") + " / " + capacity.ToString("0.#") + " L";
            default:
                return Mathf.CeilToInt(value) + " / " + Mathf.CeilToInt(capacity);
        }
    }
}
