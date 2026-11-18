using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Map of an electrical network, in the control panel (ControlPanelUI), north at the top. Stylized:
// - the topography of the terrain as contour lines and shading (generated heights, ChunkManager.GeneratedHeightAt),
//   regenerated for the area shown, and a grid
// - the buildings (outline of their shape seen from above, highlighted under the mouse: a click selects their built-in
//   panel), the cables (glowing lines colored by the state of their network) and an icon per device: square for a
//   machine, circle for a generator, flat rectangle for a storage, triangle for a control panel, diamond for a
//   distribution panel (none for the panel built into a building). Colors: blue running, red tripped, grey off. The
//   control panel in use pulses
// Wheel to zoom, drag to move, click on a device or a building to select it
public class NetworkMapElement : VisualElement
{
    private const int TopographyResolution = 160;
    private const float PickRadius = 14f;
    private const float DragThreshold = 4f;
    private const float MinMetersPerPixel = 0.05f;
    private const float MaxMetersPerPixel = 20f;

    private static readonly Color PoweredColor = new Color(0.25f, 0.95f, 1f);
    private static readonly Color TrippedColor = new Color(1f, 0.3f, 0.3f);
    private static readonly Color InactiveColor = new Color(0.45f, 0.5f, 0.55f);
    private static readonly Color HereColor = new Color(1f, 1f, 1f);
    private static readonly Color SelectedColor = new Color(1f, 1f, 1f);

    private readonly List<EnergyDevice> devices = new List<EnergyDevice>();
    private readonly List<ElectricCable> cables = new List<ElectricCable>();
    private readonly List<Vector2[]> buildings = new List<Vector2[]>();
    private readonly List<BuiltStructure> buildingOwners = new List<BuiltStructure>();
    private int hoveredBuilding = -1;

    // Shown devices: one per distribution panel (its ports are merged)
    private readonly List<EnergyDevice> shown = new List<EnergyDevice>();

    private EnergyDevice here;
    private EnergyDevice selected;
    private EnergyDevice hovered;

    private Vector2 center;
    private float metersPerPixel = 0.5f;

    private Texture2D topography;
    private readonly List<MapUtility.ContourSegment> contours = new List<MapUtility.ContourSegment>();
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
        RegisterCallback<PointerLeaveEvent>(evt => { SetHovered(null); hoveredBuilding = -1; MarkDirtyRepaint(); });
        RegisterCallback<GeometryChangedEvent>(evt => topographyDirty = true);
        RegisterCallback<DetachFromPanelEvent>(evt => DestroyTopography());
    }

    public EnergyDevice SelectedDevice => selected;

    // ---------- Content ----------

    // owners: the structure of each outline (same index)
    public void SetNetwork(IEnumerable<EnergyDevice> networkDevices, IEnumerable<ElectricCable> networkCables, IEnumerable<Vector2[]> buildingOutlines,
        IEnumerable<BuiltStructure> owners, EnergyDevice current)
    {
        devices.Clear();
        devices.AddRange(networkDevices);
        cables.Clear();
        cables.AddRange(networkCables);
        buildings.Clear();
        buildings.AddRange(buildingOutlines);
        buildingOwners.Clear();
        buildingOwners.AddRange(owners);
        if (hoveredBuilding >= buildings.Count) hoveredBuilding = -1;
        here = current;

        shown.Clear();
        HashSet<DistributionPanel> panels = new HashSet<DistributionPanel>();

        foreach (EnergyDevice device in devices)
        {
            if (device == null) continue;
            // The panel built into a building has no icon: the building itself is selected
            if (device.Panel != null && device.Panel.Building != null) continue;
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

        // Texture row 0 is at the bottom: the south of the view
        Vector2 southWest = ToWorld(new Vector2(0f, rect.height));
        MapUtility.FillTopography(topography, southWest, rect.width * metersPerPixel / width, rect.height * metersPerPixel / height, contours);
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

        MapUtility.DrawContours(painter, contours, ToLocal);
        DrawGrid(painter, rect);

        for (int i = 0; i < buildings.Count; i++)
        {
            DrawBuilding(painter, i);
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

        painter.strokeColor = MapUtility.GridColor;
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

    // Outline of the building seen from above (world X, Z). Highlighted under the mouse, and when its built-in panel
    // is selected
    private void DrawBuilding(Painter2D painter, int index)
    {
        Vector2[] building = buildings[index];
        bool highlighted = index == hoveredBuilding || (selected != null && selected.Panel != null && selected.Panel.Building != null && OwnerOf(index) == selected.Panel.Building.Structure);

        if (!highlighted)
        {
            MapUtility.DrawOutline(painter, building, ToLocal, MapUtility.BuildingColor);
            return;
        }

        painter.fillColor = new Color(MapUtility.BuildingColor.r, MapUtility.BuildingColor.g, MapUtility.BuildingColor.b, 0.35f);
        painter.strokeColor = Color.white;
        painter.lineWidth = 2.5f;
        painter.BeginPath();
        painter.MoveTo(ToLocal(building[0]));

        for (int i = 1; i < building.Length; i++)
        {
            painter.LineTo(ToLocal(building[i]));
        }

        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }

    private BuiltStructure OwnerOf(int index)
    {
        return index >= 0 && index < buildingOwners.Count ? buildingOwners[index] : null;
    }

    // A port of the built-in panel of the building, if it is in the network shown (to select it)
    private EnergyDevice PanelPortOf(BuiltStructure structure)
    {
        if (structure == null) return null;

        foreach (EnergyDevice device in devices)
        {
            if (device != null && device.Panel is BuildingPowerPanel building && building.Structure == structure) return device;
        }

        return null;
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

    // State of the network of the cable (EnergyNetworkManager.CableState)
    private static Color CableColor(ElectricCable cable)
    {
        if (!EnergyNetworkManager.HasInstance) return InactiveColor;

        switch (EnergyNetworkManager.Instance.CableState(cable))
        {
            case EnergyNetworkManager.NetworkState.Tripped: return TrippedColor;
            case EnergyNetworkManager.NetworkState.Powered: return PoweredColor;
            // Network without consumer (generators and storages only)
            case EnergyNetworkManager.NetworkState.NotLinked: return new Color(PoweredColor.r, PoweredColor.g, PoweredColor.b, 0.5f);
            default: return InactiveColor;
        }
    }

    // Shapes: square for a machine, circle for a generator, flat rectangle for a storage, triangle for a control panel,
    // diamond for a distribution panel. Colors: the state only (DeviceColor)
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
            painter.MoveTo(point + new Vector2(0f, -size - 2f));
            painter.LineTo(point + new Vector2(size + 1f, size));
            painter.LineTo(point + new Vector2(-size - 1f, size));
            painter.ClosePath();
        }
        else if (device is EnergyGenerator)
        {
            painter.Arc(point, size, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.ClosePath();
        }
        else if (device is EnergyStorage)
        {
            Rectangle(painter, point, size * 1.4f, size * 0.7f);
        }
        else
        {
            Rectangle(painter, point, size, size);
        }

        painter.Fill();
        painter.Stroke();

        // Charge of a storage: a dark part from the right
        if (device is EnergyStorage storage && !float.IsInfinity(storage.Capacity) && storage.Capacity > 0f)
        {
            float empty = 1f - Mathf.Clamp01(storage.Stored / storage.Capacity);
            float left = point.x - size * 1.4f + 1f;
            float right = point.x + size * 1.4f - 1f;

            painter.fillColor = new Color(0f, 0f, 0f, 0.6f);
            painter.BeginPath();
            painter.MoveTo(new Vector2(Mathf.Lerp(right, left, empty), point.y - size * 0.7f + 1f));
            painter.LineTo(new Vector2(right, point.y - size * 0.7f + 1f));
            painter.LineTo(new Vector2(right, point.y + size * 0.7f - 1f));
            painter.LineTo(new Vector2(Mathf.Lerp(right, left, empty), point.y + size * 0.7f - 1f));
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

    private static void Rectangle(Painter2D painter, Vector2 center, float halfWidth, float halfHeight)
    {
        painter.MoveTo(center + new Vector2(-halfWidth, -halfHeight));
        painter.LineTo(center + new Vector2(halfWidth, -halfHeight));
        painter.LineTo(center + new Vector2(halfWidth, halfHeight));
        painter.LineTo(center + new Vector2(-halfWidth, halfHeight));
        painter.ClosePath();
    }

    // Three colors only: blue running, red tripped (or a panel that blew, is shed, has no input, loops), grey off or not
    // linked (or a panel starting)
    private static Color DeviceColor(EnergyDevice device)
    {
        if (device.Panel != null)
        {
            switch (device.Panel.State)
            {
                case DistributionPanel.PanelState.On: return PoweredColor;
                case DistributionPanel.PanelState.Off:
                case DistributionPanel.PanelState.Starting: return InactiveColor;
                default: return TrippedColor;
            }
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

        // Generators and storages: their network
        if (!EnergyNetworkManager.HasInstance || !EnergyNetworkManager.Instance.IsLinked(device)) return InactiveColor;

        return EnergyNetworkManager.Instance.IsTripped(device) ? TrippedColor : PoweredColor;
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

        // No device under the mouse: the building under it is highlighted
        int building = hovered == null ? PickBuilding(local) : -1;

        if (building != hoveredBuilding)
        {
            hoveredBuilding = building;
            MarkDirtyRepaint();
        }

        string text = hovered != null ? (hovered.Panel != null ? hovered.Panel.DisplayName : hovered.DisplayName)
            : OwnerOf(hoveredBuilding) != null && OwnerOf(hoveredBuilding).Data != null ? OwnerOf(hoveredBuilding).Data.Name : null;

        tooltip.EnableInClassList("hidden", text == null);

        if (text != null)
        {
            tooltip.text = text;
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
            // A device, or else a building: its built-in panel (if it is in the network shown)
            EnergyDevice device = Pick(evt.localPosition);

            if (device == null)
            {
                device = PanelPortOf(OwnerOf(PickBuilding(evt.localPosition)));
            }

            Select(device);
        }

        dragging = false;
    }

    // Building under the point of the element, -1 if none. The last drawn (on top) first
    private int PickBuilding(Vector2 local)
    {
        Vector2 world = ToWorld(local);

        for (int i = buildings.Count - 1; i >= 0; i--)
        {
            if (MapUtility.Contains(buildings[i], world)) return i;
        }

        return -1;
    }

    private void SetHovered(EnergyDevice device)
    {
        hovered = device;
        tooltip.EnableInClassList("hidden", device == null);
    }
}
