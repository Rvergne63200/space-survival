using System.Collections.Generic;
using UnityEngine;

// Removes from a mesh what is inside convex volumes (boxes given by their planes, normals outward): each triangle is
// clipped plane by plane, the parts outside are kept, the part inside every plane is dropped. Used for the colliders
// of the domes, cut by their connectors (Dome.RefreshColliders). Positions only (no normals or UVs: physics only)
public static class MeshCutter
{
    public static Mesh CutOut(Mesh source, Matrix4x4 localToWorld, List<List<Plane>> volumes)
    {
        Vector3[] sourceVertices = source.vertices;
        int[] sourceTriangles = source.triangles;
        Matrix4x4 worldToLocal = localToWorld.inverse;

        List<Vector3> vertices = new List<Vector3>(sourceVertices.Length);
        List<int> triangles = new List<int>(sourceTriangles.Length);

        List<List<Vector3>> polygons = new List<List<Vector3>>();
        List<List<Vector3>> next = new List<List<Vector3>>();

        for (int t = 0; t < sourceTriangles.Length; t += 3)
        {
            polygons.Clear();
            polygons.Add(new List<Vector3>
            {
                localToWorld.MultiplyPoint3x4(sourceVertices[sourceTriangles[t]]),
                localToWorld.MultiplyPoint3x4(sourceVertices[sourceTriangles[t + 1]]),
                localToWorld.MultiplyPoint3x4(sourceVertices[sourceTriangles[t + 2]])
            });

            foreach (List<Plane> volume in volumes)
            {
                next.Clear();

                foreach (List<Vector3> polygon in polygons)
                {
                    CutPolygon(polygon, volume, next);
                }

                List<List<Vector3>> swap = polygons;
                polygons = next;
                next = swap;
            }

            // Fan triangulation of each convex piece, back in the local space
            foreach (List<Vector3> polygon in polygons)
            {
                int first = vertices.Count;

                foreach (Vector3 point in polygon)
                {
                    vertices.Add(worldToLocal.MultiplyPoint3x4(point));
                }

                for (int i = 1; i < polygon.Count - 1; i++)
                {
                    triangles.Add(first);
                    triangles.Add(first + i);
                    triangles.Add(first + i + 1);
                }
            }
        }

        Mesh mesh = new Mesh { name = source.name + " (cut)" };
        mesh.indexFormat = vertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    // Adds to "outside" the parts of the convex polygon outside the volume
    private static void CutPolygon(List<Vector3> polygon, List<Plane> volume, List<List<Vector3>> outside)
    {
        // Fully outside one of the planes: kept whole
        foreach (Plane plane in volume)
        {
            bool allOut = true;

            foreach (Vector3 point in polygon)
            {
                if (plane.GetDistanceToPoint(point) <= 0f)
                {
                    allOut = false;
                    break;
                }
            }

            if (allOut)
            {
                outside.Add(polygon);
                return;
            }
        }

        List<Vector3> remaining = polygon;

        foreach (Plane plane in volume)
        {
            Split(remaining, plane, out List<Vector3> front, out List<Vector3> back);

            if (front.Count >= 3) outside.Add(front);

            remaining = back;

            if (remaining.Count < 3) return;
        }

        // What remains is inside the volume: dropped
    }

    // Sutherland-Hodgman: front = positive side of the plane (outside the volume), back = the other side
    private static void Split(List<Vector3> polygon, Plane plane, out List<Vector3> front, out List<Vector3> back)
    {
        front = new List<Vector3>();
        back = new List<Vector3>();

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 a = polygon[i];
            Vector3 b = polygon[(i + 1) % polygon.Count];
            float da = plane.GetDistanceToPoint(a);
            float db = plane.GetDistanceToPoint(b);

            if (da > 0f) front.Add(a); else back.Add(a);

            if ((da > 0f) != (db > 0f))
            {
                Vector3 crossing = Vector3.Lerp(a, b, da / (da - db));
                front.Add(crossing);
                back.Add(crossing);
            }
        }
    }
}
