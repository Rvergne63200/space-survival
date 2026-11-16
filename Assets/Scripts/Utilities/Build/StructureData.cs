using System.Collections.Generic;
using UnityEngine;

// A structure the player can build from the build wheel: its plan is placed, then the resources of its cost are brought
// to it (StructureBlueprint), then it becomes real (prefab)
[CreateAssetMenu(menuName = "Construction/Structure")]
public class StructureData : ScriptableObject
{
    // Unique and stable: used by the saves ("build:<id>", "blueprint:<id>")
    [SerializeField] private string _id;
    public string Id => _id;

    [SerializeField] private string _name;
    public string Name => _name;

    [SerializeField] private Sprite _icon;
    public Sprite Icon => _icon;

    [SerializeField] private GameObject _prefab;
    public GameObject Prefab => _prefab;

    // Resources to bring to the plan
    [SerializeField] private List<RecipeData.ItemBlock> _cost = new List<RecipeData.ItemBlock>();
    public List<RecipeData.ItemBlock> Cost => _cost;

    // Resources given back when the finished structure is destroyed (any items, not necessarily the cost)
    [SerializeField] private List<RecipeData.ItemBlock> _refund = new List<RecipeData.ItemBlock>();
    public List<RecipeData.ItemBlock> Refund => _refund;

    // Flatten the terrain under the plan when it is placed
    [SerializeField] private bool _flattenTerrain = true;
    public bool FlattenTerrain => _flattenTerrain;

    // Maximum height difference of the terrain under the plan (meters)
    [SerializeField] private float _maxSlope = 2f;
    public float MaxSlope => _maxSlope;
}
