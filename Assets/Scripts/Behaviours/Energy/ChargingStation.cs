using UnityEngine;

// Charging station: a consumer of the electrical network with a storage (ItemStorage, one slot) opened by interacting
// with it. While powered, it charges the rechargeable item put inside (ItemGauge of type Energy) with chargePower, and
// asks that power to its network only while such an item is not full
[RequireComponent(typeof(ItemStorage))]
public class ChargingStation : EnergyConsumer, IInterractable
{
    [SerializeField] private float chargePower = 100f;

    private ItemStorage storage;

    private ItemStorage Storage => storage != null ? storage : (storage = GetComponent<ItemStorage>());

    public override float Consumption => ChargingSlot() != null ? chargePower : 0f;

    // The first slot holding a rechargeable item that is not full
    private ItemSlot<Item> ChargingSlot()
    {
        if (Storage == null || Storage.Content == null) return null;

        foreach (ItemSlot<Item> slot in Storage.Content.Slots)
        {
            if (slot != null && !slot.IsEmpty() && slot.Item.HasGauge && slot.Item.Data.Gauge.Rechargeable && !slot.Item.IsGaugeFull)
            {
                return slot;
            }
        }

        return null;
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
            return text + "\nVide : déposez un objet rechargeable";
        }

        if (!slot.Item.HasGauge || !slot.Item.Data.Gauge.Rechargeable)
        {
            return text + "\n" + slot.Item.Data.Name + " : pas rechargeable";
        }

        return text + "\n" + slot.Item.Data.Name + " : " + slot.Item.Data.Gauge.Format(slot.Item.Gauge) + (slot.Item.IsGaugeFull ? " (chargé)" : "");
    }
}
