using UnityEngine;

// Flattening of the terrain under the constructions, when their plan is placed (Builder, if StructureData.FlattenTerrain):
// - footprint: a disc (dome) or a rectangle, turned and tilted along its length (airlock, corridor between two domes at
//   different heights), slightly larger than the construction ("Extra": the heightmap has a sample every meter)
// - inside: the terrain is set "Clearance" under the floor, dug or filled, so it never crosses the floor and the
//   construction never floats; around it, it joins the natural terrain over "Margin" meters (smooth)
// Saved (TerrainEditSaveData) and replayed on the chunks generated later (ChunkManager). The few centimeters that may
// still show along the walls between two samples are hidden by the terrain masks (TerrainMask)
public static class TerrainFlattening
{
    // Floor of the domes, airlocks and corridors above their origin (ground of the model), and the terrain under it
    public const float FloorHeight = 0.2f;
    public const float Clearance = 0.18f;
    public const float Extra = 0.75f;
    public const float Margin = 2.5f;

    public static float TargetHeight(float groundY) => groundY + FloorHeight - Clearance;

    public static void Disc(Vector3 center, float radius, float groundY)
    {
        Add(new TerrainEditSaveData
        {
            shape = TerrainEditSaveData.Disc,
            position = center,
            height = TargetHeight(groundY),
            radius = radius + Extra,
            margin = Margin
        });
    }

    // groundY: ground of the construction at the center; slope: height change per meter along the forward of the rotation
    // (horizontal)
    public static void Box(Vector3 center, Quaternion rotation, float halfWidth, float halfLength, float groundY, float slope)
    {
        Vector3 forward = rotation * Vector3.forward;

        Add(new TerrainEditSaveData
        {
            shape = TerrainEditSaveData.Box,
            position = center,
            height = TargetHeight(groundY),
            yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg,
            halfWidth = halfWidth + Extra,
            halfLength = halfLength + Extra,
            slope = slope,
            margin = Margin
        });
    }

    private static void Add(TerrainEditSaveData edit)
    {
        if (ChunkManager.Instance != null) ChunkManager.Instance.AddTerrainEdit(edit);
    }

    // Horizontal distance from the center beyond which the terrain is not changed
    public static float Reach(TerrainEditSaveData edit)
    {
        float footprint = edit.shape == TerrainEditSaveData.Disc
            ? edit.radius
            : new Vector2(edit.halfWidth, edit.halfLength).magnitude;

        return footprint + edit.margin;
    }

    // Target height of the terrain at the world point (x, z) and weight (1 inside the footprint, down to 0 at the end of
    // the margin). False when the point is out of reach
    public static bool Evaluate(TerrainEditSaveData edit, Vector3 world, out float target, out float weight)
    {
        target = edit.height;
        weight = 0f;

        Vector3 offset = world - edit.position;
        offset.y = 0f;
        float outside;

        if (edit.shape == TerrainEditSaveData.Disc)
        {
            outside = offset.magnitude - edit.radius;
        }
        else if (edit.shape == TerrainEditSaveData.Box)
        {
            Vector3 local = Quaternion.Euler(0f, -edit.yaw, 0f) * offset;
            float dx = Mathf.Max(0f, Mathf.Abs(local.x) - edit.halfWidth);
            float dz = Mathf.Max(0f, Mathf.Abs(local.z) - edit.halfLength);
            outside = new Vector2(dx, dz).magnitude;
            target += edit.slope * Mathf.Clamp(local.z, -edit.halfLength, edit.halfLength);
        }
        else
        {
            return false;
        }

        if (outside <= 0f)
        {
            weight = 1f;
            return true;
        }

        if (outside >= edit.margin) return false;

        weight = Mathf.SmoothStep(0f, 1f, 1f - outside / edit.margin);
        return true;
    }
}
