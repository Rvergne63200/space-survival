using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Laying of electric cables, while a cable item (CableItemData) is in hand:
// - left click on a device (EnergyDevice): start of the cable
// - left click elsewhere: intermediate point
// - left click on another device: end of the cable, which uses one item every CableItemData.MetersPerItem meters
// - right click: removes the last point, or cancels the cable
// A preview follows the aim: green when it can be finished on the aimed device, red when the items are missing
public class CablePlacer : MonoBehaviour
{
    public float range = 8f;

    [SerializeField] private Color validColor = new Color(0.3f, 1f, 0.4f);
    [SerializeField] private Color neutralColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.3f, 0.3f);

    private PlayerContext playerContext;
    private PlayerInputActions inputActions;
    private Builder builder;

    private CableItemData cableItem;
    private EnergyDevice start;
    private readonly List<Vector3> points = new List<Vector3>();

    private GameObject preview;
    private LineRenderer previewLine;
    private bool showingHint;

    // Aim of the current frame
    private bool hasAim;
    private Vector3 aimPoint;
    private EnergyDevice aimedDevice;

    private Transform Source => playerContext != null && playerContext.PlayerManager != null ? playerContext.PlayerManager.ViewSource : null;
    private PlayerInventory Inventory => PlayerManager.Instance != null ? PlayerManager.Instance.Inventory : null;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        playerContext = GetComponent<PlayerContext>();
        builder = GetComponent<Builder>();
    }

    private void OnEnable()
    {
        inputActions.Actions.Use1.performed += OnUse1Performed;
        inputActions.Actions.Use2.performed += OnUse2Performed;
        inputActions.Actions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Actions.Disable();
        inputActions.Actions.Use1.performed -= OnUse1Performed;
        inputActions.Actions.Use2.performed -= OnUse2Performed;

        Cancel();
        SetPreviewVisible(false);
        HideHint();
    }

    private void OnDestroy()
    {
        if (preview != null)
        {
            Destroy(preview);
        }
    }

    // Interface, wheel or construction mode: the cable keeps its points but nothing is shown
    private bool Blocked =>
        InterfaceManager.GetInstance().HasOpenInterface
        || (BuildWheelUI.Instance != null && BuildWheelUI.Instance.IsOpen)
        || (builder != null && builder.IsActive)
        || (PlayerManager.Instance != null && PlayerManager.Instance.IsDead)
        || Source == null;

    private void Update()
    {
        PlayerInventory inventory = Inventory;
        CableItemData handed = inventory != null && !inventory.HandedSlot.IsEmpty() ? inventory.HandedSlot.Item.Data as CableItemData : null;

        // Another item in hand: the cable being laid is cancelled
        if (handed != cableItem)
        {
            Cancel();
            cableItem = handed;
        }

        if (cableItem == null || Blocked)
        {
            SetPreviewVisible(false);
            HideHint();
            return;
        }

        UpdateAim();
        UpdatePreview();
        UpdateHint();
    }

    private void UpdateAim()
    {
        int layerMask = ~(1 << gameObject.layer);

        hasAim = Physics.Raycast(Source.position, Source.forward, out RaycastHit hit, range, layerMask, QueryTriggerInteraction.Ignore);
        // A distribution panel: its input or output, the closest to the aimed point
        aimedDevice = hasAim ? EnergyDevice.FromHit(hit) : null;
        aimPoint = hasAim ? hit.point + hit.normal * 0.03f : Source.position + Source.forward * range;

        if (aimedDevice != null)
        {
            aimPoint = aimedDevice.ConnectionPoint;
        }
    }

    // ---------- Preview ----------

    private List<Vector3> ControlPoints(Vector3 last)
    {
        List<Vector3> controlPoints = new List<Vector3> { start.ConnectionPoint };
        controlPoints.AddRange(points);
        controlPoints.Add(last);
        return controlPoints;
    }

    private void UpdatePreview()
    {
        if (start == null)
        {
            SetPreviewVisible(false);
            return;
        }

        SetPreviewVisible(true);

        List<Vector3> curve = ElectricCable.BuildCurve(ControlPoints(aimPoint));
        previewLine.positionCount = curve.Count;
        previewLine.SetPositions(curve.ToArray());

        Color color = neutralColor;

        if (!HasItems(curve))
        {
            color = invalidColor;
        }
        else if (CanEndOn(aimedDevice))
        {
            color = validColor;
        }

        previewLine.startColor = color;
        previewLine.endColor = color;
    }

    private void SetPreviewVisible(bool visible)
    {
        if (visible && preview == null)
        {
            preview = new GameObject("Electric Cable (preview)");
            previewLine = ElectricCable.CreateLine(preview, cableItem);
            previewLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Colored by the vertex colors
            previewLine.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        if (preview != null)
        {
            preview.SetActive(visible);
        }
    }

    private bool HasItems(List<Vector3> curve)
    {
        return Inventory != null && Inventory.Count(cableItem) >= cableItem.ItemsFor(ElectricCable.Length(curve));
    }

    private bool CanEndOn(EnergyDevice device)
    {
        return start != null && device != null && device != start && !start.IsLinkedTo(device) && !SamePanel(start, device) && !TwoBuildingInputs(start, device);
    }

    // The input and the output of a panel cannot be linked together, nor two ports of the same building (input and output
    // socket)
    private static bool SamePanel(EnergyDevice a, EnergyDevice b)
    {
        if (a.Panel == null || b.Panel == null) return false;

        return a.Panel == b.Panel || (a.Panel.Building != null && a.Panel.Building == b.Panel.Building);
    }

    // The inputs of two buildings cannot be cabled together: a building feeds another one from its output socket, or
    // through a corridor
    private static bool TwoBuildingInputs(EnergyDevice a, EnergyDevice b)
    {
        return a is DistributionPanelInput && a.Panel is BuildingPowerPanel && b is DistributionPanelInput && b.Panel is BuildingPowerPanel;
    }

    // ---------- Hint ----------

    private void UpdateHint()
    {
        if (BuildModeUI.Instance == null) return;

        string text;

        if (start == null)
        {
            text = cableItem.Name + "   ·   Clic gauche sur un appareil électrique : début du câble";

            if (aimedDevice != null)
            {
                text += "   ·   Visé : " + aimedDevice.DisplayName;
            }
        }
        else
        {
            List<Vector3> curve = ElectricCable.BuildCurve(ControlPoints(aimPoint));
            int needed = cableItem.ItemsFor(ElectricCable.Length(curve));
            int owned = Inventory != null ? Inventory.Count(cableItem) : 0;

            text = cableItem.Name + " : " + needed + " / " + owned
                + "   ·   Clic gauche : point de passage ou appareil d'arrivée   ·   Clic droit : retirer le dernier point";
        }

        BuildModeUI.Instance.Show(text);
        showingHint = true;
    }

    private void HideHint()
    {
        if (!showingHint) return;

        showingHint = false;

        // The construction mode shows its own banner
        if (BuildModeUI.Instance != null && (builder == null || !builder.IsActive))
        {
            BuildModeUI.Instance.Hide();
        }
    }

    // ---------- Clicks ----------

    private void OnUse1Performed(InputAction.CallbackContext context)
    {
        if (cableItem == null || Blocked) return;

        UpdateAim();

        if (start == null)
        {
            if (aimedDevice != null)
            {
                start = aimedDevice;
            }

            return;
        }

        if (aimedDevice == start) return;

        if (aimedDevice != null)
        {
            Finish(aimedDevice);
        }
        else if (hasAim)
        {
            points.Add(aimPoint);
        }
    }

    private void OnUse2Performed(InputAction.CallbackContext context)
    {
        if (cableItem == null || Blocked) return;

        if (points.Count > 0)
        {
            points.RemoveAt(points.Count - 1);
        }
        else
        {
            Cancel();
        }
    }

    private void Finish(EnergyDevice end)
    {
        if (start.IsLinkedTo(end))
        {
            NotificationUI.Show("Ces deux appareils sont déjà reliés.", NotificationType.Warning);
            return;
        }

        if (SamePanel(start, end))
        {
            NotificationUI.Show("L'entrée et la sortie d'un tableau ne peuvent pas être reliées.", NotificationType.Warning);
            return;
        }

        if (TwoBuildingInputs(start, end))
        {
            NotificationUI.Show("Les entrées de deux bâtiments ne peuvent pas être reliées : partez de la sortie d'un bâtiment.", NotificationType.Warning);
            return;
        }

        List<Vector3> curve = ElectricCable.BuildCurve(ControlPoints(end.ConnectionPoint));
        int needed = cableItem.ItemsFor(ElectricCable.Length(curve));
        PlayerInventory inventory = Inventory;

        if (inventory == null || inventory.Count(cableItem) < needed)
        {
            NotificationUI.Show("Pas assez de câble : " + needed + " nécessaires.", NotificationType.Warning);
            return;
        }

        inventory.Remove(Item.CreateFromData(cableItem), needed);
        ElectricCable.Create(cableItem, start, end, points, needed);

        start = null;
        points.Clear();
    }

    private void Cancel()
    {
        start = null;
        points.Clear();
    }
}
