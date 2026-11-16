using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipePanelUI : MonoBehaviour
{
    [SerializeField] GameObject recipeItemCountPrefab;

    [SerializeField] GameObject IngredientsParent;
    [SerializeField] Image recipeResultImage;
    [SerializeField] TextMeshProUGUI recipeResultCount;

    // A recipe that cannot be crafted is shown with this opacity
    [SerializeField] float unavailableAlpha = 0.45f;
    [SerializeField] Color missingColor = new Color(1f, 0.4f, 0.4f);

    // The inventory can change without event (slot transfers): the counts are refreshed periodically
    [SerializeField] float refreshInterval = 0.25f;

    private RecipeData recipe;
    private Crafter crafter;

    private readonly List<TextMeshProUGUI> ingredientCounts = new List<TextMeshProUGUI>();
    private Color defaultCountColor = Color.white;
    private CanvasGroup canvasGroup;
    private Button craftButton;
    private float refreshTimer;

    // Not in Awake: the panels are created while the crafting interface is still inactive (Awake not called yet)
    private void EnsureComponents()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (craftButton == null)
        {
            craftButton = GetComponentInChildren<Button>(true);
        }
    }

    public void SetCrafter(Crafter crafter)
    {
        this.crafter = crafter;
        Refresh();
    }

    public void SetRecipe(RecipeData data)
    {
        recipe = data;

        recipeResultImage.sprite = data.Results[0].Item.Sprite;
        recipeResultCount.text = data.Results[0].Quantity.ToString();

        for (int i = 0; i < IngredientsParent.transform.childCount; i++)
        {
            Destroy(IngredientsParent.transform.GetChild(i).gameObject);
        }

        ingredientCounts.Clear();

        foreach(RecipeData.ItemBlock ingredient in data.Ingredients)
        {
            GameObject ingredientUI = Instantiate(recipeItemCountPrefab, IngredientsParent.transform);
            ingredientUI.GetComponentInChildren<Image>().sprite = ingredient.Item.Sprite;

            TextMeshProUGUI count = ingredientUI.GetComponentInChildren<TextMeshProUGUI>();
            defaultCountColor = count.color;
            ingredientCounts.Add(count);
        }

        Refresh();
    }

    private void Update()
    {
        refreshTimer -= Time.unscaledDeltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            Refresh();
        }
    }

    // Owned / required for each ingredient (in red when missing), and the recipe greyed out when it cannot be crafted
    private void Refresh()
    {
        if (recipe == null || PlayerManager.Instance == null) return;

        EnsureComponents();

        PlayerInventory inventory = PlayerManager.Instance.Inventory;

        for (int i = 0; i < recipe.Ingredients.Length && i < ingredientCounts.Count; i++)
        {
            RecipeData.ItemBlock ingredient = recipe.Ingredients[i];
            int owned = Mathf.Max(0, inventory.Count(ingredient.Item));

            ingredientCounts[i].text = owned + "/" + ingredient.Quantity;
            ingredientCounts[i].color = owned >= ingredient.Quantity ? defaultCountColor : missingColor;
        }

        bool available = crafter != null && crafter.CheckHasItems(recipe);

        canvasGroup.alpha = available ? 1f : unavailableAlpha;

        if (craftButton != null)
        {
            craftButton.interactable = available;
        }
    }

    public void OnCraft()
    {
        crafter.Craft(recipe);
        Refresh();
    }
}
