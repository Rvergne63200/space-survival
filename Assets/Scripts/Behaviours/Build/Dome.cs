using System.Collections.Generic;
using UnityEngine;

// Dome (prefabs Prefabs/Build/Dome_8m, Dome_12m, Dome_16m, model Resources/Models/Dome_<size>), on the root:
// - Connectors: airlocks and corridors attach to its wall through its connector (DomeConnection), created as children
// - Mask: its materials are replaced by masked copies (shaders SpaceSurvival/Masked Standard and Masked Transparent):
//   what is inside the mask of a connector is not drawn
// - Colliders: a mesh collider on each mesh of the model, cut by the masks of its connectors (triangles clipped by the
//   boxes), so that the player goes through the wall where a connector is
// Only on the finished dome (the plan and the ghost are copies of the meshes, without components)
public class Dome : MonoBehaviour
{
    // Outer radius of the wall (m), where the connectors are placed
    [SerializeField] private float radius = 4f;
    [SerializeField] private DomeConnector connectorPrefab;

    private static Shader maskedOpaque;
    private static Shader maskedTransparent;
    private static readonly Dictionary<Material, Material> maskedMaterials = new Dictionary<Material, Material>();

    private readonly List<(MeshCollider collider, Mesh source)> colliders = new List<(MeshCollider, Mesh)>();
    private readonly List<Mesh> cutMeshes = new List<Mesh>();

    public float Radius => radius;
    public DomeConnector ConnectorPrefab => connectorPrefab;

    public BuiltStructure Structure => GetComponent<BuiltStructure>();

    // The connectors on its wall
    public IEnumerable<DomeConnector> Connectors => GetComponentsInChildren<DomeConnector>();

    // The model (child "Model"): the power socket and the connectors keep their own materials and colliders
    private Transform Model
    {
        get
        {
            Transform model = transform.Find("Model");
            return model != null ? model : transform;
        }
    }

    private void Awake()
    {
        UseMaskedMaterials();

        foreach (MeshFilter filter in Model.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null) continue;

            MeshCollider meshCollider = filter.GetComponent<MeshCollider>();
            if (meshCollider == null) meshCollider = filter.gameObject.AddComponent<MeshCollider>();

            meshCollider.sharedMesh = filter.sharedMesh;
            colliders.Add((meshCollider, filter.sharedMesh));
        }
    }

    private void OnEnable()
    {
        DomeConnection.Register(this);
    }

    private void OnDisable()
    {
        DomeConnection.Unregister(this);
    }

    private void OnDestroy()
    {
        foreach (Mesh mesh in cutMeshes)
        {
            if (mesh != null) Destroy(mesh);
        }
    }

    // ---------- Materials ----------

    private void UseMaskedMaterials()
    {
        if (maskedOpaque == null) maskedOpaque = Shader.Find("SpaceSurvival/Masked Standard");
        if (maskedTransparent == null) maskedTransparent = Shader.Find("SpaceSurvival/Masked Transparent");

        if (maskedOpaque == null || maskedTransparent == null)
        {
            Debug.LogWarning("Dome: masked shaders not found (Resources/Shaders), the connectors will not open the dome.", this);
            return;
        }

        foreach (Renderer renderer in Model.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = Masked(materials[i]);
            }

            renderer.sharedMaterials = materials;
        }
    }

    // One masked copy per material of the models, shared by every dome
    private static Material Masked(Material source)
    {
        if (source == null) return null;

        if (!maskedMaterials.TryGetValue(source, out Material masked) || masked == null)
        {
            bool transparent = source.renderQueue >= 3000;
            masked = new Material(transparent ? maskedTransparent : maskedOpaque) { name = source.name + " (masked)" };
            masked.CopyPropertiesFromMaterial(source);
            masked.renderQueue = transparent ? 3000 : -1;
            maskedMaterials[source] = masked;
        }

        return masked;
    }

    // ---------- Colliders ----------

    // Called when a connector is added or removed
    public void RefreshColliders()
    {
        List<List<Plane>> boxes = new List<List<Plane>>();

        foreach (DomeConnector connector in Connectors)
        {
            if (!connector.isActiveAndEnabled) continue;

            List<Plane> planes = new List<Plane>();
            connector.GetMaskPlanes(planes);
            boxes.Add(planes);
        }

        foreach (Mesh mesh in cutMeshes)
        {
            if (mesh != null) Destroy(mesh);
        }

        cutMeshes.Clear();

        foreach ((MeshCollider meshCollider, Mesh source) in colliders)
        {
            if (meshCollider == null) continue;

            if (boxes.Count == 0)
            {
                meshCollider.sharedMesh = source;
                continue;
            }

            Mesh cut = MeshCutter.CutOut(source, meshCollider.transform.localToWorldMatrix, boxes);
            cutMeshes.Add(cut);
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = cut;
        }
    }
}
