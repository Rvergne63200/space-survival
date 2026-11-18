using UnityEngine;

// A corridor between two buildings (Corridor): built from the build wheel by aiming at a first building, then at a
// second one. A straight corridor is made between their edges. Its prefab is 1 m long along Z (pivot at the center of
// the floor), stretched to the length. The cost and the refund of the StructureData are per meter (rounded up).
// The max slope of the StructureData is the max height difference between the two ends (meters)
[CreateAssetMenu(menuName = "Construction/Corridor")]
public class CorridorData : StructureData
{
    [SerializeField] private float _minLength = 2f;
    public float MinLength => _minLength;

    [SerializeField] private float _maxLength = 20f;
    public float MaxLength => _maxLength;
}
