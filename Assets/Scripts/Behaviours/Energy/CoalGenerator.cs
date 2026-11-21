using UnityEngine;

// Coal generator (placed item 30, prefab Prefabs/Environment/Energy/CoalGenerator, model Resources/Models/CoalGenerator
// moved by its CoalGeneratorAnimator, on the model child):
// - Fuel: a reserve of one slot that only takes coal (ItemStorage, 99). Switched on, it burns one coal after the other,
//   "burnTime" seconds each (60), and gives its full power ("generation", 1000 W) while a coal burns, whatever the
//   network takes (like the other generators). No coal left: it stops (on, but without power). Switched off (or held off
//   by a distribution panel), the coal burning is lost: switched on again, it takes the next one
// - Interaction: anywhere on the machine, its interface (the storage interface, IStorageMachine): the reserve next to
//   the inventory, the state, a gauge of the coal burning and the button to switch it on or off. The hatch of the hopper
//   opens with the interface and closes with it
// - Model: running while it burns (TurnOn / TurnOff). Chimney: outside, as short as possible, no collar. Inside a
//   dome, it goes through the roof (0.5 m above it) with the collar where it crosses, along the normal of the dome
//   (from the center of its sphere). Checked again every few seconds (a dome loaded after the generator, for example)
// - Save: the coal of the reserve (ItemStorage) and the switch (EnergyDevice). Not the coal burning: lost at the reload
[RequireComponent(typeof(ItemStorage))]
public class CoalGenerator : EnergyGenerator, IInterractable, IStorageMachine, IPlacementPreview
{
    [SerializeField] private ItemData fuel;
    [SerializeField] private float burnTime = 60f;
    [SerializeField] private int reserveSize = 99;

    // Height of the chimney above the roof it goes through, and how often the roof is looked for
    [SerializeField] private float chimneyAboveRoof = 1.5f;
    [SerializeField] private float roofCheckInterval = 2f;

    // Center of the sphere of a dome above its origin (wall up to 1.2 m, then the sphere)
    private const float DomeSphereHeight = 1.2f;

    // The collar hides where the chimney goes through the structure of the dome: centered on it, thicker than it by this
    // margin on each side (along its normal). Thinnest collar, where only a pane is crossed
    private const float CollarMargin = 0.03f;
    private const float MinStructureThickness = 0.06f;

    private ItemStorage reserve;
    private CoalGeneratorAnimator generatorAnimator;
    private Transform chimney;

    // Seconds left for the coal burning (0: none)
    private float burning;
    private float roofTimer;
    private float roofHeight = float.NaN;

    // Player whose interface shows the reserve
    private Searcher searcher;

    // State last sent to the animator
    private bool animatorRunning;

    public ItemStorage Reserve => reserve;
    public ItemData Fuel => fuel;

    // Full power while a coal burns
    public override float Generation => burning > 0f ? base.Generation : 0f;

    private void Awake()
    {
        reserve = GetComponent<ItemStorage>();
        generatorAnimator = GetComponentInChildren<CoalGeneratorAnimator>();
        chimney = BuildUtility.FindChild(transform, "Chimney_Shaft");

        BuildUtility.AddMeshColliders(gameObject);
    }

    // Only coal in the reserve. The restriction is not saved, and the slots are created again when the save is restored:
    // set again each frame (one slot)
    private void RestrictReserve()
    {
        foreach (ItemSlot<Item> slot in reserve.Content.Slots)
        {
            slot?.SetRestriction(IsFuel, reserveSize);
        }
    }

    private bool IsFuel(ItemData data) => data == fuel;

    private void Start() => RestrictReserve();

    private void Update()
    {
        RestrictReserve();

        if (IsOn)
        {
            if (burning <= 0f && TakeCoal()) burning = burnTime;

            if (burning > 0f) burning = Mathf.Max(0f, burning - Time.deltaTime);
        }
        else
        {
            // Switched off: the coal burning is lost
            burning = 0f;
        }

        // The interface is closed (or shows another storage): the hatch closes
        if (searcher != null && searcher.CurrentSearchedStorage != reserve)
        {
            searcher = null;
            CloseHatch();
        }

        UpdateAnimator();

        roofTimer -= Time.deltaTime;

        if (roofTimer <= 0f)
        {
            roofTimer = roofCheckInterval;
            UpdateChimney();
        }
    }

    private bool TakeCoal()
    {
        foreach (ItemSlot<Item> slot in reserve.Content.Slots)
        {
            if (slot == null || slot.IsEmpty() || slot.Item.Data != fuel) continue;

            slot.Remove(1);
            reserve.Content.ev_Update?.Invoke();
            return true;
        }

        return false;
    }

    public int CoalCount
    {
        get
        {
            int count = 0;

            foreach (ItemSlot<Item> slot in reserve.Content.Slots)
            {
                if (slot != null && !slot.IsEmpty() && slot.Item.Data == fuel) count += slot.Quantity;
            }

            return count;
        }
    }

    // ---------- Model ----------

    // Only when it changes: TurnOn plays the lever, then lights the fire; called again meanwhile, it would start over
    private void UpdateAnimator()
    {
        if (generatorAnimator == null) return;

        bool running = IsOn && burning > 0f;

        if (running == animatorRunning) return;

        animatorRunning = running;

        if (running) generatorAnimator.TurnOn();
        else generatorAnimator.TurnOff();
    }

    // Inside a dome: through its roof, with the collar. Outside: short, no collar. Only when the roof changes
    private void UpdateChimney()
    {
        if (generatorAnimator == null || chimney == null) return;

        bool found = FindRoof(chimney.position, transform.position, out Roof roof);
        float height = found ? roof.Outer : -1f;

        if (!float.IsNaN(roofHeight) && Mathf.Abs(height - roofHeight) < 0.01f) return;

        roofHeight = height;
        ApplyChimney(generatorAnimator, found, roof, chimneyAboveRoof);
    }

    // Where the chimney goes through the roof: heights (world) of the outside and the inside of the structure, and its
    // normal
    private struct Roof
    {
        public float Outer;
        public float Inner;
        public Vector3 Normal;
    }

    // Chimney above the roof, collar centered on the structure crossed and thicker than it (its vertical span projected
    // on the normal, plus the margins)
    private static void ApplyChimney(CoalGeneratorAnimator animator, bool underRoof, Roof roof, float aboveRoof)
    {
        if (!underRoof)
        {
            animator.SetChimneyHeight(CoalGeneratorAnimator.MinChimneyHeight);
            animator.HideCollar();
            return;
        }

        float cos = Mathf.Abs(Vector3.Dot(roof.Normal, Vector3.up));
        float structure = Mathf.Max((roof.Outer - roof.Inner) * cos, MinStructureThickness);

        animator.SetChimneyTopWorldHeight(roof.Outer + aboveRoof);
        animator.SetCollarRotation(Quaternion.FromToRotation(Vector3.up, roof.Normal));
        animator.SetCollarThickness(structure + 2f * CollarMargin);
        animator.SetCollarWorldHeight((roof.Outer + roof.Inner) * 0.5f);
    }

    // Roof of the dome above the chimney, when the generator stands inside that dome (not outside, under its edge):
    // outside from above (first hit coming down), inside from the chimney (lowest hit going up, back faces included).
    // No inside found: the thinnest structure under the outside
    private static bool FindRoof(Vector3 chimneyPosition, Vector3 generatorPosition, out Roof roof)
    {
        roof = new Roof { Normal = Vector3.up };
        Vector3 point = Vector3.zero;

        Vector3 origin = new Vector3(chimneyPosition.x, chimneyPosition.y + 50f, chimneyPosition.z);
        float best = float.MaxValue;
        Dome roofDome = null;

        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 50f, ~0, QueryTriggerInteraction.Ignore))
        {
            Dome dome = hit.collider.GetComponentInParent<Dome>();

            if (dome == null || hit.collider.GetComponentInParent<DomeConnector>() != null) continue;
            if (hit.distance >= best) continue;

            best = hit.distance;
            point = hit.point;
            roofDome = dome;
        }

        if (roofDome == null) return false;

        BuildingInterior interior = roofDome.GetComponentInChildren<BuildingInterior>();
        if (interior != null && !interior.Contains(generatorPosition + Vector3.up * 0.5f)) return false;

        Vector3 center = roofDome.transform.position + Vector3.up * DomeSphereHeight;
        roof.Normal = (point - center).sqrMagnitude > 0.0001f ? (point - center).normalized : Vector3.up;
        roof.Outer = point.y;
        roof.Inner = point.y - MinStructureThickness;

        // Inside of the structure: lowest face of the same dome between the base of the chimney and the outside. Seen from
        // inside, the faces of the roof look away: the back faces are hit for this ray only
        Vector3 below = new Vector3(chimneyPosition.x, chimneyPosition.y, chimneyPosition.z);
        float lowest = float.MaxValue;
        bool hitBackfaces = Physics.queriesHitBackfaces;
        Physics.queriesHitBackfaces = true;

        foreach (RaycastHit hit in Physics.RaycastAll(below, Vector3.up, Mathf.Max(0f, point.y - below.y), ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.GetComponentInParent<Dome>() != roofDome) continue;

            if (hit.point.y < lowest) lowest = hit.point.y;
        }

        Physics.queriesHitBackfaces = hitBackfaces;

        if (lowest < point.y - 0.005f) roof.Inner = lowest;

        return true;
    }

    // ---------- Ghost (IPlacementPreview) ----------

    // The model, with its CoalGeneratorAnimator: the ghost shows the chimney as it will be once placed
    public GameObject PreviewModel
    {
        get
        {
            Transform model = transform.Find("Model");
            return model != null ? model.gameObject : null;
        }
    }

    // Called each frame on the prefab, for the ghost
    public void UpdatePreview(GameObject ghost)
    {
        CoalGeneratorAnimator animator = ghost.GetComponentInChildren<CoalGeneratorAnimator>();
        Transform ghostChimney = BuildUtility.FindChild(ghost.transform, "Chimney_Shaft");

        if (animator == null || ghostChimney == null) return;

        bool found = FindRoof(ghostChimney.position, ghost.transform.position, out Roof roof);
        ApplyChimney(animator, found, roof, chimneyAboveRoof);
    }

    // ---------- Interaction ----------

    // Opens its interface (the reserve, with the state and the switch), and the hatch
    public void Interract(GameObject interractor)
    {
        Searcher interfaceSearcher = interractor.GetComponent<Searcher>();

        if (interfaceSearcher == null) return;

        interfaceSearcher.Search(reserve);

        if (interfaceSearcher.CurrentSearchedStorage == reserve)
        {
            searcher = interfaceSearcher;
            if (generatorAnimator != null && !generatorAnimator.IsHatchOpen) generatorAnimator.OpenHatch();
        }
    }

    // ---------- Interface (IStorageMachine) ----------

    public string MachineName => DisplayName;

    public string MachineState
    {
        get
        {
            if (ForcedOff) return "Éteint par le tableau";
            if (!SwitchedOn) return "Éteint";
            if (burning <= 0f) return "Plus de charbon";
            return "En marche : " + EnergyValue.Format(Output, "W") + " / " + EnergyValue.Format(base.Generation, "W");
        }
    }

    public float Gauge => burnTime > 0f ? Mathf.Clamp01(burning / burnTime) : 0f;

    public string GaugeText => burning > 0f ? "Charbon en cours : " + Mathf.CeilToInt(burning) + " s" : "Aucun charbon en combustion";

    public bool MachineOn => SwitchedOn;

    public bool CanSwitchMachine => !ForcedOff;

    public void SetMachineOn(bool on) => SetSwitchedOn(on);

    public override string GetInfo()
    {
        string fuelText = burning > 0f ? ", charbon : " + CoalCount + " + " + Mathf.CeilToInt(burning) + " s" : ", charbon : " + CoalCount;

        return DisplayName + " : " + OffText(false) + EnergyValue.Format(Output, "W") + " / " + EnergyValue.Format(base.Generation, "W")
            + (IsOn && burning <= 0f ? " (plus de charbon)" : "") + fuelText;
    }

    private void CloseHatch()
    {
        if (generatorAnimator != null && generatorAnimator.IsHatchOpen) generatorAnimator.CloseHatch();
    }
}
