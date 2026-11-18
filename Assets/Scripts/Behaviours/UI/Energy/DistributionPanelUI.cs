using UnityEngine;
using UnityEngine.UIElements;

// Interface of a distribution panel (DistributionPanel.uxml), opened by interacting with it: its state and settings
// (DistributionPanelSettingsView). The panel (GameObject holding the UIDocument) is registered as an interface while
// it is open, Escape closes it
public class DistributionPanelUI : MonoBehaviour
{
    public static DistributionPanelUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private float refreshInterval = 0.25f;

    private readonly DistributionPanelSettingsView settings = new DistributionPanelSettingsView();

    private DistributionPanel target;
    private bool open;
    private float refreshTimer;

    private VisualElement boundPanel;
    private Label title;

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

        if (title != null) title.text = target.DisplayName;

        settings.SetTarget(target);
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
        settings.Bind(panelElement);

        Button closeButton = root.Q<Button>("close-button");

        if (closeButton != null)
        {
            closeButton.clicked += Close;
        }
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
            settings.Refresh();
        }
    }
}
