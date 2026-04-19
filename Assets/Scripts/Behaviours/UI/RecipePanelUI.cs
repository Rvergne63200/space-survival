using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipePanelUI : MonoBehaviour
{
    [SerializeField] GameObject recipeItemCountPrefab;

    [SerializeField] GameObject IngredientsParent;
    [SerializeField] Image recipeResultImage;
    [SerializeField] TextMeshProUGUI recipeResultCount;

    private RecipeData recipe;
    private Crafter crafter;

    public void SetCrafter(Crafter crafter)
    {
        this.crafter = crafter;
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

        foreach(RecipeData.ItemBlock ingredient in data.Ingredients)
        {
            GameObject ingredientUI = Instantiate(recipeItemCountPrefab, IngredientsParent.transform);
            ingredientUI.GetComponentInChildren<Image>().sprite = ingredient.Item.Sprite;
            ingredientUI.GetComponentInChildren<TextMeshProUGUI>().text = ingredient.Quantity.ToString();
        }
    }

    public void OnCraft()
    {
        crafter.Craft(recipe);
    }
}
