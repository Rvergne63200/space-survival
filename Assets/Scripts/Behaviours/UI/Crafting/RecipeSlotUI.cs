using System;
using UnityEngine;
using UnityEngine.UIElements;

// One recipe of the list of the crafting station (element "recipe-slot" of RecipeSlot.uxml), created by
// CraftingStationUI: icon and quantity of its first result. A click selects it (its detail is shown)
public class RecipeSlotUI
{
    public RecipeData Recipe { get; }
    public VisualElement Element { get; }

    public RecipeSlotUI(VisualElement element, RecipeData recipe, Action<RecipeSlotUI> onSelect)
    {
        Element = element;
        Recipe = recipe;

        VisualElement icon = element.Q("recipe-slot-icon");
        Label count = element.Q<Label>("recipe-slot-count");

        ItemData result = recipe.Results.Length > 0 ? recipe.Results[0].Item : null;
        int quantity = recipe.Results.Length > 0 ? recipe.Results[0].Quantity : 0;

        if (icon != null && result != null && result.Sprite != null)
        {
            icon.style.backgroundImage = new StyleBackground(result.Sprite);
        }

        if (count != null)
        {
            count.text = quantity.ToString();
            count.style.display = quantity > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (element is Button button)
        {
            button.clicked += () => onSelect(this);
        }
    }

    public void SetState(bool available, bool selected, bool visible)
    {
        Element.EnableInClassList("recipe-unavailable", !available);
        Element.EnableInClassList("recipe-slot-selected", selected);
        Element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
