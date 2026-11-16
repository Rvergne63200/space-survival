using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Crafter : MonoBehaviour
{
    private CraftingStation station = null;

    public UnityEvent<CraftingStation> ev_UpdateCraftingStation;

    private PlayerContext playerContext;

    private void Awake()
    {
        playerContext = GetComponent<PlayerContext>();
    }

    public void Check(CraftingStation station)
    {
        this.station = station;
        ev_UpdateCraftingStation.Invoke(station);
    }
    
    public void Craft(RecipeData recipe)
    {
        PlayerInventory inventory = playerContext.PlayerManager.Inventory;

        if (station == null)
        {
            Debug.LogError("Invalid crafting station");
            return;
        }

        if (!station.Recipes.Contains(recipe))
        {
            Debug.LogError("Cannot craft this recipe in this station");
            return;
        }

        if (!CheckHasItems(recipe))
        {
            Debug.Log("Impossible de fabriquer cet objet. Ressources manquantes");
            return;
        }

        // Remove recipe ingredients from inventory
        foreach (RecipeData.ItemBlock ingredient in recipe.Ingredients)
        {
            inventory.Remove(Item.CreateFromData(ingredient.Item), ingredient.Quantity);
        }

        // Add crafted item in the inventory, and drop on the ground what does not fit
        foreach (RecipeData.ItemBlock result in recipe.Results)
        {
            Item item = Item.CreateFromData(result.Item);
            int rest = inventory.Add(item, result.Quantity);

            if (rest > 0)
            {
                Pickupable.Spawn(item, rest, transform);
            }
        }
    }

    public bool CheckHasItems(RecipeData recipe)
    {
        PlayerInventory inventory = playerContext.PlayerManager.Inventory;

        // Stack item data count
        Dictionary<ItemData, int> recipeIngredients = new Dictionary<ItemData, int>();
        foreach (var block in recipe.Ingredients)
        {
            if (recipeIngredients.ContainsKey(block.Item))
            {
                recipeIngredients[block.Item] += block.Quantity;
            }
            else
            {
                recipeIngredients[block.Item] = block.Quantity;
            }
        }

        // Check inventory quantities (content and tool bar, like Remove)
        foreach (KeyValuePair<ItemData, int> recipeQuantity in recipeIngredients)
        {
            if (recipeQuantity.Value > inventory.Count(recipeQuantity.Key))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearStation()
    {
        station = null;
        ev_UpdateCraftingStation.Invoke(station);
    }
}
