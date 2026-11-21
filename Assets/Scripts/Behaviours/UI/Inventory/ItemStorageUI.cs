using System;
using UnityEngine;
using UnityEngine.UIElements;

// Storage searched by the player (Storage.uxml): chest, death bag... (Searcher.ev_UpdateSearchStorage, see UIManager).
// Opens the player inventory with it.
// The storage of a machine (IStorageMachine on the same object: coal generator) is its interface: its name as the title,
// and above the slots its state, a gauge and its power button (refreshed each frame)
public class ItemStorageUI : ItemCollectionUI
{
    private const string DefaultTitle = "Conteneur";

    private IStorageMachine machine;

    private VisualElement boundRoot;
    private Label title;
    private VisualElement machineBar;
    private Label machineState;
    private VisualElement machineGaugeFill;
    private Label machineGaugeText;
    private Button machinePower;

    public void SetItemStorage(ItemStorage storage)
    {
        PlayerInventory inventory = parentUI != null && parentUI.UIManager != null ? parentUI.UIManager.PlayerInventory : null;

        if (storage != null && storage.Content != null)
        {
            OpenPanel(storage.Content);
            if (inventory != null) inventory.OpenWith(this);

            machine = storage.GetComponent<IStorageMachine>();
            BindMachine();
            RefreshMachine();
        }
        else
        {
            machine = null;
            ClosePanel();
            if (inventory != null) inventory.CloseWith(this);
        }
    }

    public void SetInterfaceClearAction(Action clearInterfaceAction)
    {
        ac_OnClearInterface = clearInterfaceAction;
    }

    // The UIDocument builds its elements again each time the panel is enabled
    private void BindMachine()
    {
        UIDocument document = panel != null ? panel.GetComponent<UIDocument>() : null;
        VisualElement root = document != null ? document.rootVisualElement : null;

        if (root == null || root == boundRoot) return;

        boundRoot = root;
        title = root.Q<Label>("title");
        machineBar = root.Q("machine");
        machineState = root.Q<Label>("machine-state");
        machineGaugeFill = root.Q("machine-gauge-fill");
        machineGaugeText = root.Q<Label>("machine-gauge-text");
        machinePower = root.Q<Button>("machine-power");

        if (machinePower != null)
        {
            machinePower.clicked += () =>
            {
                if (machine != null && machine.CanSwitchMachine) machine.SetMachineOn(!machine.MachineOn);
                RefreshMachine();
            };
        }
    }

    protected override void Update()
    {
        base.Update();

        if (machine != null && panel != null && panel.activeSelf) RefreshMachine();
    }

    private void RefreshMachine()
    {
        if (title != null) title.text = machine != null ? machine.MachineName : DefaultTitle;

        machineBar?.EnableInClassList("hidden", machine == null);

        if (machine == null) return;

        if (machineState != null) machineState.text = machine.MachineState;
        if (machineGaugeFill != null) machineGaugeFill.style.width = Length.Percent(machine.Gauge * 100f);
        if (machineGaugeText != null) machineGaugeText.text = machine.GaugeText;

        if (machinePower != null)
        {
            machinePower.text = machine.MachineOn ? "Éteindre" : "Allumer";
            machinePower.SetEnabled(machine.CanSwitchMachine);
        }
    }
}
