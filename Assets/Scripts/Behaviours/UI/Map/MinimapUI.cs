using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Round minimap of the HUD (element "minimap" of Hud.uxml), turning with the view: what is in front of the player is at
// the top. Stylized like the map of the control panel (MapUtility), without the electrical network:
// - topography of a square tile around the player (generated heights), generated again when the player gets close to
//   its edge
// - a grid every 25 m, the outlines of the buildings and corridors (BuiltStructure) and of the placed objects
//   (PlacedObject), collected again every "refreshInterval" seconds
// The tile (element "minimap-image") is moved and turned around the player; the arrow at the center is the player
public class MinimapUI : MonoBehaviour
{
    private const float GridStep = 25f;

    [SerializeField] private UIDocument document;
    // Distance shown from the center to the edge (m)
    [SerializeField] private float radius = 50f;
    // Side of the topography tile, in radius (the player can move about (factor / 2 - 1) radius before it is generated again)
    [SerializeField] private float tileFactor = 3f;
    // Pixels of the topography on a side of the tile
    [SerializeField] private int topographyResolution = 256;
    [SerializeField] private float refreshInterval = 1f;

    // Background of the minimap: lighter and more transparent than the map of the control panel (lines and outlines unchanged)
    [SerializeField] private Color lowColor = new Color(0.16f, 0.25f, 0.33f);
    [SerializeField] private Color highColor = new Color(0.3f, 0.43f, 0.53f);
    [Range(0f, 1f)] [SerializeField] private float backgroundAlpha = 0.5f;

    private CameraMovement view;

    private Texture2D topography;
    private readonly List<MapUtility.ContourSegment> contours = new List<MapUtility.ContourSegment>();
    private bool hasTile;
    private Vector2 tileCenter;
    private float tileSize;

    private readonly List<Vector2[]> buildings = new List<Vector2[]>();
    private readonly List<Vector2[]> placed = new List<Vector2[]>();
    private float refreshTimer;

    private VisualElement boundRoot;
    private VisualElement minimap;
    private VisualElement image;
    private VisualElement north;

    private void OnDestroy()
    {
        if (topography != null)
        {
            Destroy(topography);
        }
    }

    // The UIDocument builds its elements again each time it is enabled
    private bool Bind()
    {
        if (document == null || document.rootVisualElement == null) return false;

        VisualElement root = document.rootVisualElement;

        if (root == boundRoot && minimap != null) return true;

        boundRoot = root;
        minimap = root.Q("minimap");
        image = root.Q("minimap-image");
        north = root.Q("minimap-north");

        if (image != null)
        {
            image.generateVisualContent += DrawTile;

            if (topography != null)
            {
                image.style.backgroundImage = Background.FromTexture2D(topography);
            }
        }

        VisualElement arrow = root.Q("minimap-arrow");

        if (arrow != null)
        {
            arrow.generateVisualContent += DrawArrow;
        }

        return minimap != null && image != null;
    }

    private static void DrawArrow(MeshGenerationContext context)
    {
        Rect rect = context.visualElement.contentRect;
        Painter2D painter = context.painter2D;

        painter.fillColor = new Color(1f, 0.85f, 0.2f);
        painter.strokeColor = new Color(0f, 0f, 0f, 0.8f);
        painter.lineWidth = 1.5f;
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.width * 0.5f, 0f));
        painter.LineTo(new Vector2(rect.width, rect.height));
        painter.LineTo(new Vector2(rect.width * 0.5f, rect.height * 0.75f));
        painter.LineTo(new Vector2(0f, rect.height));
        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }

    private void LateUpdate()
    {
        if (!Bind()) return;

        GameObject player = PlayerManager.Instance != null ? PlayerManager.Instance.Player : null;
        bool shown = player != null && !PlayerManager.Instance.IsDead;

        minimap.EnableInClassList("hidden", !shown);

        if (!shown) return;

        Vector2 position = new Vector2(player.transform.position.x, player.transform.position.z);

        // New tile when the circle shown would leave the current one
        float half = radius * tileFactor * 0.5f;
        Vector2 offset = position - tileCenter;

        if (!hasTile || Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.y)) > half - radius * 1.1f)
        {
            GenerateTile(position);
        }

        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            MapUtility.CollectBuildings(buildings);
            MapUtility.CollectPlaced(placed);
            image.MarkDirtyRepaint();
        }

        // Direction of the view: the body of the player only turns with it while moving. The player camera is not
        // tagged MainCamera, it is found by its CameraMovement
        if (view == null)
        {
            view = FindAnyObjectByType<CameraMovement>();
        }

        float yaw = view != null ? view.transform.eulerAngles.y : player.transform.eulerAngles.y;
        Place(position, yaw);

        // North on the edge of the map: turned by -yaw from the top (clockwise)
        if (north != null)
        {
            float halfSize = minimap.resolvedStyle.width * 0.5f;
            float angle = -yaw * Mathf.Deg2Rad;
            float distance = halfSize - 12f;

            north.style.left = halfSize + Mathf.Sin(angle) * distance;
            north.style.top = halfSize - Mathf.Cos(angle) * distance;
        }
    }

    private void GenerateTile(Vector2 center)
    {
        hasTile = true;
        tileCenter = center;
        tileSize = radius * tileFactor;

        if (topography == null || topography.width != topographyResolution)
        {
            if (topography != null) Destroy(topography);

            topography = new Texture2D(topographyResolution, topographyResolution, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            image.style.backgroundImage = Background.FromTexture2D(topography);
        }

        float step = tileSize / topographyResolution;
        MapUtility.FillTopography(topography, center - new Vector2(tileSize, tileSize) * 0.5f, step, step, contours,
            lowColor, highColor, backgroundAlpha);
        image.MarkDirtyRepaint();
    }

    // The tile under the minimap: the player at the center, turned so that the view points up
    private void Place(Vector2 position, float yaw)
    {
        Rect area = minimap.contentRect;

        if (float.IsNaN(area.width) || area.width <= 1f) return;

        float pixelsPerMeter = area.width * 0.5f / radius;
        float sizePixels = tileSize * pixelsPerMeter;
        Vector2 player = TileLocal(position, pixelsPerMeter);

        image.style.width = sizePixels;
        image.style.height = sizePixels;
        image.style.left = area.width * 0.5f - player.x;
        image.style.top = area.height * 0.5f - player.y;
        image.style.transformOrigin = new TransformOrigin(new Length(player.x, LengthUnit.Pixel), new Length(player.y, LengthUnit.Pixel), 0f);
        image.style.rotate = new Rotate(new Angle(-yaw, AngleUnit.Degree));
    }

    // Position on the tile element of a world (X, Z): north at the top
    private Vector2 TileLocal(Vector2 world, float pixelsPerMeter)
    {
        float half = tileSize * 0.5f;
        return new Vector2((world.x - (tileCenter.x - half)) * pixelsPerMeter, ((tileCenter.y + half) - world.y) * pixelsPerMeter);
    }

    private void DrawTile(MeshGenerationContext context)
    {
        if (!hasTile) return;

        Rect rect = context.visualElement.contentRect;

        if (float.IsNaN(rect.width) || rect.width <= 1f) return;

        Painter2D painter = context.painter2D;
        float pixelsPerMeter = rect.width / tileSize;
        System.Func<Vector2, Vector2> toLocal = world => TileLocal(world, pixelsPerMeter);

        MapUtility.DrawContours(painter, contours, toLocal);

        // Grid on round coordinates
        float half = tileSize * 0.5f;
        painter.strokeColor = MapUtility.GridColor;
        painter.lineWidth = 1f;
        painter.BeginPath();

        for (float x = Mathf.Ceil((tileCenter.x - half) / GridStep) * GridStep; x <= tileCenter.x + half; x += GridStep)
        {
            painter.MoveTo(toLocal(new Vector2(x, tileCenter.y + half)));
            painter.LineTo(toLocal(new Vector2(x, tileCenter.y - half)));
        }

        for (float y = Mathf.Ceil((tileCenter.y - half) / GridStep) * GridStep; y <= tileCenter.y + half; y += GridStep)
        {
            painter.MoveTo(toLocal(new Vector2(tileCenter.x - half, y)));
            painter.LineTo(toLocal(new Vector2(tileCenter.x + half, y)));
        }

        painter.Stroke();

        foreach (Vector2[] building in buildings)
        {
            MapUtility.DrawOutline(painter, building, toLocal, MapUtility.BuildingColor);
        }

        foreach (Vector2[] outline in placed)
        {
            MapUtility.DrawOutline(painter, outline, toLocal, MapUtility.PlacedColor, 1f);
        }
    }
}
