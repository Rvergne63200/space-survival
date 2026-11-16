using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

// Build wheel (Builder, hold the Build key): "Destroy" and the structures of the catalog, placed in a circle.
// Clicking an option selects it and closes the wheel, releasing the key closes it.
// Layout: BuildWheel.uxml and BuildWheelOption.uxml (one option), style in Menus.uss, editable in the UI Builder
public class BuildWheelUI : MonoBehaviour
{
    public static BuildWheelUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private VisualTreeAsset optionTemplate;
    [SerializeField] private StructureCatalog catalog;
    [SerializeField] private Sprite destroyIcon;

    // Distance between the center of the wheel and the center of the options, in pixels
    [SerializeField] private float radius = 170f;

    private Builder builder;
    private Label infoLabel;

    public bool IsOpen => panel != null && panel.activeSelf;

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

    public void Open(Builder builder)
    {
        if (panel == null || IsOpen) return;

        this.builder = builder;

        // The UIDocument builds its elements when it is enabled
        panel.SetActive(true);

        // Free cursor, no player control
        ModeManager.Instance.EnableMode(PlayingMode.Menu);

        VisualElement root = panel.GetComponent<UIDocument>().rootVisualElement;
        VisualElement wheel = root.Q("wheel");
        infoLabel = root.Q<Label>("wheel-info");

        if (wheel == null || optionTemplate == null)
        {
            Debug.LogWarning("Build wheel: no element named \"wheel\" in the UXML, or no option template.");
            return;
        }

        List<VisualElement> options = new List<VisualElement>();

        options.Add(CreateOption("Détruire", destroyIcon, "Détruire une construction ou un objet posé", () => builder.SelectDestroy()));

        if (catalog != null)
        {
            foreach (StructureData structure in catalog.Structures)
            {
                if (structure == null) continue;

                StructureData selected = structure;
                options.Add(CreateOption(structure.Name, structure.Icon, Describe(structure), () => builder.SelectStructure(selected)));
            }
        }

        // In a circle, the first option at the top
        for (int i = 0; i < options.Count; i++)
        {
            float angle = (i / (float)options.Count) * Mathf.PI * 2f;

            VisualElement option = options[i];
            option.style.position = Position.Absolute;
            option.style.left = new Length(50, LengthUnit.Percent);
            option.style.top = new Length(50, LengthUnit.Percent);
            option.style.translate = new Translate(new Length(Mathf.Sin(angle) * radius - 50f), new Length(-Mathf.Cos(angle) * radius - 50f));

            wheel.Add(option);
        }

        SetInfo("");
    }

    public void Close()
    {
        if (!IsOpen) return;

        panel.SetActive(false);
        ModeManager.Instance.DisableMode(PlayingMode.Menu);
        builder = null;
    }

    private VisualElement CreateOption(string name, Sprite icon, string info, System.Action action)
    {
        VisualElement option = optionTemplate.Instantiate();
        option.pickingMode = PickingMode.Ignore;

        Label label = option.Q<Label>("option-label");
        if (label != null)
        {
            label.text = name;
        }

        VisualElement iconElement = option.Q("option-icon");
        if (iconElement != null && icon != null)
        {
            iconElement.style.backgroundImage = new StyleBackground(icon);
        }

        Button button = option.Q<Button>("option-button");
        if (button != null)
        {
            button.clicked += () =>
            {
                Close();
                action();
            };

            button.RegisterCallback<PointerEnterEvent>(evt => SetInfo(info));
            button.RegisterCallback<PointerLeaveEvent>(evt => SetInfo(""));
        }

        return option;
    }

    private void SetInfo(string text)
    {
        if (infoLabel != null)
        {
            infoLabel.text = text;
        }
    }

    // Name and cost of a structure
    private static string Describe(StructureData structure)
    {
        StringBuilder text = new StringBuilder(structure.Name);

        for (int i = 0; i < structure.Cost.Count; i++)
        {
            RecipeData.ItemBlock cost = structure.Cost[i];
            text.Append(i == 0 ? "\n" : ", ").Append(cost.Quantity).Append(' ').Append(cost.Item != null ? cost.Item.Name : "?");
        }

        return text.ToString();
    }
}
