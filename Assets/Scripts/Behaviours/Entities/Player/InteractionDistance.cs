using UnityEngine;

public static class InteractionDistance
{
    // Distance between a position and the closest point of the colliders of an object (its position without collider)
    public static float To(Component target, Vector3 position)
    {
        float closest = float.MaxValue;

        foreach (Collider collider in target.GetComponentsInChildren<Collider>())
        {
            if (!collider.enabled) continue;

            closest = Mathf.Min(closest, Vector3.Distance(position, collider.ClosestPoint(position)));
        }

        return closest < float.MaxValue ? closest : Vector3.Distance(position, target.transform.position);
    }
}
