using System.Collections.Generic;
using UnityEngine;

// Connector between a dome and what is attached to it (airlock, corridor), on the root of the prefabs
// Prefabs/Build/Connectors/DomeConnector_<size> (model Resources/Models/DomeConnector_<size>). Created by DomeConnection
// as a child of the dome, at the wall: origin on the outer skin of the wall at ground level, +Z outward. Its child
// "Socket" is the flange where the airlock or the corridor starts.
// Mask: a box (local space) where the dome is hidden (masked shaders of the dome, see Dome) and where its colliders
// are cut, so that the player goes through the wall. The connector itself is not masked: its shell hides the edges
public class DomeConnector : MonoBehaviour
{
    public const int MaxMasks = 32;

    private static readonly List<DomeConnector> all = new List<DomeConnector>();
    private static readonly Matrix4x4[] matrices = new Matrix4x4[MaxMasks];
    private static readonly int MaskCountId = Shader.PropertyToID("_StructureMaskCount");
    private static readonly int MasksId = Shader.PropertyToID("_StructureMasks");

    // Box of the mask in the local space of the connector: through the dome wall, inside the shell of the connector
    [SerializeField] private Vector3 maskCenter = new Vector3(0f, 1.51f, -0.2f);
    [SerializeField] private Vector3 maskSize = new Vector3(2.2f, 2.6f, 1.2f);

    private Transform socket;

    // What the connector belongs to (airlock, corridor): it destroys it with itself
    public Component Owner { get; set; }

    public Transform Socket => socket != null ? socket : (socket = FindChild(transform, "Socket"));

    // World to unit box: |p| < 0.5 on the three axes inside the mask
    public Matrix4x4 MaskMatrix => Matrix4x4.TRS(transform.TransformPoint(maskCenter), transform.rotation, maskSize).inverse;

    // Distance from the wall to the flange (local Z of the socket), read from a prefab
    public static float FlangeDistance(DomeConnector prefab)
    {
        Transform prefabSocket = prefab != null ? prefab.Socket : null;
        return prefabSocket != null ? prefab.transform.InverseTransformPoint(prefabSocket.position).z : 0.55f;
    }

    private void Awake()
    {
        BuildUtility.AddMeshColliders(gameObject);
    }

    private void OnEnable()
    {
        all.Add(this);
        UpdateMasks();
    }

    private void OnDisable()
    {
        all.Remove(this);
        UpdateMasks();
    }

    // The point is inside the mask
    public bool MaskContains(Vector3 worldPoint)
    {
        Vector3 p = MaskMatrix.MultiplyPoint3x4(worldPoint);
        return Mathf.Abs(p.x) < 0.5f && Mathf.Abs(p.y) < 0.5f && Mathf.Abs(p.z) < 0.5f;
    }

    // Planes of the mask box in world space (normal outward, for the clipping of the colliders)
    public void GetMaskPlanes(List<Plane> planes)
    {
        planes.Clear();
        Vector3 center = transform.TransformPoint(maskCenter);
        Vector3 half = maskSize * 0.5f;
        Vector3[] axes = { transform.right, transform.up, transform.forward };
        float[] extents = { half.x, half.y, half.z };

        for (int i = 0; i < 3; i++)
        {
            planes.Add(new Plane(axes[i], center + axes[i] * extents[i]));
            planes.Add(new Plane(-axes[i], center - axes[i] * extents[i]));
        }
    }

    // Sent to every masked shader (global): the masks of all the connectors
    private static void UpdateMasks()
    {
        int count = Mathf.Min(all.Count, MaxMasks);

        for (int i = 0; i < count; i++)
        {
            matrices[i] = all[i].MaskMatrix;
        }

        Shader.SetGlobalInt(MaskCountId, count);
        Shader.SetGlobalMatrixArray(MasksId, matrices);

        if (all.Count > MaxMasks)
        {
            Debug.LogWarning("More than " + MaxMasks + " dome connectors: the others do not open their dome.");
        }
    }

    private static Transform FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName) return child;
        }

        return null;
    }
}
