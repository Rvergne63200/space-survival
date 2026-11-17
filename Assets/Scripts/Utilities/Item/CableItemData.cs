using UnityEngine;

// Cable to link two devices (CablePlacer, while it is in hand). Laying a cable uses one item every metersPerItem meters
[CreateAssetMenu(menuName = "Item/Cable")]
public class CableItemData : ItemData
{
    [SerializeField] private float metersPerItem = 1f;
    public float MetersPerItem => Mathf.Max(0.01f, metersPerItem);

    [SerializeField] private float width = 0.05f;
    public float Width => width;

    [SerializeField] private Material material;
    public Material Material => material;

    public int ItemsFor(float length)
    {
        return Mathf.Max(1, Mathf.CeilToInt(length / MetersPerItem - 0.001f));
    }
}
