using System.Collections.Generic;
using UnityEngine;

// Shared by the Builder (placement, build, destruction) and the SaveManager (objects created again when the world loads)
public static class BuildUtility
{
    private static Material blueprintMaterial;

    // Semi-transparent material of the structure plans (Resources/Materials/Blueprint)
    public static Material BlueprintMaterial => blueprintMaterial != null ? blueprintMaterial : (blueprintMaterial = Resources.Load<Material>("Materials/Blueprint"));

    // Visual copy of a prefab (meshes only: no script, no collider), with the given material.
    // localBounds: bounds of the meshes in the local space of the returned object (before its scale)
    public static GameObject CreateVisual(GameObject prefab, Material material, out Bounds localBounds)
    {
        GameObject root = new GameObject(prefab.name);
        root.transform.localScale = prefab.transform.localScale;

        localBounds = new Bounds();
        bool hasBounds = false;

        // Inactive parts and disabled renderers are not copied
        foreach (MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>())
        {
            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();

            if (renderer == null || !renderer.enabled || filter.sharedMesh == null) continue;

            // Pose of the part relative to the prefab root
            Matrix4x4 partMatrix = prefab.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;

            GameObject part = new GameObject(filter.name);
            part.transform.SetParent(root.transform, false);
            part.transform.localPosition = partMatrix.GetColumn(3);
            part.transform.localRotation = partMatrix.rotation;
            part.transform.localScale = partMatrix.lossyScale;

            part.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;

            MeshRenderer partRenderer = part.AddComponent<MeshRenderer>();
            Material[] materials = new Material[Mathf.Max(1, renderer.sharedMaterials.Length)];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            partRenderer.sharedMaterials = materials;
            partRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Corners of the mesh bounds, in the root space
            Bounds meshBounds = filter.sharedMesh.bounds;

            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 point = meshBounds.center + Vector3.Scale(meshBounds.extents, new Vector3(
                    (corner & 1) == 0 ? -1 : 1,
                    (corner & 2) == 0 ? -1 : 1,
                    (corner & 4) == 0 ? -1 : 1));

                point = partMatrix.MultiplyPoint3x4(point);

                if (!hasBounds)
                {
                    localBounds = new Bounds(point, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(point);
                }
            }
        }

        return root;
    }

    // ---------- Objects created by the construction (saved as runtime objects) ----------

    public static GameObject InstantiateStructure(StructureData data, Vector3 position, Quaternion rotation, string id = null)
    {
        GameObject instance = Object.Instantiate(data.Prefab, position, rotation);

        // Building layer, to tell the buildings apart (physics, rendering). The parts on another layer than Default keep it
        int buildingLayer = LayerMask.NameToLayer("Building");

        if (buildingLayer >= 0)
        {
            foreach (Transform part in instance.GetComponentsInChildren<Transform>(true))
            {
                if (part.gameObject.layer == 0)
                {
                    part.gameObject.layer = buildingLayer;
                }
            }
        }

        instance.AddComponent<BuiltStructure>().Initialize(data);

        // A corridor keeps its buildings and its length (set by Corridor.Setup or CopyTo, or restored from the save)
        if (data is CorridorData)
        {
            instance.AddComponent<Corridor>();
        }

        // An airlock keeps its dome
        if (data is AirlockData)
        {
            instance.AddComponent<Airlock>();
        }

        SetRuntime(instance, SaveManager.BuildPrefabKeyPrefix + data.Id, id);

        return instance;
    }

    public static GameObject CreateBlueprint(StructureData data, Vector3 position, Quaternion rotation, string id = null)
    {
        GameObject blueprint = CreateVisual(data.Prefab, BlueprintMaterial, out Bounds bounds);
        blueprint.name = data.Name + " (plan)";
        blueprint.transform.SetPositionAndRotation(position, rotation);

        // To be pointed at (interaction, destruction). The player walks through it: the Blueprint layer does not collide
        // with the Player layer (also set in the physics settings)
        BoxCollider collider = blueprint.AddComponent<BoxCollider>();
        collider.center = bounds.center;
        collider.size = bounds.size;

        int blueprintLayer = LayerMask.NameToLayer("Blueprint");
        int playerLayer = LayerMask.NameToLayer("Player");

        if (blueprintLayer >= 0)
        {
            blueprint.layer = blueprintLayer;

            if (playerLayer >= 0)
            {
                Physics.IgnoreLayerCollision(blueprintLayer, playerLayer, true);
            }
        }

        blueprint.AddComponent<StructureBlueprint>().Initialize(data);

        if (data is CorridorData)
        {
            blueprint.AddComponent<Corridor>();
        }

        if (data is AirlockData)
        {
            blueprint.AddComponent<Airlock>();
        }

        SetRuntime(blueprint, SaveManager.BlueprintPrefabKeyPrefix + data.Id, id);

        return blueprint;
    }

    public static GameObject InstantiatePlaced(PlaceableItemData data, Vector3 position, Quaternion rotation, string id = null)
    {
        GameObject instance = Object.Instantiate(data.PlacedPrefab, position, rotation);

        instance.AddComponent<PlacedObject>().Initialize(data);
        SetRuntime(instance, SaveManager.PlacedPrefabKeyPrefix + data.Id, id);

        return instance;
    }

    private static void SetRuntime(GameObject instance, string prefabKey, string id)
    {
        SaveableObject saveable = instance.GetComponent<SaveableObject>();

        if (saveable == null)
        {
            saveable = instance.AddComponent<SaveableObject>();
        }

        saveable.SetRuntime(prefabKey, id);
    }

    // Every renderer of the object (and its children) gets the material, on all its slots (ghosts, plans)
    public static void SetMaterial(GameObject root, Material material)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++) materials[i] = material;
            renderer.sharedMaterials = materials;
        }
    }

    // Bounds of the renderers of the object, in the local space of "space"
    public static Bounds LocalRendererBounds(GameObject root, Transform space)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
        {
            Bounds world = renderer.bounds;

            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 point = space.InverseTransformPoint(world.center + Vector3.Scale(world.extents,
                    new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1)));

                if (!hasBounds) { bounds = new Bounds(point, Vector3.zero); hasBounds = true; }
                else bounds.Encapsulate(point);
            }
        }

        return bounds;
    }

    // Child at any depth, by name
    public static Transform FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName) return child;
        }

        return null;
    }

    // A structure attached to others (corridor, airlock), destroyed with one of them: gives back its resources (plan:
    // what was brought)
    public static void DeconstructStructure(GameObject structure)
    {
        StructureBlueprint blueprint = structure.GetComponent<StructureBlueprint>();
        BuiltStructure built = structure.GetComponent<BuiltStructure>();

        if (blueprint != null) blueprint.Deconstruct();
        else if (built != null) built.Deconstruct();
        else Object.Destroy(structure);
    }

    // A mesh collider on each mesh of a model without collider (models imported again keep working)
    public static void AddMeshColliders(GameObject root)
    {
        foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null) continue;

            filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
        }
    }

    // ---------- Resources given back to the player ----------

    // Into the inventory, what does not fit is dropped in front of the player
    public static void GiveToPlayer(ItemData data, int quantity)
    {
        if (data == null || quantity <= 0 || PlayerManager.Instance == null) return;

        int rest = PlayerManager.Instance.Inventory.Add(Item.CreateFromData(data), quantity);

        if (rest > 0 && PlayerManager.Instance.Player != null)
        {
            Pickupable.Spawn(Item.CreateFromData(data), rest, PlayerManager.Instance.Player.transform);
            NotificationUI.Show("Inventaire plein : " + rest + " " + data.Name + " posé(s) au sol.", NotificationType.Warning);
        }
    }

    public static void GiveToPlayer(IEnumerable<RecipeData.ItemBlock> blocks)
    {
        foreach (RecipeData.ItemBlock block in blocks)
        {
            GiveToPlayer(block.Item, block.Quantity);
        }
    }

    // Content of the storages of a destroyed object
    public static void GiveStoragesToPlayer(GameObject destroyed)
    {
        Searcher searcher = PlayerManager.Instance != null && PlayerManager.Instance.Player != null
            ? PlayerManager.Instance.Player.GetComponent<Searcher>()
            : null;

        foreach (ItemStorage storage in destroyed.GetComponentsInChildren<ItemStorage>(true))
        {
            // Close it if the player is searching it
            if (searcher != null && searcher.CurrentSearchedStorage == storage)
            {
                searcher.ClearCurrentSearchedStorage();
            }

            foreach (ItemSlot<Item> slot in storage.Content.Slots)
            {
                if (slot != null && !slot.IsEmpty())
                {
                    GiveToPlayer(slot.Item.Data, slot.Quantity);
                    slot.Clear();
                }
            }
        }
    }
}
