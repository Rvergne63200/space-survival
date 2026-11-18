using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Map of an electrical network, in the control panel (ControlPanelUI), north at the top. Stylized:
// - the topography of the terrain as contour lines and shading (generated heights, ChunkManager.GeneratedHeightAt),
//   regenerated for the area shown, and a grid
// - the buildings (outline of their bounds), the cables (glowing lines colored by the state of their network) and an
//   icon per device: circle for a consumer, triangle for a generator, bar for a storage, diamond for a distribution
//   panel, square for a control panel. The control panel in use pulses
// Wheel to zoom, drag to move, click on a device to select it
public class NetworkMapElement : VisualElement
{
    private const int TopographyResolution = 160;
    private const float ContourStep = 4f;
    private const float PickRadius = 14f;
    private const float DragThreshold = 4f;
    private const float MinMetersPerPixel = 0.05f;
    private const float MaxMetersPerPixel = 20f;

    private static readonly Color BaseColor = new Color(0.02f, 0.05f, 0.09f);
    private static readonly Color HighColor = new Color(0.06f, 0.16f, 0.24f);
    private static readonly Color ContourColor = new Color(0.2f, 0.65f, 0.85f);
    private static readonly Color GridColor = new Color(0.3f, 0.7f, 1f, 0.08f);
    private static readonly Color BuildingColor = new Color(0.55f, 0.85f, 1f);
    private static readonly Color PoweredColor = new Color(0.25f, 0.95f, 1f);
    private static readonly Color TrippedColor = new Color(1f, 0.3f, 0.3f);
    private static readonly Color InactiveColor = new Color(0.45f, 0.5f, 0.55f);
    private static readonly Color GeneratorColor = new Color(1f, 0.85f, 0.25f);
    private static readonly Color StorageColor = new Color(0.4f, 1f, 0.5f);
    private static readonly Color PanelColor = new Color(1f, 0.6f, 0.2f);
    private static readonly Color HereColor = new Color(1f, 1f, 1f);
    private static readonly Color SelectedColor = new Color(1f, 1f, 1f);

    private readonly List<EnergyDevice> devices = new List<EnergyDevice>();
    private readonly List<ElectricCable> cables = new List<ElectricCable>();
    private readonly List<Rect> buildings = new List<Rect>();

    // Shown devices: one per distribution panel (its ports are merged)
    private readonly List<EnergyDevice> shown = new List<EnergyDevice>();

    private EnergyDevice here;
    private EnergyDevice selected;
    private EnergyDevice hovered;

    private Vector2 center;
    private float metersPerPixel = 0.5f;

    private Texture2D topography;
    private bool topographyDirty = true;
    private Vector2 topographyCenter;
    private float topographyScale;
    private Vector2 topographySize;

    private bool pointerDown;
    private bool dragging;
    private Vector2 pointerStart;
    private Vector2 centerStart;

    private readonly Label tooltip;

    public event Action<EnergyDevice> Selected;

    public NetworkMapElement()
    {
        AddToClassList("network-map");
        style.overflow = Overflow.Hidden;

        generateVisualContent += Draw;

        tooltip = new Label { pickingMode = PickingMode.Ignore };
        tooltip.AddToClassList("network-map-tooltip");
        tooltip.AddToClassList("hidden");
        Add(tooltip);

        RegisterCallback<WheelEvent>(OnWheel);
        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
        RegisterCallback<PointerLeaveEvent>(evt => SetHovered(null));
        RegisterCallback<GeometryChangedEvent>(evt => topographyDirty = true);
        RegisterCallback<DetachFromPanelEvent>(evt => DestroyTopography());
    }

    public EnergyDevice SelectedDevice => selected;

    // ---------- Content ----------

    public void SetNetwork(IEnumerable<EnergyDevice> networkDevices, IEnumerable<ElectricCable> networkCables, IEnumerable<Rect> buildingRects, EnergyDevice current)
    {
        devices.Clear();
        devices.AddRange(networkDevices);
        cables.Clear();
        cables.AddRange(networkCables);
        buildings.Clear();
        buildings.AddRange(buildingRects);
        here = current;

        shown.Clear();
        HashSet<DistributionPanel> panels = new HashSet<DistributionPanel>();

        foreach (EnergyDevice device in devices)
        {
            if (device == null) continue;
            if (device.Panel != null && !panels.Add(device.Panel)) continue;

            shown.Add(device);
        }

        if (selected != null && !devices.Contains(selected))
        {
            Select(null);
        }

        MarkDirtyRepaint();
    }

    public void Select(EnergyDevice device)
    {
        selected = device;
        Selected?.Invoke(device);
        MarkDirtyRepaint();
    }

    // Centers the view on the shown devices (and the current control panel)
    public void Fit()
    {
        Rect area = layout;
        bool any = false;
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;

        foreach (EnergyDevice device in shown)
        {
            Vector2 point = Flat(Position(device));

            if (!any)
            {
                min = max = point;
                any = true;
            }
            else
            {
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
        }

        if (!any) return;

        center = (min + max) * 0.5f;

        // At least 60 m shown, and a margin around the devices
        Vector2 extent = Vector2.Max(max - min, new Vector2(60f, 60f)) * 1.3f;
        float width = float.IsNaN(area.width) || area.width <= 0f ? 600f : area.width;
        float height = float.IsNaN(area.height) || area.height <= 0f ? 600f : area.height;

        metersPerPixel = Mathf.Clamp(Mathf.Max(extent.x / width, extent.y / height), MinMetersPerPixel, MaxMetersPerPixel);
        topographyDirty = true;
        MarkDirtyRepaint();
    }

    // Every frame while shown: the pulse of the current control panel, and the topography of a new view
    public void Tick()
    {
        if (topographyDirty)
        {
            UpdateTopography();
        }

        MarkDirtyRepaint();
    }

    // ---------- Coordinates ----------

    private static Vector2 Flat(Vector3 position) => new Vector2(position.x, position.z);

    private static Vector3 Position(EnergyDevice device)
    {
        return device.Panel != null ? device.Panel.transform.position : device.transform.position;
    }

    private Vector2 ToLocal(Vector2 world)
    {
        Rect rect = contentRect;
        return new Vector2(rect.width * 0.5f + (world.x - center.x) / metersPerPixel, rect.height * 0.5f - (world.y - center.y) / metersPerPixel);
    }

    private Vector2 ToWorld(Vector2 local)
    {
        Rect rect = contentRect;
        return new Vector2(center.x + (local.x - rect.width * 0.5f) * metersPerPixel, center.y - (local.y - rect.height * 0.5f) * metersPerPixel);
    }

    // ---------- Topography ----------

    private void UpdateTopography()
    {
        Rect rect = contentRect;

        if (float.IsNaN(rect.width) || rect.width <= 1f || rect.height <= 1f) return;

        topographyDirty = false;

        // Already generated for this view
        if (topography != null && topographyCenter == center && Mathf.Approximately(topographyScale, metersPerPixel) && topographySize == rect.size) return;

        topographyCenter = center;
        topographyScale = metersPerPixel;
        topographySize = rect.size;

        int width = TopographyResolution;
        int height = Mathf.Max(1, Mathf.RoundToInt(TopographyResolution * rect.height / rect.width));

        if (topography == null || topography.width != width || topography.height != height)
        {
            DestroyTopography();
            topography = new Texture2D(width, height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            style.backgroundImage = Background.FromTexture2D(topography);
        }

        ChunkManager chunks = ChunkManager.Instance;
        float[,] heights = new float[width + 1, height + 1];
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        // Texture row 0 is at the bottom: the south of the view
        Vector2 southWest = ToWorld(new Vector2(0f, rect.height));
        float stepX = rect.width * metersPerPixel / width;
        float stepY = rect.height * metersPerPixel / height;

        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                float value = chunks != null ? chunks.GeneratedHeightAt(southWest.x + x * stepX, southWest.y + y * stepY) : 0f;
                heights[x, y] = value;
                minHeight = Mathf.Min(minHeight, value);
                maxHeight = Mathf.Max(maxHeight, value);
            }
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
                Color color = Color.Lerp(BaseColor, HighColor, (value - minHeight) / range);
                Vector3 normal = new Vector3(value - right, stepX, value - up).normalized;
                float shade = Mathf.Clamp01(Vector3.Dot(normal, light));
                color *= 0.75f + shade * 0.5f;

                // Contour line where the band changes, every fifth one brighter
                int band = Mathf.FloorToInt(value / ContourStep);
                int bandRight = Mathf.FloorToInt(right / ContourStep);
                int bandUp = Mathf.FloorToInt(up / ContourStep);

                if (band != bandRight || band != bandUp)
                {
                    int line = Mathf.Max(band, Mathf.Max(bandRight, bandUp));
                    color = Color.Lerp(color, ContourColor, line % 5 == 0 ? 0.55f : 0.25f);
                }

                color.a = 1f;
                pixels[y * width + x] = color;
            }
        }

        topography.SetPixels(pixels);
        topography.Apply();
    }

    private void DestroyTopography()
    {
        if (topography != null)
        {
            UnityEngine.Object.Destroy(topography);
            topography = null;
        }
    }

    // ---------- Drawing ----------

    private void Draw(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;
        Rect rect = contentRect;

        // Topography of another view (being moved): drawn again next frame
        if (topography != null && (topographyCenter != center || !Mathf.Approximately(topographyScale, metersPerPixel)))
        {
            topographyDirty = true;
        }

        DrawGrid(painter, rect);

        foreach (Rect building in buildings)
        {
            DrawBuilding(painter, building);
        }

        foreach (ElectricCable cable in cables)
        {
            if (cable != null) DrawCable(painter, cable);
        }

        foreach (EnergyDevice device in shown)
        {
            if (device != null) DrawDevice(painter, device);
        }
    }

    private void DrawGrid(Painter2D painter, Rect rect)
    {
        // About 60 px between two lines
        float[] steps = { 5f, 10f, 25f, 50f, 100f, 250f, 500f, 1000f };
        float step = steps[steps.Length - 1];

        foreach (float candidate in steps)
        {
            if (candidate / metersPerPixel >= 60f)
            {
                step = candidate;
                break;
            }
        }

        Vector2 min = ToWorld(new Vector2(0f, rect.height));
        Vector2 max = ToWorld(new Vector2(rect.width, 0f));

        painter.strokeColor = GridColor;
        painter.lineWidth = 1f;
        painter.BeginPath();

        for (float x = Mathf.Ceil(min.x / step) * step; x <= max.x; x += step)
        {
            float localX = ToLocal(new Vector2(x, 0f)).x;
            painter.MoveTo(new Vector2(localX, 0f));
            painter.LineTo(new Vector2(localX, rect.height));
        }

        for (float y = Mathf.Ceil(min.y / step) * step; y <= max.y; y += step)
        {
            float localY = ToLocal(new Vector2(0f, y)).y;
            painter.MoveTo(new Vector2(0f, localY));
            painter.LineTo(new Vector2(rect.width, localY));
        }

        painter.Stroke();
    }

    private void DrawBuilding(Painter2D painter, Rect building)
    {
        Vector2 a = ToLocal(new Vector2(building.xMin, building.yMax));
        Vector2 b = ToLocal(new Vector2(building.xMax, building.yMin));

        painter.fillColor = new Color(BuildingColor.r, BuildingColor.g, BuildingColor.b, 0.12f);
        painter.strokeColor = new Color(BuildingColor.r, BuildingColor.g, BuildingColor.b, 0.8f);
        painter.lineWidth = 1.5f;
        painter.BeginPath();
        painter.MoveTo(a);
        painter.LineTo(new Vector2(b.x, a.y));
        painter.LineTo(b);
        painter.LineTo(new Vector2(a.x, b.y));
        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }

    private void DrawCable(Painter2D painter, ElectricCable cable)
    {
        List<Vector3> points = cable.ControlPoints();

        if (points.Count < 2) return;

        Color color = CableColor(cable);

        // Glow, then the line
        for (int layer = 0; layer < 2; layer++)
        {
            painter.strokeColor = layer == 0 ? new Color(color.r, color.g, color.b, 0.25f) : color;
            painter.lineWidth = layer == 0 ? 6f : 2f;
            painter.lineJoin = LineJoin.Round;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            painter.MoveTo(ToLocal(Flat(points[0])));

            for (int i = 1; i < points.Count; i++)
            {
                painter.LineTo(ToLocal(Flat(points[i])));
            }

            painter.Stroke();
        }
    }

    // State of the network of the cable: that of a consumer end (both are in the same network), one switched on if any
    private static Color CableColor(ElectricCable cable)
    {
        EnergyConsumer consumer = cable.Start as EnergyConsumer;
        EnergyConsumer other = cable.End as EnergyConsumer;

        if (consumer == null || (other != null && !consumer.IsOn))
        {
            consumer = other;
        }

        // Between a generator and a storage
        if (consumer == null) return new Color(PoweredColor.r, PoweredColor.g, PoweredColor.b, 0.5f);

        switch (consumer.State)
        {
            case EnergyNetworkManager.NetworkState.Tripped: return TrippedColor;
            case EnergyNetworkManager.NetworkState.Powered: return PoweredColor;
            default: return InactiveColor;
        }
    }

    private void DrawDevice(Painter2D painter, EnergyDevice device)
    {
        Vector2 point = ToLocal(Flat(Position(device)));
        Color color = DeviceColor(device);
        float size = 7f;

        painter.fillColor = color;
        painter.strokeColor = new Color(0f, 0f, 0f, 0.8f);
        painter.lineWidth = 1.5f;
        painter.BeginPath();

        if (device.Panel != null)
        {
            painter.MoveTo(point + new Vector2(0f, -size - 2f));
            painter.LineTo(point + new Vector2(size + 2f, 0f));
            painter.LineTo(point + new Vector2(0f, size + 2f));
            painter.LineTo(point + new Vector2(-size - 2f, 0f));
            painter.ClosePath();
        }
        else if (device.GetComponent<ControlPanel>() != null)
        {
            painter.MoveTo(point + new Vector2(-size, -size));
            painter.LineTo(point + new Vector2(size, -size));
            painter.LineTo(point + new Vector2(size, size));
            painter.LineTo(point + new Vector2(-size, size));
            painter.ClosePath();
        }
        else if (device is EnergyGenerator)
        {
            painter.MoveTo(point + new Vector2(0f, -size - 2f));
            painter.LineTo(point + new Vector2(size + 1f, size));
            painter.LineTo(point + new Vector2(-size - 1f, size));
            painter.ClosePath();
        }
        else if (device is EnergyStorage)
        {
            painter.MoveTo(point + new Vector2(-size * 0.6f, -size - 1f));
            painter.LineTo(point + new Vector2(size * 0.6f, -size - 1f));
            painter.LineTo(point + new Vector2(size * 0.6f, size + 1f));
            painter.LineTo(point + new Vector2(-size * 0.6f, size + 1f));
            painter.ClosePath();
        }
        else
        {
            painter.Arc(point, size, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.ClosePath();
        }

        painter.Fill();
        painter.Stroke();

        // Charge of a storage: a dark part from the top
        if (device is EnergyStorage storage && !float.IsInfinity(storage.Capacity) && storage.Capacity > 0f)
        {
            float empty = 1f - Mathf.Clamp01(storage.Stored / storage.Capacity);
            float top = point.y - size - 1f;
            float bottom = point.y + size + 1f;

            painter.fillColor = new Color(0f, 0f, 0f, 0.6f);
            painter.BeginPath();
            painter.MoveTo(new Vector2(point.x - size * 0.6f + 1f, top + 1f));
            painter.LineTo(new Vector2(point.x + size * 0.6f - 1f, top + 1f));
            painter.LineTo(new Vector2(point.x + size * 0.6f - 1f, Mathf.Lerp(top + 1f, bottom - 1f, empty)));
            painter.LineTo(new Vector2(point.x - size * 0.6f + 1f, Mathf.Lerp(top + 1f, bottom - 1f, empty)));
            painter.ClosePath();
            painter.Fill();
        }

        // Control panel in use: a pulsing ring
        if (device == here)
        {
            float pulse = Mathf.Repeat(Time.unscaledTime, 1.5f) / 1.5f;

            painter.strokeColor = new Color(HereColor.r, HereColor.g, HereColor.b, 1f - pulse);
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.Arc(point, size + 4f + pulse * 14f, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Stroke();
        }

        if (IsSame(device, selected) || IsSame(device, hovered))
        {
            painter.strokeColor = IsSame(device, selected) ? SelectedColor : new Color(1f, 1f, 1f, 0.5f);
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.Arc(point, size + 5f, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Stroke();
        }
    }

    // The ports of a distribution panel are the same icon
    private static bool IsSame(EnergyDevice a, EnergyDevice b)
    {
        if (a == null || b == null) return false;
        return a == b || (a.Panel != null && a.Panel == b.Panel);
    }

    private static Color DeviceColor(EnergyDevice device)
    {
        if (device.Panel != null)
        {
            DistributionPanel panel = device.Panel;
            if (panel.State == DistributionPanel.PanelState.On) return PanelColor;
            if (panel.State == DistributionPanel.PanelState.Starting) return Color.Lerp(PanelColor, InactiveColor, 0.5f);
            return panel.State == DistributionPanel.PanelState.Off ? InactiveColor : TrippedColor;
        }

        if (!device.IsOn) return InactiveColor;

        if (device is EnergyConsumer consumer)
        {
            switch (consumer.State)
            {
                case EnergyNetworkManager.NetworkState.Powered: return PoweredColor;
                case EnergyNetworkManager.NetworkState.Tripped: return TrippedColor;
                default: return InactiveColor;
            }
        }

        if (device is EnergyGenerator generator)
        {
            return generator.Output > 0.01f ? GeneratorColor : Color.Lerp(GeneratorColor, InactiveColor, 0.6f);
        }

        return StorageColor;
    }

    // ---------- Input ----------

    private EnergyDevice Pick(Vector2 local)
    {
        EnergyDevice best = null;
        float bestDistance = PickRadius;

        foreach (EnergyDevice device in shown)
        {
            if (device == null) continue;

            float distance = Vector2.Distance(local, ToLocal(Flat(Position(device))));

            if (distance <= bestDistance)
            {
                best = device;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void OnWheel(WheelEvent evt)
    {
        Vector2 local = evt.localMousePosition;
        Vector2 before = ToWorld(local);

        metersPerPixel = Mathf.Clamp(metersPerPixel * (evt.delta.y > 0f ? 1.2f : 1f / 1.2f), MinMetersPerPixel, MaxMetersPerPixel);

        // The point under the mouse stays under it
        Vector2 after = ToWorld(local);
        center += before - after;

        topographyDirty = true;
        evt.StopPropagation();
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0) return;

        pointerDown = true;
        dragging = false;
        pointerStart = evt.localPosition;
        centerStart = center;
        this.CapturePointer(evt.pointerId);
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        Vector2 local = evt.localPosition;

        if (pointerDown)
        {
            if (!dragging && Vector2.Distance(local, pointerStart) > DragThreshold)
            {
                dragging = true;
                SetHovered(null);
            }

            if (dragging)
            {
                Vector2 delta = local - pointerStart;
                center = centerStart + new Vector2(-delta.x, delta.y) * metersPerPixel;
                topographyDirty = true;
            }

            return;
        }

        SetHovered(Pick(local));

        if (hovered != null)
        {
            tooltip.text = hovered.Panel != null ? hovered.Panel.DisplayName : hovered.DisplayName;
            tooltip.style.left = local.x + 12f;
            tooltip.style.top = local.y - 8f;
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!pointerDown || evt.button != 0) return;

        pointerDown = false;
        this.ReleasePointer(evt.pointerId);

        if (!dragging)
        {
            Select(Pick(evt.localPosition));
        }

        dragging = false;
    }

    private void SetHovered(EnergyDevice device)
    {
        hovered = device;
        tooltip.EnableInClassList("hidden", device == null);
    }
}
