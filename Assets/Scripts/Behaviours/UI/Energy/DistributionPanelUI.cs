using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Interface of a distribution panel (DistributionPanel.uxml), opened by interacting with it: state, power and energy
// of its circuit, switch, power limit, load shedding, priority and delayed start. The panel (GameObject holding the
// UIDocument) is registered as an interface while it is open, Escape closes it
public class DistributionPanelUI : MonoBehaviour
{
    public static DistributionPanelUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private float refreshInterval = 0.25f;

    private static readonly List<string> SheddingChoices = new List<string>
    {
        "Aucun",
        "Manque d'énergie en amont",
        "Batteries en amont sous le seuil"
    };

    private DistributionPanel target;
    private bool open;
    private float refreshTimer;

    private VisualElement boundPanel;
    private Label title;
    private Label stateLabel;
    private Label powerLabel;
    private Label energyLabel;
    private Button switchButton;
    private FloatField limitField;
    private Toggle limitInfinite;
    private DropdownField sheddingField;
    private Slider thresholdField;
    private IntegerField priorityField;
    private FloatField delayField;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Open(DistributionPanel distributionPanel)
    {
        if (distributionPanel == null || panel == null) return;

        target = distributionPanel;

        // The UIDocument builds its elements when it is enabled
        panel.SetActive(true);
        InterfaceManager.GetInstance().SetInterface(panel, true);
        open = true;

        BindElements(panel.GetComponent<UIDocument>().rootVisualElement);
        LoadSettings();
        Refresh();
    }

    private void Close()
    {
        open = false;
        target = null;

        if (panel == null) return;

        if (panel.activeSelf)
        {
            panel.SetActive(false);
        }

        InterfaceManager.GetInstance().SetInterface(panel, false);
    }

    // Once per UIDocument tree: its elements are built again each time the panel is enabled
    private void BindElements(VisualElement root)
    {
        VisualElement panelElement = root.Q("panel");

        if (panelElement == null || panelElement == boundPanel) return;

        boundPanel = panelElement;

        title = root.Q<Label>("title");
        stateLabel = root.Q<Label>("state-label");
        powerLabel = root.Q<Label>("power-label");
        energyLabel = root.Q<Label>("energy-label");
        switchButton = root.Q<Button>("switch-button");
        limitField = root.Q<FloatField>("limit-field");
        limitInfinite = root.Q<Toggle>("limit-infinite");
        sheddingField = root.Q<DropdownField>("shedding-field");
        thresholdField = root.Q<Slider>("threshold-field");
        priorityField = root.Q<IntegerField>("priority-field");
        delayField = root.Q<FloatField>("delay-field");

        if (sheddingField != null)
        {
            sheddingField.choices = SheddingChoices;
            sheddingField.RegisterValueChangedCallback(evt =>
            {
                if (target == null) return;

                target.SetShedding((DistributionPanel.SheddingMode)Mathf.Max(0, SheddingChoices.IndexOf(evt.newValue)));
                UpdateVisibility();
            });
        }

        if (switchButton != null)
        {
            switchButton.clicked += () =>
            {
                if (target == null) return;

                target.SetSwitchedOn(!target.SwitchedOn);
                Refresh();
            };
        }

        limitField?.RegisterValueChangedCallback(evt => ApplyLimit());
        limitInfinite?.RegisterValueChangedCallback(evt =>
        {
            ApplyLimit();
            UpdateVisibility();
        });

        // Not "?." on the target: a destroyed panel is not seen as null by that operator
        thresholdField?.RegisterValueChangedCallback(evt => { if (target != null) target.SetBatteryThreshold(evt.newValue); });
        priorityField?.RegisterValueChangedCallback(evt => { if (target != null) target.SetPriority(evt.newValue); });
        delayField?.RegisterValueChangedCallback(evt => { if (target != null) target.SetStartDelay(evt.newValue); });

        Button closeButton = root.Q<Button>("close-button");

        if (closeButton != null)
        {
            closeButton.clicked += Close;
        }
    }

    private void ApplyLimit()
    {
        if (target == null || limitField == null || limitInfinite == null) return;

        target.SetPowerLimit(limitField.value, limitInfinite.value);
    }

    // Settings of the panel into the fields, without calling their callbacks
    private void LoadSettings()
    {
        if (target == null) return;

        if (title != null) title.text = target.DisplayName;

        limitField?.SetValueWithoutNotify(target.PowerLimit.value);
        limitInfinite?.SetValueWithoutNotify(target.PowerLimit.infinite);
        sheddingField?.SetValueWithoutNotify(SheddingChoices[(int)target.Shedding]);
        thresholdField?.SetValueWithoutNotify(target.BatteryThreshold);
        priorityField?.SetValueWithoutNotify(target.Priority);
        delayField?.SetValueWithoutNotify(target.StartDelay);

        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (target == null) return;

        limitField?.SetEnabled(!target.PowerLimit.infinite);
        thresholdField?.EnableInClassList("hidden", target.Shedding != DistributionPanel.SheddingMode.BatteryBelow);
    }

    private void Refresh()
    {
        if (target == null) return;

        if (stateLabel != null) stateLabel.text = "État : " + target.StateText;
        if (powerLabel != null) powerLabel.text = "Demande du circuit : " + EnergyValue.Format(target.Request, "W")
            + "   ·   Transmis : " + EnergyValue.Format(target.Supplied, "W")
            + "   ·   Recharge : " + EnergyValue.Format(target.ChargeSupplied, "W");
        if (energyLabel != null) energyLabel.text = "Énergie transmise : " + EnergyValue.Format(target.ConsumedEnergy, "Wh");
        if (switchButton != null) switchButton.text = target.SwitchedOn ? "Couper" : "Rétablir";
    }

    private void Update()
    {
        if (!open) return;

        // Closed by InterfaceManager.CloseInterfaces (Escape, death), or the panel was destroyed
        if (!panel.activeSelf || target == null)
        {
            Close();
            return;
        }

        refreshTimer -= Time.unscaledDeltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            Refresh();
        }
    }
}
