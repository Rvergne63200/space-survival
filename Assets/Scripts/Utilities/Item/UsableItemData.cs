using System;
using UnityEngine;


// An item that can have a bar (ItemGauge: wear, charge, oxygen), kept by each item. Base of ToolData and EquipmentData
[Serializable]
public abstract class UsableItemData : ItemData
{
    [SerializeField]
    private ItemGauge _gauge = new ItemGauge();
    public ItemGauge Gauge => _gauge;

    // The bar is enabled (ItemGauge.enabled) and has a capacity
    public bool HasGauge => _gauge != null && _gauge.enabled && _gauge.capacity > 0f;

    // An item with a bar does not stack: each one keeps its own value
    public override int StackMax => HasGauge ? 1 : base.StackMax;
}
