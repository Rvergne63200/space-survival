using UnityEngine;

public class ParentUI : MonoBehaviour
{
    private PlayerInputActions inputActions;
    public PlayerInventory playerInventory;
    public PlayerStats playerStats;

    public GameObject dragginSlotUI;

    void Awake()
    {
        InterfaceManager.GetInstance();
        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        playerInventory.draggingSlot.ev_update.AddListener(OnUpdateDraggingItemSlot);
    }

    private void OnEnable()
    {
        inputActions.Actions.Enable();
        inputActions.Actions.CloseInterfaces.performed += ctx => InterfaceManager.GetInstance().CloseInterfaces();
    }

    private void OnDisable()
    {
        inputActions.Actions.Disable();
    }

    public void OnUpdateDraggingItemSlot(ItemSlot<Item> slot)
    {
        if(slot.Item == null || slot.Quantity <= 0)
        {
            dragginSlotUI.SetActive(false);
        }
        else
        {
            ItemSlotUI slotUI = dragginSlotUI.GetComponent<ItemSlotUI>();
            slotUI.UpdateSlot(slot);
            dragginSlotUI.SetActive(true);
        }
    }
}
