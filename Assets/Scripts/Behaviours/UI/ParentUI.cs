using UnityEngine;
using UnityEngine.Events;

public class ParentUI : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public PlayerInventory PlayerInventory 
    { get => Player?.GetComponent<PlayerInventory>(); }

    public PlayerStats PlayerStats 
    { get => Player?.GetComponent<PlayerStats>(); }

    public GameObject Player { get; private set; }


    public GameObject dragginSlotUI;


    public UnityEvent ev_AfterUpdatePlayer { get; private set; }
    public UnityEvent ev_BeforeUpdatePlayer { get; private set; }

    public void InitializePlayer(GameObject player)
    {
        ev_BeforeUpdatePlayer.Invoke();

        if (PlayerInventory != null)
        {
            PlayerInventory.draggingSlot.ev_update.RemoveListener(OnUpdateDraggingItemSlot);
        }

        this.Player = player;

        if(PlayerInventory != null)
        {
            PlayerInventory.draggingSlot.ev_update.AddListener(OnUpdateDraggingItemSlot);
        }

        ev_AfterUpdatePlayer.Invoke();
    }

    void Awake()
    {
        ev_BeforeUpdatePlayer = new UnityEvent();
        ev_AfterUpdatePlayer = new UnityEvent();

        LocalPlayerEvents.OnLocalPlayerSpawned.AddListener(InitializePlayer);

        InterfaceManager.GetInstance();
        inputActions = new PlayerInputActions();
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
