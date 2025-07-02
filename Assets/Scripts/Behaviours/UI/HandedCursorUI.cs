using UnityEngine;

public class HandedCursorUI : ParentedUI
{
    public GameObject toolBar;
    private PlayerToolBarUI toolBarUI;
    public Transform slot;
    public int index;

    private Vector3 vel = Vector3.zero;


    private void Awake()
    {
        toolBarUI = toolBar.GetComponent<PlayerToolBarUI>();
    }

    public override void AfterUpdatePlayer()
    {
        base.AfterUpdatePlayer();

        if (parentUI.PlayerInventory != null)
        {
            parentUI.PlayerInventory.ev_UpdateHandedIndex.AddListener(SetIndex);
        }

        toolBarUI.ev_OnSlotsUpdated.AddListener(SynchronizeIndex);
    }

    public override void BeforeUpdatePlayer()
    {
        base.BeforeUpdatePlayer();

        if (parentUI.PlayerInventory != null)
        {
            parentUI.PlayerInventory.ev_UpdateHandedIndex.RemoveListener(SetIndex);
        }

        toolBarUI.ev_OnSlotsUpdated.AddListener(SynchronizeIndex);
    }

    private void SetIndex(int index)
    {
       this.index = index;
    }

    private void SynchronizeIndex()
    {
        index = parentUI.PlayerInventory?.HandedIndex ?? 0;
    }

    private void Update()
    {
        if (toolBar.transform.childCount > index)
        {
            slot = toolBar.transform.GetChild(index);

            if (slot != null)
            {
                transform.position = Vector3.SmoothDamp(transform.position, slot.position, ref vel, 0.02f);
            }
        }
    }
}
