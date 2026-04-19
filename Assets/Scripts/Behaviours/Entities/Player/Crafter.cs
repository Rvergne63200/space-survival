using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Crafter : MonoBehaviour
{
    private CraftingStation station = null;

    public UnityEvent<CraftingStation> ev_UpdateCraftingStation;

    public void Check(CraftingStation station)
    {
        this.station = station;
        ev_UpdateCraftingStation.Invoke(station);
    }
    
    public void Craft(RecipeData recipe)
    {
        PlayerInventory inventory = GetComponent<PlayerContext>().PlayerManager.Inventory;

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

        // Add crafted item in the inventory
        foreach (RecipeData.ItemBlock result in recipe.Results)
        {
            int rest = inventory.Add(Item.CreateFromData(result.Item), result.Quantity);
            // TODO : Penser à gérer le cas ou il n'y a plus de places dans l'inventaire
        }
    }

    public bool CheckHasItems(RecipeData recipe)
    {
        PlayerInventory inventory = GetComponent<PlayerContext>().PlayerManager.Inventory;

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

        // Check inventory quantities
        foreach (KeyValuePair<ItemData, int> recipeQuantity in recipeIngredients)
        {
            if (recipeQuantity.Value > inventory.Content.Count(recipeQuantity.Key))
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
