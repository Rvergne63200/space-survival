using System;
using UnityEngine;

public enum ItemGaugeType
{
    // No bar
    None,
    // Wear (pickaxe...): starts full, the item is destroyed at 0. Units
    Wear,
    // Charge (flashlight, jetpack...): starts empty, recharged at a charging station, unusable at 0. Wh
    Energy
}

// Bar of a tool or an equipment (ItemData), its value is kept by each item (Item.Gauge). Such items do not stack.
// It goes down while the item is active (toggle on, hold: consumptionPerSecond, in W for a charge, units/s for a wear)
// and on each use (click: consumptionPerUse, in Wh or units)
[Serializable]
public class ItemGauge
{
    public ItemGaugeType type = ItemGaugeType.None;

    // Wh for a charge, units for a wear
    public float capacity = 100f;

    public float consumptionPerSecond;
    public float consumptionPerUse;

    public bool Enabled => type != ItemGaugeType.None && capacity > 0f;
    public bool Rechargeable => type == ItemGaugeType.Energy;

    // Gauge lost per second of activity: a charge is in Wh, its consumption in W
    public float PerSecond => type == ItemGaugeType.Energy ? consumptionPerSecond / 3600f : consumptionPerSecond;

    public string Format(float value)
    {
        return type == ItemGaugeType.Energy
            ? EnergyValue.Format(value, "").Trim() + " / " + EnergyValue.Format(capacity, "Wh")
            : Mathf.CeilToInt(value) + " / " + Mathf.CeilToInt(capacity);
    }
}
