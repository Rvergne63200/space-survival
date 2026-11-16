using UnityEngine;
using UnityEngine.Events;

public class Pickuper : MonoBehaviour
{
    public UnityEvent<int, ItemData> ev_pickup;

    private PlayerContext playerContext;

    private void Awake()
    {
        playerContext = GetComponent<PlayerContext>();
    }

    public int Pickup(Item item, int count)
    {
        PlayerInventory inventory = playerContext.PlayerManager.Inventory;
        int rest = inventory.Add(item, count);

        ev_pickup.Invoke(count - rest, item.Data);

        return rest;
    }
}
