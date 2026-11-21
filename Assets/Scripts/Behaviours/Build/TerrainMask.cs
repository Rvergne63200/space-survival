using System.Collections.Generic;
using UnityEngine;

// Volume of a construction (above its floor, inside its walls) where the terrain is not drawn: the shader
// "Terrain/World UV Standard" discards the terrain inside every mask (_TerrainMaskCount, _TerrainMasks, _TerrainMaskShapes).
// Visual only: the flattening (TerrainFlattening) already puts the terrain and its collisions under the floor, the
// masks hide the few centimeters that may still show along the walls.
// Added by the finished constructions: Dome (cylinder), DomeConnector, Airlock and Corridor (boxes)
public class TerrainMask : MonoBehaviour
{
    public enum Shape { Box = 0, Cylinder = 1 }

    public const int MaxMasks = 64;

    private static readonly List<TerrainMask> all = new List<TerrainMask>();
    private static readonly Matrix4x4[] matrices = new Matrix4x4[MaxMasks];
    private static readonly float[] shapes = new float[MaxMasks];
    private static readonly int CountId = Shader.PropertyToID("_TerrainMaskCount");
    private static readonly int MasksId = Shader.PropertyToID("_TerrainMasks");
    private static readonly int ShapesId = Shader.PropertyToID("_TerrainMaskShapes");

    private Shape shape;
    private Vector3 center;
    private Vector3 size = Vector3.zero;
    private float shear;

    // Adds (or updates) the mask of the object: a box, or a cylinder (axis: local Y, diameter: size.x and size.z), in the
    // local space of the object
    // shear: y goes up by shear x z (local space), for a sheared corridor (CorridorPieces)
    public static TerrainMask Set(GameObject owner, Shape shape, Vector3 center, Vector3 size, float shear = 0f)
    {
        TerrainMask mask = owner.GetComponent<TerrainMask>();
        if (mask == null) mask = owner.AddComponent<TerrainMask>();

        mask.shape = shape;
        mask.center = center;
        mask.size = size;
        mask.shear = shear;
        UpdateMasks();
        return mask;
    }

    // Floor to roof of a box along Z from z0 to z1 (local space), "halfWidth" on each side
    public static TerrainMask SetBox(GameObject owner, float halfWidth, float bottom, float top, float z0, float z1, float shear = 0f)
    {
        float zCenter = (z0 + z1) * 0.5f;
        return Set(owner, Shape.Box, new Vector3(0f, (bottom + top) * 0.5f + shear * zCenter, zCenter), new Vector3(halfWidth * 2f, top - bottom, z1 - z0), shear);
    }

    private void OnEnable()
    {
        all.Add(this);
        UpdateMasks();
    }

    private void OnDisable()
    {
        all.Remove(this);
        UpdateMasks();
    }

    // World to unit volume (|p| < 0.5)
    private Matrix4x4 Matrix
    {
        get
        {
            Matrix4x4 sheared = Matrix4x4.identity;
            sheared.m12 = shear;
            return (transform.localToWorldMatrix * Matrix4x4.Translate(center) * sheared * Matrix4x4.Scale(size)).inverse;
        }
    }

    private static void UpdateMasks()
    {
        int count = 0;

        foreach (TerrainMask mask in all)
        {
            if (count >= MaxMasks) break;
            if (mask.size.x <= 0f || mask.size.y <= 0f || mask.size.z <= 0f) continue;

            matrices[count] = mask.Matrix;
            shapes[count] = (float)mask.shape;
            count++;
        }

        Shader.SetGlobalInt(CountId, count);
        Shader.SetGlobalMatrixArray(MasksId, matrices);
        Shader.SetGlobalFloatArray(ShapesId, shapes);
    }
}
