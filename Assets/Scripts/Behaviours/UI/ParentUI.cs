using UnityEngine;
using UnityEngine.Events;

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
