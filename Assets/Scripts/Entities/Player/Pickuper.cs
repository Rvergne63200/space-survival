using UnityEngine;
using UnityEngine.Events;

public class Pickuper : MonoBehaviour
{
    public UnityEvent<int, ItemData> ev_pickup;

    public int Pickup(Item item, int count)
    {
        PlayerInventory inventory = GetComponent<PlayerInventory>();
        int rest = inventory.Add(item, count);

        ev_pickup.Invoke(count - rest, item.Data);

        return rest;
    }
}
