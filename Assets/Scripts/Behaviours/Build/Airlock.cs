using System;
using System.Collections.Generic;
using UnityEngine;

// Airlock (AirlockData, prefab Prefabs/Build/Airlock, model Resources/Models/Airlock), added by BuildUtility on its plan
// and on the finished structure, like Corridor. It is attached to a dome (found again by its save id): its origin is on
// the flange of the connector of the dome, +Z outward (DomeConnection).
// Finished: mesh colliders on its model, and the connector of the dome (it opens the dome wall). Nothing can be placed
// inside (Builder). Destroyed and refunded with its dome.
// Doors: opened and closed by interacting with their leaves (AirlockDoor, added on the Door_Inner_* and Door_Outer_*
// children), through the sequences of AirlockAnimator: opening a door first closes the other one and cycles the chamber
public class Airlock : MonoBehaviour, ISaveableComponent
{
    private static readonly List<Airlock> all = new List<Airlock>();
    public static IReadOnlyList<Airlock> All => all;

    private BuiltStructure dome;
    private string domeId;
    private DomeConnector connector;

    // Plan: the connector shown as a plan too, until the airlock is built
    private ConnectorPreview blueprintConnector;
    private Bounds localBounds;

    private AirlockAnimator doors;

    public bool IsBuilt => GetComponent<BuiltStructure>() != null;

    public void Setup(BuiltStructure newDome)
    {
        dome = newDome;
        domeId = DomeConnection.IdOf(newDome);
    }

    public void CopyTo(Airlock other)
    {
        other.dome = dome;
        other.domeId = domeId;
    }

    public bool Links(BuiltStructure structure) => structure != null && dome == structure;

    // The point is inside a finished airlock (bounds of its model)
    public static bool AnyContains(Vector3 point)
    {
        foreach (Airlock airlock in all)
        {
            if (airlock.IsBuilt && airlock.localBounds.Contains(airlock.transform.InverseTransformPoint(point))) return true;
        }

        return false;
    }

    private void Awake()
    {
        if (!IsBuilt) return;

        BuildUtility.AddMeshColliders(gameObject);

        // No terrain drawn in the chamber
        TerrainMask.SetBox(gameObject, 1.2f, 0.1f, 2.85f, 0f, 3.62f);

        doors = GetComponentInChildren<AirlockAnimator>();

        foreach (Transform part in GetComponentsInChildren<Transform>(true))
        {
            if (part.name.StartsWith("Door_Inner") || part.name.StartsWith("Door_Outer"))
            {
                AirlockDoor door = part.gameObject.AddComponent<AirlockDoor>();
                door.Airlock = this;
                door.Outer = part.name.StartsWith("Door_Outer");
            }
        }

        localBounds = BuildUtility.LocalRendererBounds(gameObject, transform);
    }

    private void OnEnable()
    {
        if (!all.Contains(this)) all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }

    private void OnDestroy()
    {
        blueprintConnector?.Destroy();

        if (!gameObject.scene.isLoaded) return;

        DomeConnection.Remove(connector);
    }

    private void Update()
    {
        // Loaded from the save: the dome registers after or before the airlock
        if (dome == null && !string.IsNullOrEmpty(domeId)) dome = DomeConnection.FindStructure(domeId);

        if (connector == null && IsBuilt && dome != null)
        {
            connector = DomeConnection.Create(DomeConnection.DomeOf(dome), transform.position - dome.transform.position, this);
        }

        if (blueprintConnector == null && !IsBuilt && dome != null)
        {
            blueprintConnector = new ConnectorPreview();
            blueprintConnector.Show(DomeConnection.DomeOf(dome), transform.position - dome.transform.position, BuildUtility.BlueprintMaterial);
        }
    }

    // ---------- Doors ----------

    public bool DoorsBusy => doors == null || doors.IsBusy;

    public bool IsDoorOpen(bool outer) => doors != null && (outer ? doors.OuterOpen : doors.InnerOpen);

    // Opens (after closing the other door and cycling the chamber) or closes the door. Ignored during a sequence
    public void UseDoor(bool outer)
    {
        if (DoorsBusy) return;

        if (outer)
        {
            if (doors.OuterOpen) doors.CloseOuter();
            else doors.RequestEnterFromOutside();
        }
        else
        {
            if (doors.InnerOpen) doors.CloseInner();
            else doors.RequestEnterFromInside();
        }
    }

    // Destroyed with its dome: gives back its resources (plan: what was brought)
    public void Deconstruct() => BuildUtility.DeconstructStructure(gameObject);

    // ---------- Save ----------

    [Serializable]
    private class State
    {
        public string domeId;
    }

    public string SaveKey => "airlock";

    public string CaptureState() => JsonUtility.ToJson(new State { domeId = domeId });

    public void RestoreState(string json)
    {
        State state = JsonUtility.FromJson<State>(json);

        if (state == null) return;

        domeId = state.domeId;
        dome = DomeConnection.FindStructure(domeId);
    }
}
