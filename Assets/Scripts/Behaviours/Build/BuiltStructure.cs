using UnityEngine;

// A finished structure (BuildUtility.InstantiateStructure): destroyed from the build wheel, it gives back its refund
public class BuiltStructure : MonoBehaviour
{
    public StructureData Data { get; private set; }

    public void Initialize(StructureData data)
    {
        Data = data;
    }

    public void Deconstruct()
    {
        BuildUtility.GiveStoragesToPlayer(gameObject);

        if (Data != null)
        {
            BuildUtility.GiveToPlayer(Data.Refund);
        }

        Destroy(gameObject);
    }
}
