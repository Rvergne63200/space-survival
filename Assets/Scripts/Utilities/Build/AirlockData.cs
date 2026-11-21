using UnityEngine;

// An airlock (Airlock): built from the build wheel, placed freely, and attached to the nearest dome when its wall is
// within "snapDistance" of the aimed point (the ghost then sticks to the wall, with the connector). Not attached: it
// cannot be placed. The origin of its prefab is the face against the connector flange, +Z outward
[CreateAssetMenu(menuName = "Construction/Airlock")]
public class AirlockData : StructureData
{
    [SerializeField] private float _snapDistance = 3f;
    public float SnapDistance => _snapDistance;
}
