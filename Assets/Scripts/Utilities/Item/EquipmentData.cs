using System;
using UnityEngine;


// Where an equipment is worn. To review with the equipment system
public enum EquipmentSlot
{
    Head,
    Body,
    Back
}

// A usable item that the player equips (equipment system to come). How its bar goes down is not defined yet
[CreateAssetMenu(menuName = "Item/Equipment")]
[Serializable]
public class EquipmentData : UsableItemData
{
    [SerializeField]
    private EquipmentSlot _slot = EquipmentSlot.Body;
    public EquipmentSlot Slot => _slot;
}
