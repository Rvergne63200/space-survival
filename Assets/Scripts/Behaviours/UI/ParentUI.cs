using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Root of the game interfaces (UI prefab, spawned by the UIManager): HUD, inventory, storage, crafting station and
// item held at the cursor, each one a UIDocument (layouts in Assets/UI/Game, editable in the UI Builder).
// Also handles Escape: closes the open interfaces, otherwise cancels the construction mode or opens the pause menu
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

    // Hidden without player (main menu): the HUD and the item held at the cursor
    [SerializeField] private UIDocument[] hudDocuments;

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

        // The death screen cannot be closed with Escape, the build wheel closes when its key is released
        if (DeathScreenUI.Instance != null && DeathScreenUI.Instance.IsOpen) return;
        if (BuildWheelUI.Instance != null && BuildWheelUI.Instance.IsOpen) return;

        if (interfaceManager.HasOpenInterface)
        {
            interfaceManager.CloseInterfaces();
            return;
        }

        // Placement, build or destruction mode: back to the normal mode before the pause menu
        Builder builder = PlayerManager.Instance != null && PlayerManager.Instance.Player != null
            ? PlayerManager.Instance.Player.GetComponent<Builder>()
            : null;

        if (builder != null && builder.IsActive)
        {
            builder.Cancel();
        }
        else if (PauseMenuUI.Instance != null && PlayerManager.Instance != null && PlayerManager.Instance.Player != null)
        {
            PauseMenuUI.Instance.Open();
        }
    }

    public void SetHudVisible(bool visible)
    {
        foreach (UIDocument document in hudDocuments)
        {
            if (document != null && document.rootVisualElement != null)
            {
                document.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
