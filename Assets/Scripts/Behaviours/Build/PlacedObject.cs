using UnityEngine;

// An item placed in the world (BuildUtility.InstantiatePlaced): destroyed from the build wheel, it gives back its refund
// (the item itself by default) and the content of its storages
public class PlacedObject : MonoBehaviour
{
    public PlaceableItemData Data { get; private set; }

    public void Initialize(PlaceableItemData data)
    {
        Data = data;
    }

    public void Deconstruct()
    {
        BuildUtility.GiveStoragesToPlayer(gameObject);

        if (Data != null)
        {
            if (Data.Refund.Count > 0)
            {
                BuildUtility.GiveToPlayer(Data.Refund);
            }
            else
            {
                BuildUtility.GiveToPlayer(Data, 1);
            }
        }

        Destroy(gameObject);
    }
}
