using System.Collections.Generic;
using UnityEngine;

// Every structure of the game: shown in the build wheel, and found by id by the saves
[CreateAssetMenu(menuName = "Construction/Structure Catalog")]
public class StructureCatalog : ScriptableObject
{
    [SerializeField] private List<StructureData> _structures = new List<StructureData>();
    public List<StructureData> Structures => _structures;

    public StructureData Get(string id)
    {
        foreach (StructureData structure in _structures)
        {
            if (structure != null && structure.Id == id)
            {
                return structure;
            }
        }

        return null;
    }
}
