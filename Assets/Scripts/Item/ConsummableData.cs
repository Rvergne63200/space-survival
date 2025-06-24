using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Item/Consummable")]
[Serializable]
public class ConsummableData : ItemData
{
    [SerializeReference, SubclassPicker]
    public List<AbstractModifier> modifiers;

    public void Consume(GameObject consummer)
    {
        foreach (AbstractModifier modifier in modifiers)
        {
            modifier.Modify(consummer);
        }
    }
}
