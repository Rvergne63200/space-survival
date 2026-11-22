using System.Collections.Generic;
using UnityEngine;

// Attachment of an airlock or a corridor to a dome, shared by their placement (Builder) and by the structures themselves
// (Airlock, Corridor). Anywhere on the rim: a connector is placed on the wall toward a horizontal direction (from the
// center of the dome), the airlock or the corridor starts at its flange (socket)
public static class DomeConnection
{
    // Min distance between two connectors of a dome (along the wall): their shells do not overlap
    public const float MinSpacing = 2.8f;

    // Domes finished in the world (Dome registers itself)
    private static readonly List<Dome> domes = new List<Dome>();
    public static void Register(Dome dome) { if (!domes.Contains(dome)) domes.Add(dome); }
    public static void Unregister(Dome dome) { domes.Remove(dome); }

    public static Dome DomeOf(BuiltStructure structure) => structure != null ? structure.GetComponent<Dome>() : null;

    public static Vector3 Horizontal(Vector3 direction)
    {
        direction.y = 0f;
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
    }

    // Pose of the connector on the wall, toward the direction
    public static void ConnectorPose(Dome dome, Vector3 direction, out Vector3 position, out Quaternion rotation)
    {
        direction = Horizontal(direction);
        position = dome.transform.position + direction * dome.Radius;
        rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    // Distance from the center of the dome to the flange of its connector
    public static float FlangeRadius(Dome dome) => dome.Radius + DomeConnector.FlangeDistance(dome.ConnectorPrefab);

    // Pose of the flange (start of the airlock, end of a corridor), +Z outward
    public static void FlangePose(Dome dome, Vector3 direction, out Vector3 position, out Quaternion rotation)
    {
        direction = Horizontal(direction);
        position = dome.transform.position + direction * FlangeRadius(dome);
        rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    // Min distance between a connector and the power socket of the dome (along the wall): the shell of the connector
    // (half of MinSpacing) does not cover the socket (half width 0.2 m), with a margin for the cables plugged in
    public const float PowerSocketSpacing = 1.8f;

    // No other connector nor the power socket too close on the wall (ignored: the connectors of "owner")
    public static bool IsFree(Dome dome, Vector3 direction, Component owner = null) => Problem(dome, direction, owner) == null;

    // Why a connector cannot be placed on the wall toward the direction ("... occupe déjà cet endroit du dôme"), null
    // if it can
    public static string Problem(Dome dome, Vector3 direction, Component owner = null)
    {
        Vector3 wallPoint = dome.transform.position + Horizontal(direction) * dome.Radius;

        foreach (DomeConnector connector in dome.Connectors)
        {
            if (owner != null && connector.Owner == owner) continue;
            if (HorizontalDistance(connector.transform.position, wallPoint) < MinSpacing) return "un connecteur";
        }

        foreach (BuildingPowerPanel socket in dome.GetComponentsInChildren<BuildingPowerPanel>())
        {
            if (HorizontalDistance(socket.transform.position, wallPoint) < PowerSocketSpacing) return "la borne électrique";
        }

        return null;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = b.y;
        return (a - b).magnitude;
    }

    // Finished dome whose wall is the closest to the point, within maxDistance of the wall (horizontally). Direction:
    // from its center toward the point
    public static Dome NearestDome(Vector3 point, float maxDistance, out Vector3 direction)
    {
        Dome nearest = null;
        float nearestDistance = maxDistance;
        direction = Vector3.forward;

        foreach (Dome dome in domes)
        {
            if (dome == null) continue;

            Vector3 offset = point - dome.transform.position;
            offset.y = 0f;
            float distance = Mathf.Abs(offset.magnitude - dome.Radius);

            if (distance <= nearestDistance && offset.sqrMagnitude > 0.01f)
            {
                nearest = dome;
                nearestDistance = distance;
                direction = offset.normalized;
            }
        }

        return nearest;
    }

    // The connector of the dome toward the direction, owned by the airlock or the corridor (destroyed with it)
    public static DomeConnector Create(Dome dome, Vector3 direction, Component owner)
    {
        if (dome == null || dome.ConnectorPrefab == null) return null;

        ConnectorPose(dome, direction, out Vector3 position, out Quaternion rotation);
        DomeConnector connector = Object.Instantiate(dome.ConnectorPrefab, position, rotation, dome.transform);
        connector.Owner = owner;

        foreach (Transform part in connector.GetComponentsInChildren<Transform>(true))
        {
            part.gameObject.layer = dome.gameObject.layer;
        }

        dome.RefreshColliders();
        return connector;
    }

    public static void Remove(DomeConnector connector)
    {
        if (connector == null) return;

        Dome dome = connector.GetComponentInParent<Dome>();
        connector.gameObject.SetActive(false);
        Object.Destroy(connector.gameObject);

        if (dome != null) dome.RefreshColliders();
    }

    // ---------- Links saved by id (like the cables) ----------

    public static string IdOf(BuiltStructure structure)
    {
        SaveableObject saveable = structure != null ? structure.GetComponent<SaveableObject>() : null;
        return saveable != null ? saveable.Id : null;
    }

    public static BuiltStructure FindStructure(string id)
    {
        if (string.IsNullOrEmpty(id) || SaveManager.Instance == null) return null;

        SaveableObject saveable = SaveManager.Instance.GetLiveObject(id);
        return saveable != null ? saveable.GetComponent<BuiltStructure>() : null;
    }
}

// Ghost of a connector during a placement (Builder): a copy of the meshes of the connector of the dome
public class ConnectorPreview
{
    private GameObject visual;
    private DomeConnector source;

    public void Show(Dome dome, Vector3 direction, Material material)
    {
        if (dome == null || dome.ConnectorPrefab == null)
        {
            Hide();
            return;
        }

        if (visual == null || source != dome.ConnectorPrefab)
        {
            Destroy();
            source = dome.ConnectorPrefab;
            visual = BuildUtility.CreateVisual(source.gameObject, material, out _);
            visual.name = source.name + " (ghost)";
        }

        DomeConnection.ConnectorPose(dome, direction, out Vector3 position, out Quaternion rotation);
        visual.transform.SetPositionAndRotation(position, rotation);
        visual.SetActive(true);

        BuildUtility.SetMaterial(visual, material);
    }

    public void Hide()
    {
        if (visual != null) visual.SetActive(false);
    }

    public void Destroy()
    {
        if (visual != null) Object.Destroy(visual);
        visual = null;
        source = null;
    }
}
