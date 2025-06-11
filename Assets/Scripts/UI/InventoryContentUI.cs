using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryContentUI : ParentedUI
{
    [SerializeField]
    protected ItemStorage inventory;

    public GameObject slotPrefab;

    public int slotsByLine;

    private GameObject[] contentSlots;

    private Action ac_OnClearInterface = () => { };

    public void Start() => UpdateUI();

    private void UpdateItemCollectionUI<T>(ItemCollection<T> collection, ref GameObject[] slotsUI) where T : Item
    {
        slotsUI = new GameObject[collection.Size];

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int j = 0; j < collection.Size; j++)
        {           
            slotsUI[j] = Instantiate(slotPrefab, transform);

            InventorySlotUI<T> slotUI = slotsUI[j].GetComponent<InventorySlotUI<T>>();
            slotUI.parentUI = parentUI;

            if (j >= collection.Slots.Count || collection.Slots[j] == null)
            {
                slotUI.setSlot(null);
            }
            else
            {
                slotUI.setSlot(collection.Slots[j]);
            }
        }
    }

    private void SetItemCollectionUISize<T>(ItemCollection<T> collection) where T : Item
    {
        float width = 0;
        float height = 0;

        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();

        int slotsByLine = this.slotsByLine > collection.Size ? collection.Size : this.slotsByLine;

        width = (grid.cellSize.x + grid.spacing.x) * slotsByLine - grid.spacing.x + grid.padding.right + grid.padding.left;

        int lineCount = Mathf.CeilToInt((float)collection.Size / (float)slotsByLine);
        height = (grid.cellSize.y + grid.spacing.y) * lineCount - grid.spacing.y + grid.padding.top + grid.padding.bottom;

        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public void UpdateUI()
    {
        if(inventory != null)
        {
            UpdateItemCollectionUI(inventory.Content, ref contentSlots);
            SetItemCollectionUISize(inventory.Content);
        }
    }

    private void DisplayInventory(bool display)
    {
        InterfaceManager.GetInstance().SetInterface(gameObject, display);
        gameObject.SetActive(display);
    }

    private void OnDisable()
    {
        ac_OnClearInterface();
    }

    public void SetInventory(ItemStorage inventory, Action clearInterfaceAction)
    {
        this.inventory = inventory;
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
