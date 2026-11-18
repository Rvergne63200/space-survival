using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Interface of a control panel (ControlPanel.uxml), opened by interacting with it while it is powered: the map of its
// network (NetworkMapElement) and, for the device selected on it, its information and on/off button, or all the
// settings of a distribution panel (DistributionPanelSettingsView). Closed when the control panel loses power.
// The panel (GameObject holding the UIDocument) is registered as an interface while it is open, Escape closes it
public class ControlPanelUI : MonoBehaviour
{
    public static ControlPanelUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private float refreshInterval = 0.25f;

    private readonly DistributionPanelSettingsView settings = new DistributionPanelSettingsView();
    private readonly List<EnergyDevice> devices = new List<EnergyDevice>();
    private readonly List<ElectricCable> cables = new List<ElectricCable>();
    private readonly List<Rect> buildings = new List<Rect>();

    private ControlPanel target;
    private bool open;
    private float refreshTimer;

    private VisualElement boundPanel;
    private Label title;
    private NetworkMapElement map;
    private Label detailEmpty;
    private VisualElement deviceDetail;
    private Label deviceName;
    private Label deviceInfo;
    private Button deviceSwitch;
    private VisualElement panelDetail;
    private Label panelName;

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

    public void Open(ControlPanel controlPanel)
    {
        if (controlPanel == null || panel == null) return;

        target = controlPanel;

        // The UIDocument builds its elements when it is enabled
        panel.SetActive(true);
        InterfaceManager.GetInstance().SetInterface(panel, true);
        open = true;

        BindElements(panel.GetComponent<UIDocument>().rootVisualElement);

        if (title != null) title.text = target.DisplayName;

        CollectBuildings();
        RefreshNetwork();

        if (map != null)
        {
            map.Select(null);

            // Once the map has its size
            map.schedule.Execute(() => map.Fit());
        }
    }

    private void Close()
    {
        open = false;
        target = null;
        settings.SetTarget(null);

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
        detailEmpty = root.Q<Label>("detail-empty");
        deviceDetail = root.Q("device-detail");
        deviceName = root.Q<Label>("device-name");
        deviceInfo = root.Q<Label>("device-info");
        deviceSwitch = root.Q<Button>("device-switch");
        panelDetail = root.Q("panel-detail");
        panelName = root.Q<Label>("panel-name");

        settings.Bind(panelDetail);

        VisualElement mapContainer = root.Q("map");

        if (mapContainer != null)
        {
            map = new NetworkMapElement();
            map.Selected += device => ShowDetail();
            mapContainer.Add(map);
        }

        if (deviceSwitch != null)
        {
            deviceSwitch.clicked += () =>
            {
                EnergyDevice device = map != null ? map.SelectedDevice : null;

                if (device == null) return;

                device.SetSwitchedOn(!device.SwitchedOn);
                RefreshDetail();
            };
        }

        Button fitButton = root.Q<Button>("fit-button");

        if (fitButton != null)
        {
            fitButton.clicked += () => { if (map != null) map.Fit(); };
        }

        Button closeButton = root.Q<Button>("close-button");

        if (closeButton != null)
        {
            closeButton.clicked += Close;
        }
    }

    // Outlines of the buildings (bounds of their renderers, seen from above)
    private void CollectBuildings()
    {
        buildings.Clear();

        foreach (BuiltStructure structure in FindObjectsByType<BuiltStructure>(FindObjectsSortMode.None))
        {
            bool any = false;
            Bounds bounds = default;

            foreach (Renderer renderer in structure.GetComponentsInChildren<Renderer>())
            {
                if (!any)
                {
                    bounds = renderer.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (any)
            {
                buildings.Add(Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z));
            }
        }
    }

    // Devices and cables of the network: they may have changed (cables placed, devices destroyed)
    private void RefreshNetwork()
    {
        if (target == null || map == null) return;

        target.CollectNetwork(devices, cables);
        map.SetNetwork(devices, cables, buildings, target.Consumer);
    }

    private void ShowDetail()
    {
        EnergyDevice device = map != null ? map.SelectedDevice : null;
        DistributionPanel distributionPanel = device != null ? device.Panel : null;

        detailEmpty?.EnableInClassList("hidden", device != null);
        deviceDetail?.EnableInClassList("hidden", device == null || distributionPanel != null);
        panelDetail?.EnableInClassList("hidden", distributionPanel == null);

        if (distributionPanel != null)
        {
            if (panelName != null) panelName.text = distributionPanel.DisplayName;
            settings.SetTarget(distributionPanel);
        }
        else
        {
            settings.SetTarget(null);
        }

        RefreshDetail();
    }

    private void RefreshDetail()
    {
        EnergyDevice device = map != null ? map.SelectedDevice : null;

        if (device == null) return;

        if (device.Panel != null)
        {
            settings.Refresh();
            return;
        }

        if (deviceName != null) deviceName.text = device.DisplayName;
        if (deviceInfo != null) deviceInfo.text = device.GetInfo();

        if (deviceSwitch != null)
        {
            // Held off by a distribution panel: its setting cannot be changed meanwhile
            deviceSwitch.EnableInClassList("hidden", !device.CanSwitch);
            deviceSwitch.SetEnabled(!device.ForcedOff);
            deviceSwitch.text = device.ForcedOff ? "Éteint par le tableau" : device.SwitchedOn ? "Éteindre" : "Allumer";
        }
    }

    private void Update()
    {
        if (!open) return;

        // Closed by InterfaceManager.CloseInterfaces (Escape, death), or the control panel was destroyed
        if (!panel.activeSelf || target == null)
        {
            Close();
            return;
        }

        if (!target.IsPowered)
        {
            NotificationUI.Show(target.DisplayName + " n'est plus alimenté.", NotificationType.Warning);
            Close();
            return;
        }

        if (map != null)
        {
            map.Tick();
        }

        refreshTimer -= Time.unscaledDeltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            RefreshNetwork();
            RefreshDetail();
        }
    }
}
