using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Clicks are handled here rather than by the Button's onClick, which only reports the left button
public class ItemSlotUI : InventorySlotUI<Item>, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayerInventory inventory = parentUI?.UIManager?.PlayerInventory;

        if (inventory == null)
        {
            return;
        }

        if (slot == null)
        {
            setSlot(new ItemSlot<Item>());
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            bool shift = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

            if (shift)
            {
                inventory.QuickTransfer(slot, GetSearchedStorage());
            }
            else
            {
                inventory.TryMerge(slot, Direction.Out);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventory.SplitOrPlaceOne(slot);
        }
    }

    // Content of the storage currently searched by the player, if any
    private ItemCollection<Item> GetSearchedStorage()
    {
        PlayerContext context = parentUI.UIManager.PlayerContext;

        if (context == null)
        {
            return null;
        }

        Searcher searcher = context.GetComponent<Searcher>();

        if (searcher == null || searcher.CurrentSearchedStorage == null)
        {
            return null;
        }

        return searcher.CurrentSearchedStorage.Content;
    }
}
