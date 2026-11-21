using System.Collections.Generic;
using UnityEngine;

// An item placed in the world as an object (crate, furniture...): while it is in hand, a ghost shows where it will be
// placed (Builder). The placed object can be destroyed from the build wheel, which gives back the refund
[CreateAssetMenu(menuName = "Item/Placeable")]
public class PlaceableItemData : ItemData
{
    [SerializeField] private GameObject _placedPrefab;
    public GameObject PlacedPrefab => _placedPrefab;

    // Given back when the placed object is destroyed. Empty: the item itself
    [SerializeField] private List<RecipeData.ItemBlock> _refund = new List<RecipeData.ItemBlock>();
    public List<RecipeData.ItemBlock> Refund => _refund;

    // Placed outside only (solar panel, wind turbine): not inside a dome, an airlock or a corridor (Builder)
    [SerializeField] private bool _outdoorOnly;
    public bool OutdoorOnly => _outdoorOnly;
}
