using UnityEngine;

public class CraftingStation : MonoBehaviour, IInterractable
{
    [SerializeField] private RecipeData[] _recipes;
    public RecipeData[] Recipes { get => _recipes; }

    public string GetInfo()
    {
        return "Craft an item";
    }

    public void Interract(GameObject interractor)
    {
        Crafter crafter = interractor.GetComponent<Crafter>();

        if (crafter != null)
        {
            crafter.Check(this);
        }
    }
}
