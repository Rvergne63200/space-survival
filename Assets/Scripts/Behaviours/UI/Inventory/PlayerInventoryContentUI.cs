using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UIElements;

// Inventory of the player (Inventory.uxml), opened and closed with the Inventory key (I or Tab, PlayerInventory.IsOpen).
// Under it, the equipment of the player (PlayerInventory.Equipment): one slot per place, with its name
public class PlayerInventoryContentUI : ItemCollectionUI
{
    private UnityEvent<bool, Action> ev_UpdateIsOpen;
    private PlayerInventory inventory;

    private readonly List<ItemSlotUI> equipmentSlotsUI = new List<ItemSlotUI>();

    public override void OnUpdateManager(UIManager manager)
    {
        if (manager == null)
        {
            return;
        }

        ev_UpdateIsOpen?.RemoveListener(SetVisible);

        inventory = manager.PlayerInventory;

        ev_UpdateIsOpen = inventory.ev_UpdateIsOpen;
        ev_UpdateIsOpen.AddListener(SetVisible);
    }

    protected override void OnDestroy()
    {
        ev_UpdateIsOpen?.RemoveListener(SetVisible);
        UnbindEquipment();
        base.OnDestroy();
    }

    private void SetVisible(bool visible, Action hideInterfaceAction)
    {
        ac_OnClearInterface = hideInterfaceAction;

        if (visible)
        {
            OpenPanel(inventory.Content);
            BindEquipment();
        }
        else
        {
            UnbindEquipment();
            ClosePanel();
        }
    }

    private void BindEquipment()
    {
        UnbindEquipment();

        VisualElement container = panel != null ? panel.GetComponent<UIDocument>().rootVisualElement.Q("equipment") : null;

        if (container == null || slotTemplate == null) return;

        container.Clear();

        for (int i = 0; i < PlayerInventory.EquipmentLayout.Length; i++)
        {
            VisualElement place = new VisualElement();
            place.AddToClassList("equipment-slot");

            VisualElement element = slotTemplate.Instantiate().Q("slot");
            place.Add(element);
            place.Add(new Label(PlaceName(i)) { pickingMode = PickingMode.Ignore });
            place[1].AddToClassList("equipment-slot-name");

            container.Add(place);

            ItemSlotUI slotUI = new ItemSlotUI(element, parentUI);
            slotUI.SetSlot(inventory.Equipment.Slots[i]);
            equipmentSlotsUI.Add(slotUI);
        }
    }

    private void UnbindEquipment()
    {
        foreach (ItemSlotUI slotUI in equipmentSlotsUI)
        {
            slotUI.Unbind();
        }

        equipmentSlotsUI.Clear();
    }

    // The oxygen tanks are numbered
    private static string PlaceName(int index)
    {
        EquipmentSlot place = PlayerInventory.EquipmentLayout[index];

        switch (place)
        {
            case EquipmentSlot.Head: return "Tête";
            case EquipmentSlot.Body: return "Corps";
            case EquipmentSlot.Back: return "Dos";
            case EquipmentSlot.Feet: return "Pieds";
            case EquipmentSlot.Hands: return "Mains";
            default:
                int number = 1;

                for (int i = 0; i < index; i++)
                {
                    if (PlayerInventory.EquipmentLayout[i] == place) number++;
                }

                return "Bouteille " + number;
        }
    }
}
