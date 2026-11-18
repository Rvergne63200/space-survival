using UnityEngine;

public class Pickupable : MonoBehaviour, IInterractable, ISaveableComponent
{
    public Item item;
    public int count;

    public bool identified = false;

    [System.Serializable]
    private class SaveState
    {
        public string itemId;
        public int count;
        public bool identified;
        public bool hasGauge;
        public float gauge;
    }

    public string SaveKey => "pickupable";

    private void Awake()
    {
        // Every pickup is saved: picked up (removed) or partially picked up (count)
        if (GetComponent<SaveableObject>() == null)
        {
            gameObject.AddComponent<SaveableObject>();
        }
    }

    public string CaptureState()
    {
        return JsonUtility.ToJson(new SaveState
        {
            itemId = item?.Data?.Id,
            count = count,
            identified = identified,
            hasGauge = item != null && item.HasGauge,
            gauge = item != null && item.HasGauge ? item.Gauge : 0f
        });
    }

    public void RestoreState(string json)
    {
        SaveState state = JsonUtility.FromJson<SaveState>(json);

        if (!string.IsNullOrEmpty(state.itemId) && SaveManager.Instance != null)
        {
            ItemData data = SaveManager.Instance.GetItem(state.itemId);

            if (data != null)
            {
                item = Item.CreateFromData(data);

                if (state.hasGauge)
                {
                    item.SetGauge(state.gauge);
                }
            }
        }

        count = state.count;
        identified = state.identified;
    }

    // Spawn the item model in front of the source, thrown forward. Returns null if the item has no valid model
    public static Pickupable Spawn(Item item, int count, Transform source)
    {
        GameObject model = item?.Data?.Model;

        if (model == null || model.GetComponent<Pickupable>() == null)
        {
            Debug.LogError("Cannot drop " + (item?.Data?.Name ?? "null item") + ": its model is missing or has no Pickupable");
            return null;
        }

        GameObject instance = Instantiate(model, source.position + source.forward * 1f, source.rotation, null);
        Pickupable pickupable = instance.GetComponent<Pickupable>();

        pickupable.item = item;
        pickupable.count = count;
        pickupable.identified = true;

        // Created during the game: the SaveManager instantiates it again from the item model when the world loads
        instance.GetComponent<SaveableObject>().SetRuntime(SaveManager.ItemPrefabKeyPrefix + item.Data.Id);

        // Not "??": Unity's fake null from GetComponent is not seen as null by that operator
        if (!instance.TryGetComponent(out Rigidbody rb))
        {
            rb = instance.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.AddForce(source.forward * 50f);

        return pickupable;
    }

    public string GetInfo()
    {
        return identified ? count + "x " + item?.Data.Name : "Pickup";
    }

    public void Interract(GameObject interractor)
    {
        Pickuper pickuper = interractor.GetComponent<Pickuper>();

        if (pickuper != null)
        {
            count = pickuper.Pickup(item, count);

            if(count <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
