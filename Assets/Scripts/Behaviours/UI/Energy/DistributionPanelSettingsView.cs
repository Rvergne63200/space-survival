using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// State and settings of a distribution panel (DistributionPanelSettings.uxml): state, power and energy of its circuit,
// on/off, power limit, load shedding, priority and delayed start. Used by its own interface (DistributionPanelUI) and
// by the map of the control panel (ControlPanelUI)
public class DistributionPanelSettingsView
{
    private static readonly List<string> SheddingChoices = new List<string>
    {
        "Aucun",
        "Manque d'énergie en amont",
        "Batteries en amont sous le seuil"
    };

    private DistributionPanel target;

    private VisualElement boundRoot;
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

    public DistributionPanel Target => target;

    // Once per tree: the elements of a UIDocument are built again each time it is enabled
    public void Bind(VisualElement root)
    {
        if (root == null || root == boundRoot) return;

        boundRoot = root;

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
    }

    public void SetTarget(DistributionPanel panel)
    {
        target = panel;
        LoadSettings();
        Refresh();
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

    public void Refresh()
    {
        if (target == null) return;

        if (stateLabel != null) stateLabel.text = "État : " + target.StateText;
        // A building: what it uses itself (its inside is shared with the buildings linked by corridors)
        string demand = target is BuildingPowerPanel building && EnergyNetworkManager.HasInstance
            ? "Consommation du bâtiment : " + EnergyValue.Format(EnergyNetworkManager.Instance.BuildingConsumption(building), "W")
            : "Demande du circuit : " + EnergyValue.Format(target.Request, "W");

        if (powerLabel != null) powerLabel.text = demand
            + "   ·   Transmis : " + EnergyValue.Format(target.Supplied, "W")
            + "   ·   Recharge : " + EnergyValue.Format(target.BatteryChargeSupplied, "W")
            + "   ·   Sortie : " + EnergyValue.Format(target.OutletSupplied, "W");
        if (energyLabel != null) energyLabel.text = "Énergie transmise : " + EnergyValue.Format(target.ConsumedEnergy, "Wh");
        if (switchButton != null)
        {
            // Held off by a panel above it: its setting cannot be changed meanwhile
            switchButton.SetEnabled(!target.ForcedOff);
            switchButton.text = target.ForcedOff ? "Éteint par le tableau du dessus" : target.SwitchedOn ? "Éteindre" : "Allumer";
        }
    }
}
