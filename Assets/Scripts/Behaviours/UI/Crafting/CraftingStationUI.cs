using UnityEngine;

public class CraftingStationUI : MonoBehaviour
{
    [SerializeField] GameObject recipePanelPrefab;

    private Crafter crafter;

    public void SetCrafter(Crafter crafter)
    {
        this.crafter = crafter;

        foreach(var recipePanelUI in transform.GetComponentsInChildren<RecipePanelUI>())
        {
            recipePanelUI.SetCrafter(crafter);
        }
    }

    public void SetCraftingStation(CraftingStation craftingStation)
    {
        bool display = craftingStation != null;


        if(display)
        {
            Clear();

            foreach (RecipeData recipe in craftingStation.Recipes)
            {
                AddRecipeUI(recipe);
            }

            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
            Clear();
        }

        InterfaceManager.GetInstance().SetInterface(gameObject, display);
    }

    private void Clear()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void AddRecipeUI(RecipeData data)
    {
        GameObject recipePanel = Instantiate(recipePanelPrefab, transform);
        RecipePanelUI recipePanelUI = recipePanel.GetComponent<RecipePanelUI>();

        recipePanelUI.SetRecipe(data);
        recipePanelUI.SetCrafter(crafter);
    }
}
