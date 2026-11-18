using System;
using UnityEngine;

[Serializable]
public class Item
{
    [SerializeField]
    private ItemData _data;
    public ItemData Data { get => _data; private set => _data = value; }

    // Value of the bar of the item (ItemData.Gauge): charge in Wh or wear in units. Set on first read when the item is
    // new: empty for a rechargeable item, full for one that wears out
    [SerializeField]
    private bool _gaugeSet;
    [SerializeField]
    private float _gauge;

    private Item(ItemData data)
    {
        _data = data;
    }

    public static Item CreateFromData(ItemData data)
    {
        Item item = new Item(data);
        return item;
    }

    // An item with a bar is its own object in each slot (its value must not be shared)
    public Item Copy()
    {
        return (Item)MemberwiseClone();
    }

    public bool HasGauge => Data != null && Data.HasGauge;

    public float Gauge
    {
        get
        {
            if (!HasGauge) return 0f;

            if (!_gaugeSet)
            {
                _gauge = Data.Gauge.Rechargeable ? 0f : Data.Gauge.capacity;
                _gaugeSet = true;
            }

            return Mathf.Min(_gauge, Data.Gauge.capacity);
        }
    }

    // 0 to 1
    public float GaugeRatio => HasGauge ? Gauge / Data.Gauge.capacity : 0f;

    public bool IsGaugeEmpty => HasGauge && Gauge <= 0.0001f;
    public bool IsGaugeFull => HasGauge && Gauge >= Data.Gauge.capacity - 0.0001f;

    public void SetGauge(float value)
    {
        if (!HasGauge) return;

        _gauge = Mathf.Clamp(value, 0f, Data.Gauge.capacity);
        _gaugeSet = true;
    }

    // Returns what was actually taken
    public float ConsumeGauge(float amount)
    {
        float before = Gauge;
        SetGauge(before - amount);
        return before - Gauge;
    }

    // Returns what was actually added
    public float ChargeGauge(float amount)
    {
        float before = Gauge;
        SetGauge(before + amount);
        return Gauge - before;
    }
}
