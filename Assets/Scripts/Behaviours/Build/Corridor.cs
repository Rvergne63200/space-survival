using System;
using System.Collections.Generic;
using UnityEngine;

// Corridor between two buildings (CorridorData), added by BuildUtility on its plan (StructureBlueprint) and on the
// finished structure (BuiltStructure). It keeps its two buildings (found again by their save id, like the cables) and
// its length (scale of the object along Z, and cost multiplier: cost and refund are per meter).
// Electricity (EnergyNetworkManager): a finished corridor links the inputs of the panels of its two buildings without
// cable, both ways, like a cable from input to input. Each building keeps its own panel: switching one off does not
// switch off the other. What is placed in the corridor itself is not linked to anything (it must be cabled).
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

    public BuiltStructure Start => start;
    public BuiltStructure End => end;
    public float Length => length;

    // Finished (not a plan)
    public bool IsBuilt => GetComponent<BuiltStructure>() != null;

    // Cost and refund per meter
    public int CostMultiplier => Mathf.Max(1, Mathf.CeilToInt(length));

    // ---------- Placement ----------

    // A building that can hold a corridor: a finished structure with an inside (BuildingInterior)
    public static BuildingInterior InteriorOf(BuiltStructure structure)
    {
        return structure != null ? structure.GetComponentInChildren<BuildingInterior>() : null;
    }

    // Straight corridor between the edges of two buildings, at the height of their floor: position (center of its
    // floor), rotation (Z from a to b) and length
    public static bool ComputePlacement(BuiltStructure a, BuiltStructure b, out Vector3 position, out Quaternion rotation, out float corridorLength)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        corridorLength = 0f;

        BuildingInterior interiorA = InteriorOf(a);
        BuildingInterior interiorB = InteriorOf(b);

        if (interiorA == null || interiorB == null || a == b) return false;

        Vector3 centerA = a.transform.position;
        Vector3 centerB = b.transform.position;
        Vector3 direction = centerB - centerA;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return false;

        direction.Normalize();

        Vector3 edgeA = centerA + direction * interiorA.Radius;
        Vector3 edgeB = centerB - direction * interiorB.Radius;
        Vector3 along = edgeB - edgeA;

        // The buildings overlap: no room for a corridor
        if (Vector3.Dot(along, direction) <= 0f) return false;

        position = (edgeA + edgeB) * 0.5f;
        rotation = Quaternion.LookRotation(along, Vector3.up);
        corridorLength = along.magnitude;
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

    public bool Links(BuiltStructure structure)
    {
        return structure != null && (start == structure || end == structure);
    }

    public void Setup(BuiltStructure newStart, BuiltStructure newEnd, float newLength)
    {
        start = newStart;
        end = newEnd;
        startId = IdOf(newStart);
        endId = IdOf(newEnd);
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
        other.SetLength(length);
        MarkNetworksDirty();
    }

    private void SetLength(float newLength)
    {
        length = Mathf.Max(0.1f, newLength);

        Vector3 scale = transform.localScale;
        scale.z = length;
        transform.localScale = scale;

        BuiltStructure built = GetComponent<BuiltStructure>();
        if (built != null) built.CostMultiplier = CostMultiplier;

        StructureBlueprint blueprint = GetComponent<StructureBlueprint>();
        if (blueprint != null) blueprint.CostMultiplier = CostMultiplier;
    }

    private static string IdOf(BuiltStructure structure)
    {
        SaveableObject saveable = structure != null ? structure.GetComponent<SaveableObject>() : null;
        return saveable != null ? saveable.Id : null;
    }

    // Destroyed with one of its buildings: gives back its resources (plan: what was brought)
    public void Deconstruct()
    {
        StructureBlueprint blueprint = GetComponent<StructureBlueprint>();
        BuiltStructure built = GetComponent<BuiltStructure>();

        if (blueprint != null) blueprint.Deconstruct();
        else if (built != null) built.Deconstruct();
        else Destroy(gameObject);
    }

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

    private void Update()
    {
        // Loaded from the save: the buildings register after or before the corridor
        if ((start == null && !string.IsNullOrEmpty(startId)) || (end == null && !string.IsNullOrEmpty(endId)))
        {
            TryFindEnds();
        }
    }

    private void TryFindEnds()
    {
        if (SaveManager.Instance == null) return;

        if (start == null) start = FindStructure(startId);
        if (end == null) end = FindStructure(endId);

        if (start != null && end != null)
        {
            MarkNetworksDirty();
        }
    }

    private static BuiltStructure FindStructure(string id)
    {
        SaveableObject saveable = SaveManager.Instance.GetLiveObject(id);
        return saveable != null ? saveable.GetComponent<BuiltStructure>() : null;
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
    }

    public string SaveKey => "corridor";

    public string CaptureState()
    {
        return JsonUtility.ToJson(new State { startId = startId, endId = endId, length = length });
    }

    public void RestoreState(string json)
    {
        State state = JsonUtility.FromJson<State>(json);

        if (state == null) return;

        startId = state.startId;
        endId = state.endId;
        start = null;
        end = null;
        SetLength(state.length);
        TryFindEnds();
    }
}
