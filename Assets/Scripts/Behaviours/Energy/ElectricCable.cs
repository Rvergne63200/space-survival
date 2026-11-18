using System;
using System.Collections.Generic;
using UnityEngine;

// Electric cable between two devices (EnergyDevice), placed by the player (CablePlacer). It goes through its
// intermediate points with a rounded line (Catmull-Rom curve, slightly sagging between two points).
// Saved as a runtime object ("cable:<item id>"): its ends are found again by the id of their saved object.
// Destroyed from the build wheel (or with one of its devices), it gives back the cable items it used
public class ElectricCable : MonoBehaviour, ISaveableComponent
{
    private const int SamplesPerSegment = 10;
    private const float SagRatio = 0.04f;
    private const float MaxSag = 0.6f;
    private const float ColliderRadius = 0.12f;

    private CableItemData data;
    private EnergyDevice start;
    private EnergyDevice end;

    // Kept to draw the cable before its devices are found (load) and to find them again
    private string startId;
    private string endId;
    private string startPort;
    private string endPort;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private readonly List<Vector3> points = new List<Vector3>();
    private int itemCount;

    private LineRenderer line;
    private readonly List<GameObject> colliders = new List<GameObject>();

    public static ElectricCable Create(CableItemData data, EnergyDevice start, EnergyDevice end, List<Vector3> points, int itemCount)
    {
        ElectricCable cable = CreateEmpty(data, null);

        cable.points.AddRange(points);
        cable.itemCount = itemCount;
        cable.SetEnds(start, end);

        return cable;
    }

    // Recreated from the save: the state is restored when its SaveableObject registers
    public static ElectricCable CreateEmpty(CableItemData data, string id)
    {
        GameObject cableObject = new GameObject("Electric Cable");
        ElectricCable cable = cableObject.AddComponent<ElectricCable>();
        cable.data = data;

        cableObject.AddComponent<SaveableObject>().SetRuntime(SaveManager.CablePrefabKeyPrefix + (data != null ? data.Id : 0), id);

        return cable;
    }

    // Curve through the points, used for the preview of the CablePlacer too
    public static List<Vector3> BuildCurve(List<Vector3> controlPoints)
    {
        List<Vector3> curve = new List<Vector3>();

        if (controlPoints.Count < 2)
        {
            curve.AddRange(controlPoints);
            return curve;
        }

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 p0 = controlPoints[Mathf.Max(0, i - 1)];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = controlPoints[Mathf.Min(controlPoints.Count - 1, i + 2)];

            float sag = Mathf.Min(MaxSag, Vector3.Distance(p1, p2) * SagRatio);

            for (int s = 0; s < SamplesPerSegment; s++)
            {
                float t = s / (float)SamplesPerSegment;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                point.y -= sag * 4f * t * (1f - t);
                curve.Add(point);
            }
        }

        curve.Add(controlPoints[controlPoints.Count - 1]);
        return curve;
    }

    public static float Length(List<Vector3> curve)
    {
        float length = 0f;

        for (int i = 1; i < curve.Count; i++)
        {
            length += Vector3.Distance(curve[i - 1], curve[i]);
        }

        return length;
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (3f * p1 - p0 - 3f * p2 + p3) * t3);
    }

    public static LineRenderer CreateLine(GameObject owner, CableItemData data)
    {
        LineRenderer line = owner.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.widthMultiplier = data != null ? data.Width : 0.05f;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        if (data != null && data.Material != null)
        {
            line.sharedMaterial = data.Material;
        }

        return line;
    }

    public EnergyDevice Start => start;
    public EnergyDevice End => end;

    // Ends and intermediate points (map of the control panel)
    public List<Vector3> ControlPoints()
    {
        List<Vector3> controlPoints = new List<Vector3> { start != null ? start.ConnectionPoint : startPosition };
        controlPoints.AddRange(points);
        controlPoints.Add(end != null ? end.ConnectionPoint : endPosition);
        return controlPoints;
    }

    public EnergyDevice Other(EnergyDevice device)
    {
        if (device == start) return end;
        if (device == end) return start;
        return null;
    }

    private void SetEnds(EnergyDevice newStart, EnergyDevice newEnd)
    {
        start = newStart;
        end = newEnd;

        if (start != null)
        {
            startId = start.SaveId;
            startPort = start.PortId;
            startPosition = start.ConnectionPoint;
            start.AddCable(this);
        }

        if (end != null)
        {
            endId = end.SaveId;
            endPort = end.PortId;
            endPosition = end.ConnectionPoint;
            end.AddCable(this);
        }

        Redraw();
    }

    private void Update()
    {
        // Loaded from the save: the devices register after or before the cable
        if ((start == null && !string.IsNullOrEmpty(startId)) || (end == null && !string.IsNullOrEmpty(endId)))
        {
            TryFindEnds();
        }
    }

    private void TryFindEnds()
    {
        if (SaveManager.Instance == null) return;

        EnergyDevice foundStart = start != null ? start : FindDevice(startId, startPort);
        EnergyDevice foundEnd = end != null ? end : FindDevice(endId, endPort);

        if (foundStart == null || foundEnd == null) return;

        SetEnds(foundStart, foundEnd);
    }

    private static EnergyDevice FindDevice(string id, string port)
    {
        SaveableObject saveable = SaveManager.Instance.GetLiveObject(id);

        if (saveable == null) return null;

        foreach (EnergyDevice device in saveable.GetComponentsInChildren<EnergyDevice>())
        {
            if (string.IsNullOrEmpty(port) || device.PortId == port) return device;
        }

        return null;
    }

    private void Redraw()
    {
        if (line == null)
        {
            line = CreateLine(gameObject, data);
        }

        List<Vector3> curve = BuildCurve(ControlPoints());
        line.positionCount = curve.Count;
        line.SetPositions(curve.ToArray());

        BuildColliders(curve);
    }

    // Triggers along the cable: aimed by the destruction (Builder), not in the way of the player
    private void BuildColliders(List<Vector3> curve)
    {
        foreach (GameObject colliderObject in colliders)
        {
            Destroy(colliderObject);
        }

        colliders.Clear();

        for (int i = 1; i < curve.Count; i++)
        {
            Vector3 from = curve[i - 1];
            Vector3 to = curve[i];
            float length = Vector3.Distance(from, to);

            if (length < 0.01f) continue;

            GameObject colliderObject = new GameObject("Collider");
            colliderObject.transform.SetParent(transform, false);
            colliderObject.transform.SetPositionAndRotation((from + to) * 0.5f, Quaternion.FromToRotation(Vector3.up, to - from));

            CapsuleCollider capsule = colliderObject.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = ColliderRadius;
            capsule.height = length + ColliderRadius * 2f;

            colliders.Add(colliderObject);
        }
    }

    // Destruction: gives back its cable items
    public void Deconstruct()
    {
        if (data != null && itemCount > 0)
        {
            BuildUtility.GiveToPlayer(data, itemCount);
        }

        itemCount = 0;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (start != null) start.RemoveCable(this);
        if (end != null) end.RemoveCable(this);
    }

    // ---------- Save ----------

    [Serializable]
    private class State
    {
        public string startId;
        public string endId;
        public string startPort;
        public string endPort;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public List<Vector3> points = new List<Vector3>();
        public int itemCount;
    }

    public string SaveKey => "electricCable";

    public string CaptureState()
    {
        return JsonUtility.ToJson(new State
        {
            startId = startId,
            endId = endId,
            startPort = startPort,
            endPort = endPort,
            startPosition = start != null ? start.ConnectionPoint : startPosition,
            endPosition = end != null ? end.ConnectionPoint : endPosition,
            points = new List<Vector3>(points),
            itemCount = itemCount
        });
    }

    public void RestoreState(string json)
    {
        State state = JsonUtility.FromJson<State>(json);

        if (state == null) return;

        startId = state.startId;
        endId = state.endId;
        startPort = state.startPort;
        endPort = state.endPort;
        startPosition = state.startPosition;
        endPosition = state.endPosition;
        itemCount = state.itemCount;

        points.Clear();
        points.AddRange(state.points);

        Redraw();
        TryFindEnds();
    }
}
