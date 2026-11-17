using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

// Shows an ItemCollection: one ItemSlot.uxml per slot, added in a container of a UIDocument.
// The interfaces that open and close (inventory, storage) have a panel: the GameObject holding their UIDocument,
// registered in the InterfaceManager while it is open
public abstract class ItemCollectionUI : ParentedUI
{
    [SerializeField] protected VisualTreeAsset slotTemplate;
    [SerializeField] protected GameObject panel;

    public UnityEvent ev_OnSlotsUpdated = new UnityEvent();

    private ItemCollection<Item> collection;
    private VisualElement container;
    private readonly List<ItemSlotUI> slotsUI = new List<ItemSlotUI>();
    private bool panelOpen;

    // Called when the panel is closed by the InterfaceManager (Escape, death): the owner of the interface releases it
    protected Action ac_OnClearInterface = () => { };

    public IReadOnlyList<ItemSlotUI> SlotsUI => slotsUI;

    protected override void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        base.Awake();
    }

    protected override void OnDestroy()
    {
        Unbind();
        base.OnDestroy();
    }

    protected void Bind(ItemCollection<Item> collection, VisualElement container)
    {
        Unbind();

        if (collection == null || container == null)
        {
            return;
        }

        this.collection = collection;
        this.container = container;

        container.Clear();
        collection.ev_Update.AddListener(UpdateUI);

        UpdateUI();
    }

    protected void Unbind()
    {
        if (collection != null)
        {
            collection.ev_Update.RemoveListener(UpdateUI);
        }

        foreach (ItemSlotUI slotUI in slotsUI)
        {
            slotUI.Unbind();
        }

        slotsUI.Clear();
        container?.Clear();

        collection = null;
        container = null;
    }

    public void UpdateUI()
    {
        if (collection == null || container == null || slotTemplate == null)
        {
            return;
        }

        int size = collection.Slots.Count;

        for (int i = slotsUI.Count - 1; i >= size; i--)
        {
            slotsUI[i].Unbind();
            slotsUI[i].Element.RemoveFromHierarchy();
            slotsUI.RemoveAt(i);
        }

        while (slotsUI.Count < size)
        {
            // Only the slot element is kept: its style comes from the document (Game.uss)
            VisualElement element = slotTemplate.Instantiate().Q("slot");
            container.Add(element);
            slotsUI.Add(new ItemSlotUI(element, parentUI));
        }

        // A null slot keeps its UI (displayed empty) so that the UI index always matches the slot index
        for (int j = 0; j < size; j++)
        {
            slotsUI[j].SetSlot(collection.Slots[j]);
        }

        ev_OnSlotsUpdated.Invoke();
    }

    protected void OpenPanel(ItemCollection<Item> collection)
    {
        if (panel == null || collection == null)
        {
            ClosePanel();
            return;
        }

        // The UIDocument builds its elements when it is enabled
        panel.SetActive(true);
        InterfaceManager.GetInstance().SetInterface(panel, true);
        panelOpen = true;

        Bind(collection, panel.GetComponent<UIDocument>().rootVisualElement.Q("slots"));
    }

    protected void ClosePanel()
    {
        Unbind();
        panelOpen = false;

        if (panel == null)
        {
            return;
        }

        if (panel.activeSelf)
        {
            panel.SetActive(false);
        }

        InterfaceManager.GetInstance().SetInterface(panel, false);
    }

    protected virtual void Update()
    {
        // Also closed by InterfaceManager.CloseInterfaces (Escape, death)
        if (panelOpen && (panel == null || !panel.activeSelf))
        {
            ClosePanel();
            ac_OnClearInterface();
        }
    }
}
