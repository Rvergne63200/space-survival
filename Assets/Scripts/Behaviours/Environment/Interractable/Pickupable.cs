using UnityEngine;

public class Pickupable : MonoBehaviour, IInterractable
{
    public Item item;
    public int count;

    public bool identified = false;

    [SerializeField] private Sprite icon;

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

    public Sprite GetIcon()
    {
        return icon;
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
