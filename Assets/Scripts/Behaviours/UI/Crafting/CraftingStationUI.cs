using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Crafting station used by the player (CraftingStation.uxml): list of its recipes (one RecipeSlot.uxml each) and
// detail of the selected one: name, description, ingredients (owned / required), quantity to craft and craft button.
// Above: state of the station (power), button to switch it on or off, crafts in progress and button to cancel them
// (CraftingStation). Nothing can be crafted while it is not usable (off or not powered).
// "Craftable only" hides the recipes that cannot be crafted. The panel (GameObject holding the UIDocument) is
// registered as an interface while it is open, and opens the player inventory with it
public class CraftingStationUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private VisualTreeAsset recipeTemplate;
    [SerializeField] private VisualTreeAsset ingredientTemplate;

    // The inventory can change without event (slot transfers): the counts are refreshed periodically
    [SerializeField] private float refreshInterval = 0.25f;

    private Crafter crafter;
    private CraftingStation station;
    private bool open;
    private float refreshTimer;

    // Kept between two openings
    private bool craftableOnly;

    private readonly List<RecipeSlotUI> recipeSlots = new List<RecipeSlotUI>();
    private RecipeSlotUI selected;
    private int quantity = 1;

    private VisualElement boundPanel;
    private ScrollView recipesList;
    private Label emptyLabel;
    private VisualElement detail;
    private VisualElement detailIcon;
    private Label detailName;
    private Label detailCount;
    private Label detailDescription;
    private VisualElement detailIngredients;
    private Label quantityLabel;
    private Button minusButton;
    private Button plusButton;
    private Button maxButton;
    private Button craftButton;
    private Label stationState;
    private Label stationQueue;
    private Button powerButton;
    private Button cancelButton;
    private readonly List<Label> ingredientCounts = new List<Label>();

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void SetCrafter(Crafter crafter)
    {
        this.crafter = crafter;
        Refresh();
    }

    public void SetCraftingStation(CraftingStation craftingStation)
    {
        if (craftingStation == null || panel == null)
        {
            Close();
            return;
        }

        // The UIDocument builds its elements when it is enabled
        panel.SetActive(true);
        InterfaceManager.GetInstance().SetInterface(panel, true);
        open = true;

        BindElements(panel.GetComponent<UIDocument>().rootVisualElement);

        station = craftingStation;
        recipeSlots.Clear();
        selected = null;
        quantity = 1;

        if (recipesList != null && recipeTemplate != null)
        {
            recipesList.Clear();

            foreach (RecipeData recipe in craftingStation.Recipes)
            {
                if (recipe == null) continue;

                VisualElement element = recipeTemplate.Instantiate().Q("recipe-slot");
                recipesList.Add(element);

                recipeSlots.Add(new RecipeSlotUI(element, recipe, Select));
            }
        }

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.Inventory.OpenWith(this);
        }

        Refresh();
    }

    // Once per UIDocument tree: its elements are built again each time the panel is enabled
    private void BindElements(VisualElement root)
    {
        VisualElement panelElement = root.Q("panel");

        if (panelElement == null || panelElement == boundPanel) return;

        boundPanel = panelElement;

        recipesList = root.Q<ScrollView>("recipes");
        emptyLabel = root.Q<Label>("empty-label");
        detail = root.Q("detail");
        detailIcon = root.Q("detail-icon");
        detailName = root.Q<Label>("detail-name");
        detailCount = root.Q<Label>("detail-count");
        detailDescription = root.Q<Label>("detail-description");
        detailIngredients = root.Q("detail-ingredients");
        quantityLabel = root.Q<Label>("quantity-label");
        minusButton = root.Q<Button>("quantity-minus");
        plusButton = root.Q<Button>("quantity-plus");
        maxButton = root.Q<Button>("quantity-max");
        craftButton = root.Q<Button>("craft-button");
        stationState = root.Q<Label>("station-state");
        stationQueue = root.Q<Label>("station-queue");
        powerButton = root.Q<Button>("station-power");
        cancelButton = root.Q<Button>("station-cancel");

        if (powerButton != null) powerButton.clicked += () => { if (station != null) station.SetSwitchedOn(!station.Power.SwitchedOn); Refresh(); };
        if (cancelButton != null) cancelButton.clicked += () => { if (station != null) station.CancelQueue(); Refresh(); };

        if (minusButton != null) minusButton.clicked += () => SetQuantity(quantity - 1);
        if (plusButton != null) plusButton.clicked += () => SetQuantity(quantity + 1);
        if (maxButton != null) maxButton.clicked += () => SetQuantity(MaxCraftable());
        if (craftButton != null) craftButton.clicked += OnCraft;

        Toggle filter = root.Q<Toggle>("craftable-only");

        if (filter != null)
        {
            filter.SetValueWithoutNotify(craftableOnly);
            filter.RegisterValueChangedCallback(evt =>
            {
                craftableOnly = evt.newValue;
                Refresh();
            });
        }
    }

    private void Close()
    {
        bool wasOpen = open;

        recipeSlots.Clear();
        selected = null;
        open = false;

        if (wasOpen && PlayerManager.Instance != null)
        {
            PlayerManager.Instance.Inventory.CloseWith(this);
        }

        if (panel == null) return;

        if (panel.activeSelf)
        {
            panel.SetActive(false);
        }

        InterfaceManager.GetInstance().SetInterface(panel, false);
    }

    private void Select(RecipeSlotUI slot)
    {
        if (selected == slot) return;

        selected = slot;
        quantity = 1;

        BuildIngredients();
        Refresh();
    }

    private void BuildIngredients()
    {
        ingredientCounts.Clear();

        if (detailIngredients == null) return;

        detailIngredients.Clear();

        if (selected == null || ingredientTemplate == null) return;

        foreach (RecipeData.ItemBlock ingredient in selected.Recipe.Ingredients)
        {
            VisualElement element = ingredientTemplate.Instantiate().Q("ingredient");
            detailIngredients.Add(element);

            VisualElement icon = element.Q("ingredient-icon");

            if (icon != null && ingredient.Item != null && ingredient.Item.Sprite != null)
            {
                icon.style.backgroundImage = new StyleBackground(ingredient.Item.Sprite);
            }

            Label name = element.Q<Label>("ingredient-name");

            if (name != null)
            {
                name.text = ingredient.Item != null ? ingredient.Item.Name : "?";
            }

            ingredientCounts.Add(element.Q<Label>("ingredient-count"));
        }
    }

    private int MaxCraftable()
    {
        return crafter != null && selected != null ? crafter.MaxCraftable(selected.Recipe) : 0;
    }

    private void SetQuantity(int value)
    {
        quantity = Mathf.Clamp(value, 1, Mathf.Max(1, MaxCraftable()));
        Refresh();
    }

    private void Refresh()
    {
        if (!open) return;

        RefreshStation();

        // The selected recipe stays visible, unless the filter hides it: the first visible one is selected instead
        bool hasVisible = false;
        RecipeSlotUI firstVisible = null;

        foreach (RecipeSlotUI slot in recipeSlots)
        {
            bool available = crafter != null && crafter.CheckHasItems(slot.Recipe);
            bool visible = !craftableOnly || available;

            slot.SetState(available, slot == selected, visible);

            if (visible)
            {
                hasVisible = true;
                firstVisible ??= slot;
            }
        }

        if (selected == null || (craftableOnly && (crafter == null || !crafter.CheckHasItems(selected.Recipe))))
        {
            if (firstVisible != selected)
            {
                // Refreshed again by Select
                if (firstVisible != null)
                {
                    Select(firstVisible);
                    return;
                }

                selected = null;
                BuildIngredients();
            }
        }

        emptyLabel?.EnableInClassList("hidden", hasVisible);
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        if (detail == null) return;

        detail.style.visibility = selected != null ? Visibility.Visible : Visibility.Hidden;

        if (selected == null) return;

        RecipeData recipe = selected.Recipe;
        ItemData result = recipe.Results.Length > 0 ? recipe.Results[0].Item : null;
        int resultQuantity = recipe.Results.Length > 0 ? recipe.Results[0].Quantity : 0;

        int max = MaxCraftable();
        quantity = Mathf.Clamp(quantity, 1, Mathf.Max(1, max));

        if (detailIcon != null)
        {
            detailIcon.style.backgroundImage = result != null && result.Sprite != null ? new StyleBackground(result.Sprite) : new StyleBackground(StyleKeyword.None);
        }

        if (detailName != null) detailName.text = result != null ? result.Name : "?";
        if (detailCount != null) detailCount.text = "x" + (resultQuantity * quantity);

        if (detailDescription != null)
        {
            string description = result != null ? result.Description : "";
            detailDescription.text = description;
            detailDescription.style.display = string.IsNullOrEmpty(description) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        PlayerInventory inventory = PlayerManager.Instance != null ? PlayerManager.Instance.Inventory : null;

        for (int i = 0; i < recipe.Ingredients.Length && i < ingredientCounts.Count; i++)
        {
            Label count = ingredientCounts[i];

            if (count == null || inventory == null) continue;

            RecipeData.ItemBlock ingredient = recipe.Ingredients[i];
            int owned = Mathf.Max(0, inventory.Count(ingredient.Item));
            int required = ingredient.Quantity * quantity;

            count.text = owned + "/" + required;
            count.EnableInClassList("recipe-missing", owned < required);
        }

        if (quantityLabel != null) quantityLabel.text = quantity.ToString();

        minusButton?.SetEnabled(quantity > 1);
        plusButton?.SetEnabled(quantity < max);
        maxButton?.SetEnabled(max > 1 && quantity < max);
        craftButton?.SetEnabled(max >= quantity && max > 0 && station != null && station.IsUsable);
    }

    // State of the station (power, queue) and its buttons
    private void RefreshStation()
    {
        if (station == null) return;

        EnergyConsumer power = station.Power;
        string state;

        if (power.ForcedOff) state = "Éteinte par le tableau";
        else if (!power.SwitchedOn) state = "Éteinte";
        else if (power.IsPowered) state = "Alimentée";
        else if (power.State == EnergyNetworkManager.NetworkState.Tripped) state = "Réseau disjoncté";
        else state = "Non alimentée";

        if (stationState != null) stationState.text = state + " (" + EnergyValue.Format(power.Consumption, "W") + ")";

        if (stationQueue != null)
        {
            RecipeData current = station.Current;
            string text = current == null ? "Aucune fabrication en cours" : "En cours : " + RecipeName(current) + " (" + Mathf.RoundToInt(station.Progress * 100f) + " %)"
                + (station.Queue.Count > 1 ? ", puis " + (station.Queue.Count - 1) + " en attente" : "");

            if (!station.Output.IsEmpty()) text += "   ·   Sur la station : " + station.Output.Quantity + "x " + station.Output.Item.Data.Name + " [F]";

            stationQueue.text = text;
        }

        if (powerButton != null)
        {
            powerButton.text = power.SwitchedOn ? "Éteindre" : "Allumer";
            powerButton.SetEnabled(!power.ForcedOff);
        }

        cancelButton?.SetEnabled(station.Queue.Count > 0);
    }

    private static string RecipeName(RecipeData recipe)
    {
        return recipe.Results.Length > 0 && recipe.Results[0].Item != null ? recipe.Results[0].Item.Name : recipe.name;
    }

    private void OnCraft()
    {
        if (crafter == null || selected == null) return;

        for (int i = 0; i < quantity; i++)
        {
            if (!crafter.CheckHasItems(selected.Recipe)) break;

            crafter.Craft(selected.Recipe);
        }

        quantity = 1;
        Refresh();
    }

    private void Update()
    {
        if (!open) return;

        // Closed by InterfaceManager.CloseInterfaces (Escape, death): release the station,
        // like the Searcher does for the searched storage
        if (!panel.activeSelf)
        {
            Close();

            if (crafter != null)
            {
                crafter.ClearStation();
            }

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
