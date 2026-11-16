using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ParentUI : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public UIManager _uiManager;
    public UIManager UIManager 
    { 
        get => _uiManager; 
        private set
        {
            _uiManager = value;
            ev_updateManager.Invoke(_uiManager);
        } 
    }

    public GameObject dragginSlotUI;

    public UnityEvent<UIManager> ev_updateManager = new();


    [SerializeField] private PickupInfoGroupUI _pickupInfoGroupUI;
    public PickupInfoGroupUI PickupInfoGroupUI { get => _pickupInfoGroupUI; private set => _pickupInfoGroupUI = value; }

    [SerializeField] private ItemStorageUI _searchedItemStorageUI;
    public ItemStorageUI SearchedItemStorageUI { get => _searchedItemStorageUI; private set => _searchedItemStorageUI = value; }

    [SerializeField] private CraftingStationUI _craftingStationUI;
    public CraftingStationUI CraftingStationUI { get => _craftingStationUI; private set => _craftingStationUI = value; }


    public void Initialize(UIManager UIManager)
    {
        this.UIManager = UIManager;
    }

    void Awake()
    {
        InterfaceManager.GetInstance();
        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        UIManager?.PlayerInventory.draggingSlot.ev_update.AddListener(OnUpdateDraggingItemSlot);
    }

    private void OnEnable()
    {
        inputActions.Actions.CloseInterfaces.performed += OnCloseInterfacesPerformed;
        inputActions.Actions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Actions.Disable();
        inputActions.Actions.CloseInterfaces.performed -= OnCloseInterfacesPerformed;
    }

    // Escape: closes the open interfaces (the pause menu is one of them), otherwise opens the pause menu in game
    private void OnCloseInterfacesPerformed(InputAction.CallbackContext ctx)
    {
        InterfaceManager interfaceManager = InterfaceManager.GetInstance();

        // The death screen cannot be closed with Escape
        if (DeathScreenUI.Instance != null && DeathScreenUI.Instance.IsOpen) return;

        if (interfaceManager.HasOpenInterface)
        {
            interfaceManager.CloseInterfaces();
        }
        else if (PauseMenuUI.Instance != null && PlayerManager.Instance != null && PlayerManager.Instance.Player != null)
        {
            PauseMenuUI.Instance.Open();
        }
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
