using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
[Serializable]
public class RecipeData : ScriptableObject
{
    [Serializable]
    public struct ItemBlock
    {
        [SerializeField] private ItemData _item;
        public ItemData Item { get => _item; }

        [SerializeField] private int _quantity;
        public int Quantity { get => _quantity; }
    }

    [SerializeField] private ItemBlock[] _ingredients;
    public ItemBlock[] Ingredients { get => _ingredients; protected set => _ingredients = value; }

    [SerializeField] private ItemBlock[] _results;
    public ItemBlock[] Results { get => _results; protected set => _results = value; }
}
