using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour, IInterractable
{
    public Item item;
    public int count;

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
