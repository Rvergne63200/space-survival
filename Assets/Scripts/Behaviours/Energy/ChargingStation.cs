using UnityEngine;

// Charging station: a consumer of the electrical network with a storage (ItemStorage, one slot) opened by interacting
// with it. While powered, it charges the item put inside when its bar is in Wh (ItemGaugeUnit.Energy) with chargePower,
// and asks that power to its network only while such an item is not full
[RequireComponent(typeof(ItemStorage))]
public class ChargingStation : EnergyConsumer, IInterractable
{
    [SerializeField] private float chargePower = 100f;

    private ItemStorage storage;

    private ItemStorage Storage => storage != null ? storage : (storage = GetComponent<ItemStorage>());

    public override float Consumption => ChargingSlot() != null ? chargePower : 0f;

    // The first slot holding an item with a bar in Wh that is not full
    private ItemSlot<Item> ChargingSlot()
    {
        if (Storage == null || Storage.Content == null) return null;

        foreach (ItemSlot<Item> slot in Storage.Content.Slots)
        {
            if (slot != null && !slot.IsEmpty() && IsElectric(slot.Item) && !slot.Item.IsGaugeFull)
            {
                return slot;
            }
        }

        return null;
    }

    private static bool IsElectric(Item item)
    {
        return item.HasGauge && item.GaugeSettings.unit == ItemGaugeUnit.Energy;
    }

    private void Update()
    {
        if (!IsPowered) return;

        ItemSlot<Item> slot = ChargingSlot();

        if (slot == null) return;

        slot.Item.ChargeGauge(chargePower * Time.deltaTime / 3600f);
        slot.NotifyChanged();
    }

    // ---------- Interaction ----------

    public void Interract(GameObject interractor)
    {
        Searcher searcher = interractor.GetComponent<Searcher>();

        if (searcher != null)
        {
            searcher.Search(Storage);
        }
    }

    string IInterractable.GetInfo()
    {
        string text = GetInfo();
        ItemSlot<Item> slot = Storage != null && Storage.Content != null && Storage.Content.Slots.Count > 0 ? Storage.Content.Slots[0] : null;

        if (slot == null || slot.IsEmpty())
        {
            return text + "\nVide : déposez un objet électrique à recharger";
        }

        if (!IsElectric(slot.Item))
        {
            return text + "\n" + slot.Item.Data.Name + " : ne se recharge pas à l'électricité";
        }

        return text + "\n" + slot.Item.Data.Name + " : " + slot.Item.GaugeSettings.Format(slot.Item.Gauge) + (slot.Item.IsGaugeFull ? " (chargé)" : "");
    }
}
