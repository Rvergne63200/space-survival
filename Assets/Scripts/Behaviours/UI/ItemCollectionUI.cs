using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public abstract class ItemCollectionUI<T> : ParentedUI where T : Item
{
    public UnityEvent ev_OnSlotsUpdated;


    private ItemCollection<T> _inventory;
    protected ItemCollection<T> Inventory
    {
        get { return _inventory; } 
        set { 
            if(_inventory != null)
            {
                _inventory.ev_Update.RemoveListener(UpdateUI);
            }

            _inventory = value;

            if (value != null)
            {
                _inventory.ev_Update.AddListener(UpdateUI);
            }

            UpdateUI();
        }
    }


    public GameObject slotPrefab;

    public int slotsByLine;

    private GameObject[] contentSlots;

    protected Action ac_OnClearInterface = () => { };


    protected virtual void Awake()
    {
        ev_OnSlotsUpdated = new UnityEvent();
    }

    public void Start()
    {
        Inventory = Inventory;
    }

    protected void UpdateItemCollectionUI<U>(ItemCollection<U> collection, ref GameObject[] slotsUI) where U : T
    {
        slotsUI = new GameObject[collection.Slots.Count];

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int j = 0; j < collection.Slots.Count; j++)
        {
            if (collection.Slots[j] == null)
            {
                slotsUI[j] = null;
                continue;
            }

            slotsUI[j] = Instantiate(slotPrefab, transform);

            InventorySlotUI<U> slotUI = slotsUI[j].GetComponent<InventorySlotUI<U>>();
            slotUI.parentUI = parentUI;

            slotUI.setSlot(collection.Slots[j]);
        }

        ev_OnSlotsUpdated.Invoke();
    }

    protected void SetItemCollectionUISize<U>(ItemCollection<U> collection) where U : T
    {
        float width = 0;
        float height = 0;

        int size = collection.Slots.Count;

        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();

        int slotsByLine = this.slotsByLine > size ? size : this.slotsByLine;

        width = (grid.cellSize.x + grid.spacing.x) * slotsByLine - grid.spacing.x + grid.padding.right + grid.padding.left;

        int lineCount = Mathf.CeilToInt((float)size / (float)slotsByLine);
        height = (grid.cellSize.y + grid.spacing.y) * lineCount - grid.spacing.y + grid.padding.top + grid.padding.bottom;

        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public void UpdateUI()
    {
        if(Inventory != null)
        {
            UpdateItemCollectionUI(Inventory, ref contentSlots);
            SetItemCollectionUISize(Inventory);
        }
    }

    protected void DisplayInventory(bool display)
    {
        InterfaceManager.GetInstance().SetInterface(gameObject, display);
        gameObject.SetActive(display);
    }

    private void OnDisable()
    {
        ac_OnClearInterface();
    }

    public void SetInventory(ItemCollection<T> inventory, Action clearInterfaceAction)
    {
        this.Inventory = inventory;
        DisplayInventory(inventory != null);
        ac_OnClearInterface = clearInterfaceAction;
        UpdateUI();
    }

    public void SetVisible(bool visible, Action hideInterfaceAction)
    {
        DisplayInventory(visible);
        ac_OnClearInterface = hideInterfaceAction;
    }
}
