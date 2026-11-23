using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Shared by the stylized maps: the map of the control panel (NetworkMapElement) and the minimap of the HUD (MinimapUI).
// Topography of the terrain (generated heights, ChunkManager.GeneratedHeightAt: tint by height, shading by slope,
// contour lines), and outlines of objects seen from above (convex hull of their meshes)
public static class MapUtility
{
    public const float ContourStep = 4f;

    public static readonly Color BaseColor = new Color(0.02f, 0.05f, 0.09f);
    public static readonly Color HighColor = new Color(0.06f, 0.16f, 0.24f);
    public static readonly Color ContourColor = new Color(0.2f, 0.65f, 0.85f);
    public static readonly Color GridColor = new Color(0.3f, 0.7f, 1f, 0.08f);
    public static readonly Color BuildingColor = new Color(0.55f, 0.85f, 1f);
    public static readonly Color PlacedColor = new Color(1f, 0.75f, 0.35f);

    // ---------- Topography ----------

    // Fills the texture with the topography of an area (tint and shading): southWest is the world (X, Z) of the bottom
    // left corner, step the meters between two pixels. Texture row 0 is at the bottom (the south). The contour lines are
    // given apart, as segments to draw (DrawContours)
    public static void FillTopography(Texture2D texture, Vector2 southWest, float stepX, float stepY, List<ContourSegment> contours)
    {
        FillTopography(texture, southWest, stepX, stepY, contours, BaseColor, HighColor, 1f);
    }

    // Same, with the colors of the low and high grounds and the opacity of the texture (lighter background of the minimap)
    public static void FillTopography(Texture2D texture, Vector2 southWest, float stepX, float stepY, List<ContourSegment> contours,
        Color lowColor, Color highColor, float alpha)
    {
        int width = texture.width;
        int height = texture.height;
        ChunkManager chunks = ChunkManager.Instance;
        float[,] heights = new float[width + 1, height + 1];
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        // The generated height is a pure function (TerrainShape): the columns are sampled in parallel
        System.Threading.Tasks.Parallel.For(0, width + 1, x =>
        {
            for (int y = 0; y <= height; y++)
            {
                heights[x, y] = chunks != null ? chunks.GeneratedHeightAt(southWest.x + x * stepX, southWest.y + y * stepY) : 0f;
            }
        });

        foreach (float value in heights)
        {
            minHeight = Mathf.Min(minHeight, value);
            maxHeight = Mathf.Max(maxHeight, value);
        }

        float range = Mathf.Max(1f, maxHeight - minHeight);
        Color[] pixels = new Color[width * height];
        Vector3 light = new Vector3(-1f, 1.5f, 1f).normalized;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = heights[x, y];
                float right = heights[x + 1, y];
                float up = heights[x, y + 1];

                // Height as a tint, slope as a shading
                Color color = Color.Lerp(lowColor, highColor, (value - minHeight) / range);
                Vector3 normal = new Vector3(value - right, stepX, value - up).normalized;
                float shade = Mathf.Clamp01(Vector3.Dot(normal, light));
                color *= 0.75f + shade * 0.5f;

                color.a = alpha;
                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        if (contours != null)
        {
            BuildContours(heights, southWest, stepX, stepY, contours);
        }
    }

    // ---------- Contour lines ----------

    // A piece of contour line (world X, Z). Major: every fifth line, brighter
    public struct ContourSegment
    {
        public Vector2 a;
        public Vector2 b;
        public bool major;
    }

    // Marching squares on the height grid: the contour lines as segments, interpolated between the samples (smooth lines
    // drawn as vectors, not pixels of the texture)
    private static void BuildContours(float[,] heights, Vector2 southWest, float stepX, float stepY, List<ContourSegment> contours)
    {
        contours.Clear();

        int width = heights.GetLength(0) - 1;
        int height = heights.GetLength(1) - 1;
        Vector2[] crossings = new Vector2[4];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Corners counterclockwise from the south-west, and the edges between them
                float h0 = heights[x, y];
                float h1 = heights[x + 1, y];
                float h2 = heights[x + 1, y + 1];
                float h3 = heights[x, y + 1];

                float min = Mathf.Min(Mathf.Min(h0, h1), Mathf.Min(h2, h3));
                float max = Mathf.Max(Mathf.Max(h0, h1), Mathf.Max(h2, h3));

                Vector2 p0 = southWest + new Vector2(x * stepX, y * stepY);
                Vector2 p1 = p0 + new Vector2(stepX, 0f);
                Vector2 p2 = p0 + new Vector2(stepX, stepY);
                Vector2 p3 = p0 + new Vector2(0f, stepY);

                for (int level = Mathf.CeilToInt(min / ContourStep); level * ContourStep <= max; level++)
                {
                    float value = level * ContourStep;
                    int count = 0;

                    Cross(h0, h1, p0, p1, value, crossings, ref count);
                    Cross(h1, h2, p1, p2, value, crossings, ref count);
                    Cross(h2, h3, p2, p3, value, crossings, ref count);
                    Cross(h3, h0, p3, p0, value, crossings, ref count);

                    bool major = level % 5 == 0;

                    // Two crossings: one segment. Four (saddle): two segments, edge after edge
                    for (int i = 0; i + 1 < count; i += 2)
                    {
                        contours.Add(new ContourSegment { a = crossings[i], b = crossings[i + 1], major = major });
                    }
                }
            }
        }
    }

    // The level crosses the edge (one end below, the other at or above): point interpolated on it
    private static void Cross(float ha, float hb, Vector2 a, Vector2 b, float level, Vector2[] crossings, ref int count)
    {
        if ((ha < level) == (hb < level) || count >= crossings.Length) return;

        float t = (level - ha) / (hb - ha);
        crossings[count++] = Vector2.Lerp(a, b, t);
    }

    // Segments stroked per path: a UI Toolkit mesh holds at most 65535 vertices, a very zoomed out map has more segments
    private const int SegmentsPerStroke = 2000;

    // Contour lines, toLocal giving the position on the element of a world (X, Z). Stroked in batches (SegmentsPerStroke)
    public static void DrawContours(Painter2D painter, List<ContourSegment> contours, Func<Vector2, Vector2> toLocal)
    {
        if (contours == null || contours.Count == 0) return;

        for (int pass = 0; pass < 2; pass++)
        {
            bool major = pass == 1;

            painter.strokeColor = new Color(ContourColor.r, ContourColor.g, ContourColor.b, major ? 0.6f : 0.3f);
            painter.lineWidth = major ? 1.4f : 1f;
            int inPath = 0;

            foreach (ContourSegment segment in contours)
            {
                if (segment.major != major) continue;

                if (inPath == 0) painter.BeginPath();

                painter.MoveTo(toLocal(segment.a));
                painter.LineTo(toLocal(segment.b));

                if (++inPath >= SegmentsPerStroke)
                {
                    painter.Stroke();
                    inPath = 0;
                }
            }

            if (inPath > 0) painter.Stroke();
        }
    }

    // ---------- Outlines ----------

    // Outlines of every finished structure (buildings, corridors)
    // owners (optional): the structure of each outline, same index
    public static void CollectBuildings(List<Vector2[]> outlines, List<BuiltStructure> owners = null)
    {
        outlines.Clear();
        owners?.Clear();

        foreach (BuiltStructure structure in UnityEngine.Object.FindObjectsByType<BuiltStructure>())
        {
            Vector2[] outline = Footprint(structure);

            if (outline.Length < 3) continue;

            outlines.Add(outline);
            owners?.Add(structure);
        }
    }

    // The point (world X, Z) is inside the convex outline
    public static bool Contains(Vector2[] outline, Vector2 point)
    {
        if (outline == null || outline.Length < 3) return false;

        // Counterclockwise hull: the point is on the left of every edge
        for (int i = 0; i < outline.Length; i++)
        {
            if (Cross(outline[i], outline[(i + 1) % outline.Length], point) < 0f) return false;
        }

        return true;
    }

    // Outlines of every placed object (crates, machines, generators...)
    public static void CollectPlaced(List<Vector2[]> outlines)
    {
        outlines.Clear();

        foreach (PlacedObject placed in UnityEngine.Object.FindObjectsByType<PlacedObject>())
        {
            AddOutline(outlines, placed);
        }
    }

    private static void AddOutline(List<Vector2[]> outlines, Component root)
    {
        Vector2[] outline = Footprint(root);

        if (outline.Length >= 3)
        {
            outlines.Add(outline);
        }
    }

    // Convex outline (world X, Z) of the vertices of the meshes of the object. A mesh that cannot be read gives the
    // corners of its bounds
    public static Vector2[] Footprint(Component root)
    {
        List<Vector2> points = new List<Vector2>();

        foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
        {
            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            Mesh mesh = filter.sharedMesh;

            if (mesh == null || renderer == null || !renderer.enabled) continue;

            Matrix4x4 matrix = filter.transform.localToWorldMatrix;

            if (mesh.isReadable)
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    Vector3 world = matrix.MultiplyPoint3x4(vertex);
                    points.Add(new Vector2(world.x, world.z));
                }
            }
            else
            {
                Bounds bounds = mesh.bounds;

                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 local = bounds.center + Vector3.Scale(bounds.extents, new Vector3(
                        (corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                    Vector3 world = matrix.MultiplyPoint3x4(local);
                    points.Add(new Vector2(world.x, world.z));
                }
            }
        }

        return ConvexHull(points);
    }

    // Monotone chain: the hull in counterclockwise order
    private static Vector2[] ConvexHull(List<Vector2> points)
    {
        if (points.Count < 3) return points.ToArray();

        points.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        Vector2[] hull = new Vector2[points.Count * 2];
        int count = 0;

        for (int i = 0; i < points.Count; i++)
        {
            while (count >= 2 && Cross(hull[count - 2], hull[count - 1], points[i]) <= 0f) count--;
            hull[count++] = points[i];
        }

        for (int i = points.Count - 2, lower = count + 1; i >= 0; i--)
        {
            while (count >= lower && Cross(hull[count - 2], hull[count - 1], points[i]) <= 0f) count--;
            hull[count++] = points[i];
        }

        // The last point is the first one
        Array.Resize(ref hull, Mathf.Max(0, count - 1));
        return hull;
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    // Filled outline, toLocal giving the position on the element of a world (X, Z)
    public static void DrawOutline(Painter2D painter, Vector2[] outline, Func<Vector2, Vector2> toLocal, Color color, float lineWidth = 1.5f)
    {
        if (outline == null || outline.Length < 3) return;

        painter.fillColor = new Color(color.r, color.g, color.b, 0.12f);
        painter.strokeColor = new Color(color.r, color.g, color.b, 0.8f);
        painter.lineWidth = lineWidth;
        painter.BeginPath();
        painter.MoveTo(toLocal(outline[0]));

        for (int i = 1; i < outline.Length; i++)
        {
            painter.LineTo(toLocal(outline[i]));
        }

        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }
}
