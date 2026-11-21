using System;
using System.Collections.Generic;
using UnityEngine;

// Corridor between two buildings (CorridorData), added by BuildUtility on its plan (StructureBlueprint) and on the
// finished structure (BuiltStructure). It keeps its two buildings (found again by their save id, like the cables) and
// its length (pieces of its model, CorridorPieces, and cost multiplier: cost and refund are per meter).
// Electricity (EnergyNetworkManager): a finished corridor links the inputs of the panels of its two buildings without
// cable, both ways, like a cable from input to input. Each building keeps its own panel: switching one off does not
// switch off the other. What is placed in the corridor itself is not linked to anything (it must be cabled).
// Domes: it goes from the flange of the connector of a dome to the flange of the other (DomeConnection). Finished, it
// creates the two connectors (they open the walls) and removes them with itself.
// Destroyed with one of its buildings (refunded)
public class Corridor : MonoBehaviour, ISaveableComponent
{
    private static readonly List<Corridor> all = new List<Corridor>();
    public static IReadOnlyList<Corridor> All => all;

    private BuiltStructure start;
    private BuiltStructure end;
    private string startId;
    private string endId;
    private float length = 1f;
    private float slope;
    private DomeConnector startConnector;
    private DomeConnector endConnector;

    // Bounds of its pieces (local space), to tell what is inside
    private Bounds pieceBounds;

    // Plan: the two connectors shown as a plan too, until the corridor is built
    private ConnectorPreview startBlueprintConnector;
    private ConnectorPreview endBlueprintConnector;

    public BuiltStructure Start => start;
    public BuiltStructure End => end;
    public float Length => length;

    // Finished (not a plan)
    public bool IsBuilt => GetComponent<BuiltStructure>() != null;

    // Cost and refund per meter
    public int CostMultiplier => Mathf.Max(1, Mathf.CeilToInt(length));

    // ---------- Placement ----------

    // Straight corridor between the flanges of the connectors of two domes, toward each other: position (center of its
    // floor), rotation (level, Z from a to b), horizontal length and slope (height change per meter, from a to b)
    public static bool ComputePlacement(BuiltStructure a, BuiltStructure b, out Vector3 position, out Quaternion rotation, out float corridorLength, out float corridorSlope)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        corridorLength = 0f;
        corridorSlope = 0f;

        Dome domeA = DomeConnection.DomeOf(a);
        Dome domeB = DomeConnection.DomeOf(b);

        if (domeA == null || domeB == null || a == b) return false;

        Vector3 centerA = a.transform.position;
        Vector3 centerB = b.transform.position;
        Vector3 direction = centerB - centerA;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return false;

        direction.Normalize();

        Vector3 edgeA = centerA + direction * DomeConnection.FlangeRadius(domeA);
        Vector3 edgeB = centerB - direction * DomeConnection.FlangeRadius(domeB);
        Vector3 along = edgeB - edgeA;

        // The buildings overlap: no room for a corridor
        if (Vector3.Dot(along, direction) <= 0f) return false;

        Vector3 horizontal = new Vector3(along.x, 0f, along.z);

        position = (edgeA + edgeB) * 0.5f;
        rotation = Quaternion.LookRotation(horizontal, Vector3.up);
        corridorLength = horizontal.magnitude;
        corridorSlope = along.y / corridorLength;
        return true;
    }

    public static bool AreLinked(BuiltStructure a, BuiltStructure b)
    {
        foreach (Corridor corridor in all)
        {
            if (corridor.Links(a) && corridor.Links(b)) return true;
        }

        return false;
    }

    // The buildings linked to this one by finished corridors, directly or through others (itself included)
    public static HashSet<BuiltStructure> Group(BuiltStructure structure)
    {
        HashSet<BuiltStructure> group = new HashSet<BuiltStructure>();
        Queue<BuiltStructure> queue = new Queue<BuiltStructure>();

        if (structure == null) return group;

        group.Add(structure);
        queue.Enqueue(structure);

        while (queue.Count > 0)
        {
            BuiltStructure current = queue.Dequeue();

            foreach (Corridor corridor in all)
            {
                if (!corridor.IsBuilt || !corridor.Links(current)) continue;

                BuiltStructure other = corridor.start == current ? corridor.end : corridor.start;

                if (other != null && group.Add(other)) queue.Enqueue(other);
            }
        }

        return group;
    }

    // The point is inside a finished corridor (bounds of its pieces)
    public static bool AnyContains(Vector3 point)
    {
        foreach (Corridor corridor in all)
        {
            if (corridor.IsBuilt && corridor.pieceBounds.Contains(corridor.transform.InverseTransformPoint(point))) return true;
        }

        return false;
    }

    public bool Links(BuiltStructure structure)
    {
        return structure != null && (start == structure || end == structure);
    }

    public void Setup(BuiltStructure newStart, BuiltStructure newEnd, float newLength, float newSlope)
    {
        slope = newSlope;
        start = newStart;
        end = newEnd;
        startId = DomeConnection.IdOf(newStart);
        endId = DomeConnection.IdOf(newEnd);
        SetLength(newLength);
        MarkNetworksDirty();
    }

    // The plan becomes the finished corridor
    public void CopyTo(Corridor other)
    {
        other.start = start;
        other.end = end;
        other.startId = startId;
        other.endId = endId;
        other.slope = slope;
        other.SetLength(length);
        MarkNetworksDirty();
    }

    private void SetLength(float newLength)
    {
        length = Mathf.Max(0.1f, newLength);

        BuiltStructure built = GetComponent<BuiltStructure>();
        StructureBlueprint blueprint = GetComponent<StructureBlueprint>();

        if (built != null) built.CostMultiplier = CostMultiplier;
        if (blueprint != null) blueprint.CostMultiplier = CostMultiplier;

        // Model: the pieces for this length. Finished: models with colliders. Plan: blueprint material, and its collider
        // (to be aimed at) around the pieces
        CorridorData data = (built != null ? built.Data : blueprint != null ? blueprint.Data : null) as CorridorData;

        if (data == null) return;

        Bounds bounds = CorridorPieces.Build(transform, data, length, slope, built != null ? null : BuildUtility.BlueprintMaterial, built != null);
        pieceBounds = bounds;

        // Finished: no terrain drawn inside (tilted with the corridor)
        if (built != null) TerrainMask.SetBox(gameObject, 0.9f, 0.1f, 2.6f, -length * 0.5f, length * 0.5f, slope);
        BoxCollider box = built == null ? GetComponent<BoxCollider>() : null;

        if (box != null)
        {
            box.center = bounds.center;
            box.size = bounds.size;
        }
    }

    // Destroyed with one of its buildings: gives back its resources (plan: what was brought)
    public void Deconstruct() => BuildUtility.DeconstructStructure(gameObject);

    // ---------- Life ----------

    private void OnEnable()
    {
        if (!all.Contains(this)) all.Add(this);
        MarkNetworksDirty();
    }

    private void OnDisable()
    {
        all.Remove(this);
        MarkNetworksDirty();
    }

    private void OnDestroy()
    {
        startBlueprintConnector?.Destroy();
        endBlueprintConnector?.Destroy();

        if (!gameObject.scene.isLoaded) return;

        DomeConnection.Remove(startConnector);
        DomeConnection.Remove(endConnector);
    }

    private void Update()
    {
        // Loaded from the save: the buildings register after or before the corridor
        if ((start == null && !string.IsNullOrEmpty(startId)) || (end == null && !string.IsNullOrEmpty(endId)))
        {
            TryFindEnds();
        }

        // Plan, both domes known: the connectors shown as a plan
        if (startBlueprintConnector == null && !IsBuilt && start != null && end != null)
        {
            Vector3 between = end.transform.position - start.transform.position;
            startBlueprintConnector = new ConnectorPreview();
            endBlueprintConnector = new ConnectorPreview();
            startBlueprintConnector.Show(DomeConnection.DomeOf(start), between, BuildUtility.BlueprintMaterial);
            endBlueprintConnector.Show(DomeConnection.DomeOf(end), -between, BuildUtility.BlueprintMaterial);
        }

        // Finished, both domes known: the connectors on their walls, toward each other
        if (startConnector == null && endConnector == null && IsBuilt && start != null && end != null)
        {
            Vector3 direction = end.transform.position - start.transform.position;
            startConnector = DomeConnection.Create(DomeConnection.DomeOf(start), direction, this);
            endConnector = DomeConnection.Create(DomeConnection.DomeOf(end), -direction, this);
        }
    }

    private void TryFindEnds()
    {
        if (SaveManager.Instance == null) return;

        if (start == null) start = DomeConnection.FindStructure(startId);
        if (end == null) end = DomeConnection.FindStructure(endId);

        if (start != null && end != null)
        {
            MarkNetworksDirty();
        }
    }

    private static void MarkNetworksDirty()
    {
        if (EnergyNetworkManager.HasInstance)
        {
            EnergyNetworkManager.Instance.MarkDirty();
        }
    }

    // ---------- Save ----------

    [Serializable]
    private class State
    {
        public string startId;
        public string endId;
        public float length;
        public float slope;
    }

    public string SaveKey => "corridor";

    public string CaptureState()
    {
        return JsonUtility.ToJson(new State { startId = startId, endId = endId, length = length, slope = slope });
    }

    public void RestoreState(string json)
    {
        State state = JsonUtility.FromJson<State>(json);

        if (state == null) return;

        startId = state.startId;
        endId = state.endId;
        start = null;
        end = null;
        slope = state.slope;
        SetLength(state.length);
        TryFindEnds();
    }
}
