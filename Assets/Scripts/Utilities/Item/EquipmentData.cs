using System;
using UnityEngine;


// Where an equipment is worn (PlayerInventory.Equipment). Oxygen tanks have their own slots, apart from the back, so
// that a jetpack and oxygen can be worn together
public enum EquipmentSlot
{
    Head,
    Body,
    Back,
    Feet,
    Hands,
    OxygenTank
}

// A usable item that the player wears in an equipment slot (inventory, Tab or I). How its bar goes down depends on the
// equipment (JetpackData: while flying)
[CreateAssetMenu(menuName = "Item/Equipment")]
[Serializable]
public class EquipmentData : UsableItemData
{
    [SerializeField]
    private EquipmentSlot _slot = EquipmentSlot.Body;
    public EquipmentSlot Slot => _slot;
}
