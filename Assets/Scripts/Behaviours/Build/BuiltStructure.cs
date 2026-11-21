using System.Collections.Generic;
using UnityEngine;

// A finished structure (BuildUtility.InstantiateStructure): destroyed from the build wheel, it gives back its refund.
// The corridors and airlocks attached to it (Corridor, Airlock) are destroyed and refunded with it
public class BuiltStructure : MonoBehaviour
{
    public StructureData Data { get; private set; }

    // Refund multiplied by it: the length of a corridor (per meter), 1 otherwise
    public int CostMultiplier { get; set; } = 1;

    public void Initialize(StructureData data)
    {
        Data = data;
    }

    public void Deconstruct()
    {
        foreach (Corridor corridor in new List<Corridor>(Corridor.All))
        {
            if (corridor != null && corridor.gameObject != gameObject && corridor.Links(this))
            {
                corridor.Deconstruct();
            }
        }

        foreach (Airlock airlock in new List<Airlock>(Airlock.All))
        {
            if (airlock != null && airlock.gameObject != gameObject && airlock.Links(this))
            {
                airlock.Deconstruct();
            }
        }

        BuildUtility.GiveStoragesToPlayer(gameObject);

        if (Data != null)
        {
            foreach (RecipeData.ItemBlock block in Data.Refund)
            {
                BuildUtility.GiveToPlayer(block.Item, block.Quantity * CostMultiplier);
            }
        }

        Destroy(gameObject);
    }
}
