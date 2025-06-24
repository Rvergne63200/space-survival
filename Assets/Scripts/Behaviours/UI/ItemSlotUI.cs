public class ItemSlotUI : InventorySlotUI<Item>
{
    public void OnPlayerClick()
    {
        if(slot == null)
        {
            setSlot(new ItemSlot<Item>());
        }

        parentUI.playerInventory.TryMerge(slot, Direction.Out);
    }
}