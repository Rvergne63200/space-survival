using UnityEngine;

// Inside of a building: a trigger collider on a child of the building prefab (on the Ignore Raycast layer, so that it
// does not stop the aim nor the placement of objects). It tells what is inside the building: the electricity of the
// building (BuildingPowerPanel) uses it, later the oxygen of the room and the player inside (F51, F55)
[RequireComponent(typeof(Collider))]
public class BuildingInterior : MonoBehaviour
{
    private Collider zone;
    private Collider Zone => zone != null ? zone : (zone = GetComponent<Collider>());

    // Horizontal radius of the zone (edge of the building, where the corridors start)
    public float Radius => Zone != null ? Zone.bounds.extents.x : 0f;

    // The point is inside the zone (a convex collider: sphere, box, capsule or convex mesh)
    public bool Contains(Vector3 point)
    {
        if (Zone == null || !Zone.enabled || !gameObject.activeInHierarchy) return false;

        return (Zone.ClosestPoint(point) - point).sqrMagnitude < 0.0001f;
    }
}
