using UnityEngine;

// A corridor between two domes (Corridor): built from the build wheel by aiming at a first dome, then at a second one.
// A straight corridor is made between the flanges of their connectors, from pieces (CorridorPieces): straight pieces
// repeated and centered, and a stretched extension at each end. Its prefab is an empty root (pivot at the center of the
// floor, Z along the corridor). The cost and the refund of the StructureData are per meter (rounded up).
// The max slope of the StructureData is the max height difference between the two ends (meters)
[CreateAssetMenu(menuName = "Construction/Corridor")]
public class CorridorData : StructureData
{
    [SerializeField] private float _minLength = 2f;
    public float MinLength => _minLength;

    [SerializeField] private float _maxLength = 20f;
    public float MaxLength => _maxLength;

    // Pieces of the model (Resources/Models/Tunnel): straight piece repeated, and stretchable extension
    [SerializeField] private GameObject _straightPiece;
    public GameObject StraightPiece => _straightPiece;

    [SerializeField] private float _straightLength = 2f;
    public float StraightLength => _straightLength;

    [SerializeField] private GameObject _extensionPiece;
    public GameObject ExtensionPiece => _extensionPiece;

    // Length of the extension model before it is stretched
    [SerializeField] private float _extensionLength = 1f;
    public float ExtensionLength => _extensionLength;

    // Longest extension, in straight pieces (0.75: 1.5 m). An extension is at least half a straight piece shorter (0.25:
    // 0.5 m): shorter, it shows, and on a slope the tilted straight piece would go into the connector. One straight piece
    // less then gives each extension half a straight piece more
    [SerializeField] private float _maxExtension = 0.75f;
    public float MaxExtension => _maxExtension;

    public float MinExtensionLength => Mathf.Max(_maxExtension - 0.5f, 0f) * _straightLength;
}
