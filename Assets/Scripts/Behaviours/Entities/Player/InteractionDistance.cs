using UnityEngine;

public static class InteractionDistance
{
    // Distance between a position and the closest point of the colliders of an object (its position without collider).
    // A non convex mesh collider (models: coal generator...) has no closest point: the closest point of its bounds
    public static float To(Component target, Vector3 position)
    {
        float closest = float.MaxValue;

        foreach (Collider collider in target.GetComponentsInChildren<Collider>())
        {
            if (!collider.enabled) continue;

            Vector3 point = collider is MeshCollider mesh && !mesh.convex ? collider.bounds.ClosestPoint(position) : collider.ClosestPoint(position);
            closest = Mathf.Min(closest, Vector3.Distance(position, point));
        }

        return closest < float.MaxValue ? closest : Vector3.Distance(position, target.transform.position);
    }
}
