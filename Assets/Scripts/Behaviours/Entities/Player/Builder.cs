using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Construction of the player:
// - build wheel: hold the Build key (B) and click an option (Destroy or a structure), releasing the key closes it
// - placement: while a placeable item is in hand, a ghost shows where it will be placed
// - build: the structure chosen in the wheel is shown as a ghost, then placed as a plan (StructureBlueprint)
// - corridor (CorridorData): click a first dome, then a second one: the corridor between them is placed as a plan, with
//   a ghost of the connector on each dome
// - airlock (AirlockData): placed freely, the ghost sticks to the wall of a dome close to the aimed point, with a ghost
//   of the connector (DomeConnection). Not attached to a dome: it cannot be placed
// - destruction: the pointed structure, plan, placed object or electric cable is highlighted, a click destroys it
// With a ghost: R rotates (Shift + R: 45° steps), left click places, right click cancels
public class Builder : MonoBehaviour
{
    private enum Mode { None, Placing, Building, Corridor, Airlock, Destroying }

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

    // Corridor: first building clicked, and the corridor towards the aimed building
    public float corridorAimRange = 30f;
    private BuiltStructure corridorStart;
    private BuiltStructure corridorEnd;
    private float corridorLength;
    private float corridorSlope;
    private float ghostCorridorSlope;
    private float ghostCorridorLength = -1f;
    private string corridorHint;

    // Ghosts of the connectors (corridor: one per dome, airlock: the first one)
    private readonly ConnectorPreview startPreview = new ConnectorPreview();
    private readonly ConnectorPreview endPreview = new ConnectorPreview();

    // Airlock: dome it is attached to, and direction from its center
    private Dome airlockDome;
    private Vector3 airlockDirection;

    // Placement cancelled with a right click: not shown again until the item in hand changes
    private ItemData dismissedItem;

    // The ghost of an outdoor only object is inside (shown in the hint)
    private bool placingIndoor;
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
        SetMode(structure is CorridorData ? Mode.Corridor : structure is AirlockData ? Mode.Airlock : Mode.Building);
    }

    // ---------- Modes ----------

    private void SetMode(Mode newMode)
    {
        placingIndoor = false;

        if (newMode != Mode.Placing && newMode != Mode.Building && newMode != Mode.Corridor && newMode != Mode.Airlock)
        {
            DestroyGhost();
            placingItem = null;
            buildingStructure = null;
        }

        if (newMode != Mode.Corridor)
        {
            corridorStart = null;
            corridorEnd = null;
        }

        if (newMode != Mode.Corridor && newMode != Mode.Airlock)
        {
            corridorHint = null;
            startPreview.Destroy();
            endPreview.Destroy();
        }

        airlockDome = null;

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
                BuildModeUI.Instance.Show("Placement : " + (placingItem != null ? placingItem.Name : "") + (placingIndoor ? "   ·   Se pose uniquement dehors" : "") + "   ·   " + ghostControls);
                break;
            case Mode.Building:
                BuildModeUI.Instance.Show("Construction : " + (buildingStructure != null ? buildingStructure.Name : "") + "   ·   " + ghostControls);
                break;
            case Mode.Destroying:
                BuildModeUI.Instance.Show("Destruction   ·   Clic gauche : détruire la cible (en rouge)   ·   Clic droit / Échap : quitter");
                break;
            case Mode.Corridor:
            case Mode.Airlock:
                // Shown each frame by UpdateCorridor and UpdateAirlock
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

        if (blocked)
        {
            startPreview.Hide();
            endPreview.Hide();
        }

        if (blocked || Source == null) return;

        switch (mode)
        {
            case Mode.Placing:
            case Mode.Building:
                UpdateRotation();
                UpdateGhost();
                break;
            case Mode.Corridor:
                UpdateCorridor();
                break;
            case Mode.Airlock:
                UpdateRotation();
                UpdateAirlock();
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
        ghostCorridorLength = -1f;
        ghostCorridorSlope = 0f;
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

        // Nothing can be placed inside an airlock; some objects only outside (solar panel...)
        if (mode == Mode.Placing)
        {
            bool indoor = placingItem != null && placingItem.OutdoorOnly && IsIndoor(point);
            valid &= !Airlock.AnyContains(point) && !indoor;

            if (indoor != placingIndoor)
            {
                placingIndoor = indoor;
                UpdateHint();
            }
        }

        // The ghost is created with the invalid material (ghostValid false)
        if (valid != ghostValid)
        {
            ghostValid = valid;
            SetGhostMaterial(valid ? validMaterial : invalidMaterial);
        }
    }

    // Inside a dome, an airlock or a corridor
    private static bool IsIndoor(Vector3 point)
    {
        Vector3 above = point + Vector3.up * 0.3f;
        return BuildingInterior.AnyContains(above) || Airlock.AnyContains(above) || Corridor.AnyContains(above);
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

    private void SetGhostMaterial(Material material) => BuildUtility.SetMaterial(ghost, material);

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
            case Mode.Corridor:
                ClickCorridor();
                break;
            case Mode.Airlock:
                PlaceAirlock();
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

        if (buildingStructure.FlattenTerrain)
        {
            // A dome: its disc. Another structure: its box
            Dome dome = buildingStructure.Prefab.GetComponent<Dome>();

            if (dome != null)
            {
                TerrainFlattening.Disc(position, dome.Radius, position.y);
            }
            else
            {
                GetGhostBox(out Vector3 center, out Vector3 halfExtents);
                TerrainFlattening.Box(center, ghost.transform.rotation, halfExtents.x, halfExtents.z, position.y, 0f);
            }
        }

        BuildUtility.CreateBlueprint(buildingStructure, position, ghost.transform.rotation);
        SetMode(Mode.None);
    }

    // ---------- Corridor ----------

    private CorridorData CorridorStructure => buildingStructure as CorridorData;

    // Aimed dome and where it is aimed (point on its wall, or on anything else within corridorAimRange)
    private BuiltStructure AimedBuilding(out Vector3 point)
    {
        int layerMask = ~(1 << gameObject.layer);
        point = Source.position + Source.forward * corridorAimRange;

        if (!Physics.Raycast(Source.position, Source.forward, out RaycastHit hit, corridorAimRange, layerMask, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        point = hit.point;
        BuiltStructure structure = hit.collider.GetComponentInParent<BuiltStructure>();
        return DomeConnection.DomeOf(structure) != null ? structure : null;
    }

    // Distance from the wall of a dome within which the corridor sticks to it
    private const float CorridorSnapDistance = 3f;

    // 1. First dome: its connector follows the aimed point on its wall. 2. Clicked: the corridor goes from that connector
    // toward the aimed point, red. 3. Close to another dome (aimed, or its wall within CorridorSnapDistance): it sticks to
    // it, on the line between their centers (the first connector moves there), with the second connector
    private void UpdateCorridor()
    {
        CorridorData data = CorridorStructure;

        if (data == null || ghost == null) return;

        const string controls = "   ·   Clic droit / Échap : annuler";
        BuiltStructure aimed = AimedBuilding(out Vector3 point);
        corridorEnd = null;

        if (corridorStart == null)
        {
            ghost.SetActive(false);
            endPreview.Hide();

            Dome aimedDome = DomeConnection.DomeOf(aimed);

            if (aimedDome == null)
            {
                startPreview.Hide();
                ShowCorridorHint("Couloir : visez un premier dôme" + controls);
                return;
            }

            Vector3 toPoint = point - aimed.transform.position;
            bool free = DomeConnection.IsFree(aimedDome, toPoint);
            startPreview.Show(aimedDome, toPoint, free ? validMaterial : invalidMaterial);
            ShowCorridorHint("Couloir : " + (free ? "clic gauche pour partir de ce dôme" : "un connecteur occupe déjà cet endroit du dôme") + controls);
            return;
        }

        Dome startDome = DomeConnection.DomeOf(corridorStart);

        // Second dome: aimed, or close to the aimed point
        BuiltStructure end = aimed != corridorStart ? aimed : null;

        if (end == null)
        {
            Dome near = DomeConnection.NearestDome(point, CorridorSnapDistance, out _);
            end = near != null && near != startDome ? near.Structure : null;
        }

        if (end == null || !Corridor.ComputePlacement(corridorStart, end, out Vector3 position, out Quaternion rotation, out float length, out float slope))
        {
            ShowFreeCorridor(data, startDome, point);
            ShowCorridorHint("Couloir : approchez-le d'un autre dôme" + controls);
            return;
        }

        corridorEnd = end;
        corridorLength = length;
        corridorSlope = slope;
        ShowGhostCorridor(data, position, rotation, length, slope);

        string problem = CorridorProblem(data, corridorStart, end, length);
        SetCorridorValid(problem == null);

        Material previewMaterial = ghostValid ? validMaterial : invalidMaterial;
        Vector3 between = end.transform.position - corridorStart.transform.position;
        startPreview.Show(startDome, between, previewMaterial);
        endPreview.Show(DomeConnection.DomeOf(end), -between, previewMaterial);

        int meters = Mathf.Max(1, Mathf.CeilToInt(length));
        List<string> cost = new List<string>();

        foreach (RecipeData.ItemBlock block in data.Cost)
        {
            cost.Add((block.Quantity * meters) + " " + (block.Item != null ? block.Item.Name : "?"));
        }

        ShowCorridorHint("Couloir : " + length.ToString("0.0") + " m (" + string.Join(", ", cost) + ")"
            + (ghostValid ? "   ·   Clic gauche : poser le plan" : "   ·   " + problem) + controls);
    }

    // Not attached to a second dome: from the connector of the first dome toward the aimed point, red
    private void ShowFreeCorridor(CorridorData data, Dome startDome, Vector3 point)
    {
        Vector3 direction = DomeConnection.Horizontal(point - startDome.transform.position);
        DomeConnection.FlangePose(startDome, direction, out Vector3 flange, out Quaternion rotation);

        Vector3 toPoint = point - flange;
        toPoint.y = 0f;
        float length = Mathf.Clamp(Vector3.Dot(toPoint, direction), data.StraightLength, data.MaxLength);

        ShowGhostCorridor(data, flange + direction * (length * 0.5f), rotation, length, 0f);
        SetCorridorValid(false);
        startPreview.Show(startDome, direction, invalidMaterial);
        endPreview.Hide();
    }

    // Ghost at the pose, with the pieces of the model for this length and slope (built again only when they change)
    private void ShowGhostCorridor(CorridorData data, Vector3 position, Quaternion rotation, float length, float slope)
    {
        ghost.SetActive(true);
        ghost.transform.SetPositionAndRotation(position, rotation);

        if (Mathf.Abs(length - ghostCorridorLength) > 0.001f || Mathf.Abs(slope - ghostCorridorSlope) > 0.0005f)
        {
            ghostCorridorLength = length;
            ghostCorridorSlope = slope;
            ghostBounds = CorridorPieces.Build(ghost.transform, data, length, slope, ghostValid ? validMaterial : invalidMaterial, false);
        }
    }

    private void SetCorridorValid(bool valid)
    {
        if (valid == ghostValid) return;

        ghostValid = valid;
        SetGhostMaterial(valid ? validMaterial : invalidMaterial);
    }

    // Why the corridor cannot be placed, null if it can
    private string CorridorProblem(CorridorData data, BuiltStructure start, BuiltStructure end, float length)
    {
        if (Corridor.AreLinked(start, end)) return "ces bâtiments sont déjà reliés";
        if (!DomeConnection.IsFree(DomeConnection.DomeOf(start), end.transform.position - start.transform.position)) return "un connecteur occupe déjà cet endroit du premier dôme";
        if (!DomeConnection.IsFree(DomeConnection.DomeOf(end), start.transform.position - end.transform.position)) return "un connecteur occupe déjà cet endroit du second dôme";
        if (length < data.MinLength) return "trop court (min " + data.MinLength.ToString("0.#") + " m)";
        if (length > data.MaxLength) return "trop long (max " + data.MaxLength.ToString("0.#") + " m)";
        if (Mathf.Abs(start.transform.position.y - end.transform.position.y) > data.MaxSlope) return "dénivelé trop fort (max " + data.MaxSlope.ToString("0.#") + " m)";

        // Anything in the way, the terrain and the two buildings excepted
        GetGhostBox(out Vector3 center, out Vector3 halfExtents);
        int layerMask = ~LayerMask.GetMask("Terrain");

        foreach (Collider collider in Physics.OverlapBox(center, halfExtents * 0.9f, ghost.transform.rotation, layerMask, QueryTriggerInteraction.Ignore))
        {
            BuiltStructure owner = collider.GetComponentInParent<BuiltStructure>();

            if (owner != start && owner != end) return "obstacle sur le passage";
        }

        return null;
    }

    private void ShowCorridorHint(string text)
    {
        if (text == corridorHint || BuildModeUI.Instance == null) return;

        corridorHint = text;
        BuildModeUI.Instance.Show(text);
    }

    private void ClickCorridor()
    {
        CorridorData data = CorridorStructure;

        if (data == null || Source == null) return;

        if (corridorStart == null)
        {
            BuiltStructure aimed = AimedBuilding(out Vector3 point);
            Dome aimedDome = DomeConnection.DomeOf(aimed);

            if (aimedDome != null && DomeConnection.IsFree(aimedDome, point - aimed.transform.position)) corridorStart = aimed;
            return;
        }

        if (corridorEnd == null || ghost == null || !ghost.activeSelf || !ghostValid) return;

        // Under the whole corridor, tilted from one dome to the other
        if (data.FlattenTerrain)
        {
            Vector3 center = ghost.transform.position;
            TerrainFlattening.Box(center, ghost.transform.rotation, ghostBounds.extents.x, corridorLength * 0.5f, center.y, corridorSlope);
        }

        GameObject blueprint = BuildUtility.CreateBlueprint(data, ghost.transform.position, ghost.transform.rotation);
        Corridor corridor = blueprint.GetComponent<Corridor>();

        if (corridor != null)
        {
            corridor.Setup(corridorStart, corridorEnd, corridorLength, corridorSlope);
        }

        SetMode(Mode.None);
    }

    // ---------- Airlock ----------

    private void UpdateAirlock()
    {
        AirlockData data = buildingStructure as AirlockData;

        if (data == null || ghost == null) return;

        const string controls = "   ·   Clic droit / Échap : annuler";
        airlockDome = null;

        if (!TryGetGhostPoint(range + data.SnapDistance, 0f, out Vector3 point, out _))
        {
            ghost.SetActive(false);
            startPreview.Hide();
            ghostValid = false;
            ShowCorridorHint("Sas : visez le sol près d'un dôme" + controls);
            return;
        }

        ghost.SetActive(true);
        Dome dome = DomeConnection.NearestDome(point, data.SnapDistance, out Vector3 direction);
        string problem = null;

        if (dome == null)
        {
            // Free: follows the aim, cannot be placed
            ghost.transform.SetPositionAndRotation(point, Quaternion.Euler(0f, rotation, 0f));
            startPreview.Hide();
            problem = "approchez-le du mur d'un dôme";
        }
        else
        {
            // Sticks to the wall of the dome, on the flange of its connector
            DomeConnection.FlangePose(dome, direction, out Vector3 position, out Quaternion flangeRotation);
            ghost.transform.SetPositionAndRotation(position, flangeRotation);

            if (!DomeConnection.IsFree(dome, direction)) problem = "un connecteur occupe déjà cet endroit du dôme";
            else if (AirlockBlocked(dome.Structure)) problem = "obstacle sur le passage";
            else
            {
                airlockDome = dome;
                airlockDirection = direction;
            }
        }

        bool valid = problem == null;

        if (valid != ghostValid)
        {
            ghostValid = valid;
            SetGhostMaterial(valid ? validMaterial : invalidMaterial);
        }

        if (dome != null)
        {
            startPreview.Show(dome, direction, valid ? validMaterial : invalidMaterial);
        }

        ShowCorridorHint("Sas" + (valid ? "   ·   Clic gauche : poser le plan" : " : " + problem) + controls);
    }

    // Anything in the way, the terrain and the dome excepted
    private bool AirlockBlocked(BuiltStructure dome)
    {
        GetGhostBox(out Vector3 center, out Vector3 halfExtents);
        int layerMask = ~LayerMask.GetMask("Terrain");

        foreach (Collider collider in Physics.OverlapBox(center, halfExtents * 0.9f, ghost.transform.rotation, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (collider.GetComponentInParent<BuiltStructure>() != dome) return true;
        }

        return false;
    }

    private void PlaceAirlock()
    {
        if (ghost == null || !ghost.activeSelf || !ghostValid || airlockDome == null || buildingStructure == null) return;

        Vector3 position = ghost.transform.position;

        // Same level as the dome floor
        if (buildingStructure.FlattenTerrain)
        {
            GetGhostBox(out Vector3 center, out Vector3 halfExtents);
            TerrainFlattening.Box(center, ghost.transform.rotation, halfExtents.x, halfExtents.z, airlockDome.transform.position.y, 0f);
        }

        GameObject blueprint = BuildUtility.CreateBlueprint(buildingStructure, position, ghost.transform.rotation);
        Airlock airlock = blueprint.GetComponent<Airlock>();

        if (airlock != null)
        {
            airlock.Setup(airlockDome.Structure);
        }

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
