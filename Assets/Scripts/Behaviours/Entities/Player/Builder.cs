using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Construction of the player:
// - build wheel: hold the Build key (B) and click an option (Destroy or a structure), releasing the key closes it
// - placement: while a placeable item is in hand, a ghost shows where it will be placed
// - build: the structure chosen in the wheel is shown as a ghost, then placed as a plan (StructureBlueprint)
// - destruction: the pointed structure, plan, placed object or electric cable is highlighted, a click destroys it
// With a ghost: R rotates (Shift + R: 45° steps), left click places, right click cancels
public class Builder : MonoBehaviour
{
    private enum Mode { None, Placing, Building, Destroying }

    public float range = 8f;
    public float rotationSpeed = 90f;
    public float rotationStep = 45f;

    private PlayerContext playerContext;
    private PlayerInputActions inputActions;

    private Mode mode = Mode.None;
    private float rotation;

    // Ghost (placement and build)
    private GameObject ghost;
    private Bounds ghostBounds;
    private bool ghostValid;
    private Material validMaterial;
    private Material invalidMaterial;

    private PlaceableItemData placingItem;
    private StructureData buildingStructure;

    // Placement cancelled with a right click: not shown again until the item in hand changes
    private ItemData dismissedItem;
    private int dismissedIndex = -1;

    // Destruction
    private Component destroyTarget;
    private readonly Dictionary<Renderer, Material[]> highlightedRenderers = new Dictionary<Renderer, Material[]>();

    private Transform Source => playerContext != null && playerContext.PlayerManager != null ? playerContext.PlayerManager.ViewSource : null;
    private PlayerInventory Inventory => PlayerManager.Instance != null ? PlayerManager.Instance.Inventory : null;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        playerContext = GetComponent<PlayerContext>();

        validMaterial = Resources.Load<Material>("Materials/ValidBuild");
        invalidMaterial = Resources.Load<Material>("Materials/InvalidBuild");
    }

    private void OnEnable()
    {
        inputActions.Actions.Build.performed += OnBuildPerformed;
        inputActions.Actions.Build.canceled += OnBuildCanceled;
        inputActions.Actions.Use1.performed += OnUse1Performed;
        inputActions.Actions.Use2.performed += OnUse2Performed;
        inputActions.Actions.Enable();
    }

    private void OnDisable()
    {
        // Disable before unsubscribing so that the canceled callback still closes the wheel
        inputActions.Actions.Disable();
        inputActions.Actions.Build.performed -= OnBuildPerformed;
        inputActions.Actions.Build.canceled -= OnBuildCanceled;
        inputActions.Actions.Use1.performed -= OnUse1Performed;
        inputActions.Actions.Use2.performed -= OnUse2Performed;

        SetMode(Mode.None);
    }

    private void OnDestroy()
    {
        SetMode(Mode.None);
    }

    // ---------- Wheel ----------

    private void OnBuildPerformed(InputAction.CallbackContext context)
    {
        if (InterfaceManager.GetInstance().HasOpenInterface || IsDead()) return;

        if (BuildWheelUI.Instance != null)
        {
            BuildWheelUI.Instance.Open(this);
        }
    }

    private void OnBuildCanceled(InputAction.CallbackContext context)
    {
        if (BuildWheelUI.Instance != null)
        {
            BuildWheelUI.Instance.Close();
        }
    }

    // Called by the build wheel
    public void SelectDestroy()
    {
        SetMode(Mode.Destroying);
    }

    // Called by the build wheel
    public void SelectStructure(StructureData structure)
    {
        if (structure == null || structure.Prefab == null) return;

        SetMode(Mode.None);
        buildingStructure = structure;
        CreateGhost(structure.Prefab);
        SetMode(Mode.Building);
    }

    // ---------- Modes ----------

    private void SetMode(Mode newMode)
    {
        if (newMode != Mode.Placing && newMode != Mode.Building)
        {
            DestroyGhost();
            placingItem = null;
            buildingStructure = null;
        }

        if (newMode != Mode.Destroying)
        {
            SetDestroyTarget(null);
        }

        // The Build mode disables the use of the item in hand (left/right click are used by the construction)
        if (mode == Mode.None && newMode != Mode.None)
        {
            ModeManager.Instance.EnableMode(PlayingMode.Build);
        }
        else if (mode != Mode.None && newMode == Mode.None)
        {
            ModeManager.Instance.DisableMode(PlayingMode.Build);
        }

        mode = newMode;
        UpdateHint();
    }

    // Banner with the mode and its controls (BuildModeUI)
    private void UpdateHint()
    {
        if (BuildModeUI.Instance == null) return;

        const string ghostControls = "Clic gauche : poser   ·   R : tourner (Shift + R : 45°)   ·   Clic droit / Échap : annuler";

        switch (mode)
        {
            case Mode.Placing:
                BuildModeUI.Instance.Show("Placement : " + (placingItem != null ? placingItem.Name : "") + "   ·   " + ghostControls);
                break;
            case Mode.Building:
                BuildModeUI.Instance.Show("Construction : " + (buildingStructure != null ? buildingStructure.Name : "") + "   ·   " + ghostControls);
                break;
            case Mode.Destroying:
                BuildModeUI.Instance.Show("Destruction   ·   Clic gauche : détruire la cible (en rouge)   ·   Clic droit / Échap : quitter");
                break;
            default:
                BuildModeUI.Instance.Hide();
                break;
        }
    }

    private void Update()
    {
        UpdatePlacementFromHand();

        // Hidden while an interface (inventory, storage...) or the wheel is open
        bool blocked = InterfaceManager.GetInstance().HasOpenInterface || (BuildWheelUI.Instance != null && BuildWheelUI.Instance.IsOpen) || IsDead();

        if (ghost != null)
        {
            ghost.SetActive(!blocked);
        }

        if (blocked || Source == null) return;

        switch (mode)
        {
            case Mode.Placing:
            case Mode.Building:
                UpdateRotation();
                UpdateGhost();
                break;
            case Mode.Destroying:
                UpdateDestroyTarget();
                break;
        }
    }

    // A placeable item in hand starts the placement, another item stops it
    private void UpdatePlacementFromHand()
    {
        PlayerInventory inventory = Inventory;

        if (inventory == null) return;

        PlaceableItemData handed = !inventory.HandedSlot.IsEmpty() ? inventory.HandedSlot.Item.Data as PlaceableItemData : null;

        // The dismissed placement is forgotten when the item in hand changes
        if (handed != dismissedItem || inventory.HandedIndex != dismissedIndex)
        {
            dismissedItem = null;
            dismissedIndex = -1;
        }

        if (mode == Mode.Placing && handed != placingItem)
        {
            SetMode(Mode.None);
        }

        if (mode == Mode.None && handed != null && handed.PlacedPrefab != null && dismissedItem == null)
        {
            placingItem = handed;
            CreateGhost(handed.PlacedPrefab);
            SetMode(Mode.Placing);
        }
    }

    private bool IsDead()
    {
        return PlayerManager.Instance != null && PlayerManager.Instance.IsDead;
    }

    // ---------- Ghost ----------

    private void CreateGhost(GameObject prefab)
    {
        DestroyGhost();

        ghost = BuildUtility.CreateVisual(prefab, invalidMaterial, out ghostBounds);
        ghost.name = prefab.name + " (ghost)";
        ghostValid = false;
    }

    private void DestroyGhost()
    {
        if (ghost != null)
        {
            Destroy(ghost);
            ghost = null;
        }
    }

    private void UpdateRotation()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        if (keyboard.shiftKey.isPressed)
        {
            if (keyboard.rKey.wasPressedThisFrame)
            {
                rotation = (Mathf.Round(rotation / rotationStep) + 1) * rotationStep;
            }
        }
        else if (keyboard.rKey.isPressed)
        {
            rotation += rotationSpeed * Time.deltaTime;
        }

        rotation %= 360f;
    }

    private void UpdateGhost()
    {
        if (ghost == null) return;

        // A structure can be larger than the range: its center is kept out of reach of the player,
        // at least its radius + 1 m away, otherwise the player would always be inside it (invalid)
        float footprintRadius = 0f;

        if (mode == Mode.Building)
        {
            // Half diagonal: the player stays out of the box whatever its rotation
            GetGhostBox(out _, out Vector3 halfExtents);
            footprintRadius = new Vector2(halfExtents.x, halfExtents.z).magnitude;
        }

        if (!TryGetGhostPoint(range + footprintRadius, footprintRadius > 0f ? footprintRadius + 1f : 0f, out Vector3 point, out Vector3 normal))
        {
            ghost.SetActive(false);
            ghostValid = false;
            return;
        }

        ghost.SetActive(true);
        ghost.transform.SetPositionAndRotation(point, Quaternion.Euler(0f, rotation, 0f));

        bool valid = normal.y > 0.6f && !Overlaps();

        if (mode == Mode.Building && buildingStructure != null)
        {
            valid &= CheckSlope(buildingStructure.MaxSlope);
        }

        // The ghost is created with the invalid material (ghostValid false)
        if (valid != ghostValid)
        {
            ghostValid = valid;
            SetGhostMaterial(valid ? validMaterial : invalidMaterial);
        }
    }

    // Where the camera aims, within maxDistance. Closer than minDistance (horizontally) to the player, the point is pushed
    // away in the same direction and put back on the terrain
    private bool TryGetGhostPoint(float maxDistance, float minDistance, out Vector3 point, out Vector3 normal)
    {
        point = Vector3.zero;
        normal = Vector3.up;

        // Everything except the player and the triggers
        int layerMask = ~(1 << gameObject.layer);

        if (!Physics.Raycast(Source.position, Source.forward, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        point = hit.point;
        normal = hit.normal;

        Vector3 fromPlayer = point - transform.position;
        fromPlayer.y = 0f;

        if (minDistance <= 0f || fromPlayer.magnitude >= minDistance) return true;

        Vector3 direction = fromPlayer.sqrMagnitude > 0.01f ? fromPlayer.normalized : Vector3.ProjectOnPlane(Source.forward, Vector3.up).normalized;
        Vector3 pushed = transform.position + direction * minDistance;

        if (!Physics.Raycast(pushed + Vector3.up * 50f, Vector3.down, out RaycastHit groundHit, 200f, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        point = groundHit.point;
        normal = groundHit.normal;
        return true;
    }

    private void SetGhostMaterial(Material material)
    {
        foreach (MeshRenderer renderer in ghost.GetComponentsInChildren<MeshRenderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }

    // World center and half size of the ghost bounds
    private void GetGhostBox(out Vector3 center, out Vector3 halfExtents)
    {
        center = ghost.transform.TransformPoint(ghostBounds.center);
        halfExtents = Vector3.Scale(ghostBounds.extents, ghost.transform.lossyScale);
    }

    // Anything in the way (the terrain excepted): slightly smaller box, so that the supporting floor does not count
    private bool Overlaps()
    {
        GetGhostBox(out Vector3 center, out Vector3 halfExtents);

        int layerMask = ~LayerMask.GetMask("Terrain");

        return Physics.CheckBox(center, halfExtents * 0.9f, ghost.transform.rotation, layerMask, QueryTriggerInteraction.Ignore);
    }

    // Height difference of the terrain under the corners of the ghost
    private bool CheckSlope(float maxSlope)
    {
        GetGhostBox(out Vector3 center, out Vector3 halfExtents);

        int terrainMask = LayerMask.GetMask("Terrain");
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int corner = 0; corner < 4; corner++)
        {
            Vector3 offset = new Vector3((corner & 1) == 0 ? -halfExtents.x : halfExtents.x, 0f, (corner & 2) == 0 ? -halfExtents.z : halfExtents.z);
            Vector3 origin = center + ghost.transform.rotation * offset + Vector3.up * 50f;

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f, terrainMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            minHeight = Mathf.Min(minHeight, hit.point.y);
            maxHeight = Mathf.Max(maxHeight, hit.point.y);
        }

        return maxHeight - minHeight <= maxSlope;
    }

    // ---------- Clicks ----------

    private void OnUse1Performed(InputAction.CallbackContext context)
    {
        if (InterfaceManager.GetInstance().HasOpenInterface || (BuildWheelUI.Instance != null && BuildWheelUI.Instance.IsOpen) || IsDead()) return;

        switch (mode)
        {
            case Mode.Placing:
                Place();
                break;
            case Mode.Building:
                PlaceBlueprint();
                break;
            case Mode.Destroying:
                DestroyTarget();
                break;
        }
    }

    // Right click: cancel
    private void OnUse2Performed(InputAction.CallbackContext context)
    {
        if (InterfaceManager.GetInstance().HasOpenInterface || (BuildWheelUI.Instance != null && BuildWheelUI.Instance.IsOpen)) return;

        Cancel();
    }

    // In placement, build or destruction mode (Escape cancels it before opening the pause menu, see ParentUI)
    public bool IsActive => mode != Mode.None;

    // Back to the normal mode. A cancelled placement is not shown again until the item in hand changes
    public void Cancel()
    {
        if (mode == Mode.Placing && Inventory != null)
        {
            dismissedItem = placingItem;
            dismissedIndex = Inventory.HandedIndex;
        }

        SetMode(Mode.None);
    }

    private void Place()
    {
        if (ghost == null || !ghost.activeSelf || !ghostValid || placingItem == null || Inventory == null) return;

        BuildUtility.InstantiatePlaced(placingItem, ghost.transform.position, ghost.transform.rotation);
        Inventory.HandedSlot.Remove(1);
    }

    private void PlaceBlueprint()
    {
        if (ghost == null || !ghost.activeSelf || !ghostValid || buildingStructure == null) return;

        Vector3 position = ghost.transform.position;

        if (buildingStructure.FlattenTerrain && ChunkManager.Instance != null)
        {
            GetGhostBox(out _, out Vector3 halfExtents);
            ChunkManager.Instance.FlattenTerrainAt(position, Mathf.Max(halfExtents.x, halfExtents.z) + 0.5f, position.y + 0.2f);
        }

        BuildUtility.CreateBlueprint(buildingStructure, position, ghost.transform.rotation);
        SetMode(Mode.None);
    }

    // ---------- Destruction ----------

    private void UpdateDestroyTarget()
    {
        Component target = null;
        int layerMask = ~(1 << gameObject.layer);

        if (Physics.Raycast(Source.position, Source.forward, out RaycastHit hit, range, layerMask, QueryTriggerInteraction.Ignore))
        {
            // Not "??": Unity's fake null objects are not seen as null by that operator
            StructureBlueprint blueprint = hit.collider.GetComponentInParent<StructureBlueprint>();
            BuiltStructure structure = hit.collider.GetComponentInParent<BuiltStructure>();
            PlacedObject placed = hit.collider.GetComponentInParent<PlacedObject>();

            if (blueprint != null) target = blueprint;
            else if (structure != null) target = structure;
            else if (placed != null) target = placed;
        }

        // The cables only have triggers (not in the way of the player): aimed separately, when closer than the hit
        float maxCableDistance = target != null ? hit.distance : range;
        ElectricCable cable = FindAimedCable(maxCableDistance, layerMask);

        if (cable != null)
        {
            target = cable;
        }

        if (target != destroyTarget)
        {
            SetDestroyTarget(target);
        }
    }

    private ElectricCable FindAimedCable(float maxDistance, int layerMask)
    {
        ElectricCable closest = null;
        float closestDistance = maxDistance;

        foreach (RaycastHit cableHit in Physics.RaycastAll(Source.position, Source.forward, maxDistance, layerMask, QueryTriggerInteraction.Collide))
        {
            ElectricCable cable = cableHit.collider.GetComponentInParent<ElectricCable>();

            if (cable != null && cableHit.distance <= closestDistance)
            {
                closest = cable;
                closestDistance = cableHit.distance;
            }
        }

        return closest;
    }

    // The target is shown with the invalid (red) material
    private void SetDestroyTarget(Component target)
    {
        foreach (KeyValuePair<Renderer, Material[]> highlighted in highlightedRenderers)
        {
            if (highlighted.Key != null)
            {
                highlighted.Key.sharedMaterials = highlighted.Value;
            }
        }

        highlightedRenderers.Clear();
        destroyTarget = target;

        if (target == null) return;

        foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
        {
            highlightedRenderers[renderer] = renderer.sharedMaterials;

            Material[] materials = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = invalidMaterial;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private void DestroyTarget()
    {
        Component target = destroyTarget;

        if (target == null) return;

        // Original materials back before the object gives its resources and is destroyed
        SetDestroyTarget(null);

        switch (target)
        {
            case StructureBlueprint blueprint:
                blueprint.Deconstruct();
                break;
            case BuiltStructure structure:
                structure.Deconstruct();
                break;
            case PlacedObject placed:
                placed.Deconstruct();
                break;
            case ElectricCable cable:
                cable.Deconstruct();
                break;
        }
    }
}
