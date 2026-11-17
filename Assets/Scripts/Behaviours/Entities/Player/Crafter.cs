using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Crafter : MonoBehaviour
{
    private CraftingStation station = null;

    public UnityEvent<CraftingStation> ev_UpdateCraftingStation;

    private PlayerContext playerContext;

    // The crafting interface is closed beyond this distance from the station (meters), or when it is destroyed
    public float maxDistance = 4.5f;

    private void Awake()
    {
        playerContext = GetComponent<PlayerContext>();
    }

    private void Update()
    {
        if (station == null)
        {
            // Destroyed while open (Unity null): close its interface
            if (!ReferenceEquals(station, null))
            {
                ClearStation();
            }

            return;
        }

        if (InteractionDistance.To(station, transform.position) > maxDistance)
        {
            ClearStation();
        }
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
            NotificationUI.Show("Aucun poste de fabrication ouvert.", NotificationType.Warning);
            return;
        }

        if (!station.Recipes.Contains(recipe))
        {
            NotificationUI.Show("Cette recette ne se fabrique pas ici.", NotificationType.Warning);
            return;
        }

        if (!CheckHasItems(recipe))
        {
            NotificationUI.Show("Ressources manquantes.", NotificationType.Warning);
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
                NotificationUI.Show("Inventaire plein : " + result.Item.Name + " posé au sol.", NotificationType.Warning);
            }
        }
    }

    public bool CheckHasItems(RecipeData recipe)
    {
        return MaxCraftable(recipe) >= 1;
    }

    // Number of times the recipe can be crafted with the inventory (content and tool bar, like Remove)
    public int MaxCraftable(RecipeData recipe)
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

        int max = int.MaxValue;

        foreach (KeyValuePair<ItemData, int> recipeQuantity in recipeIngredients)
        {
            if (recipeQuantity.Value <= 0) continue;

            max = Mathf.Min(max, Mathf.Max(0, inventory.Count(recipeQuantity.Key)) / recipeQuantity.Value);
        }

        // A recipe without ingredient: no limit, one at a time
        return max == int.MaxValue ? 1 : max;
    }

    public void ClearStation()
    {
        station = null;
        ev_UpdateCraftingStation.Invoke(station);
    }
}
