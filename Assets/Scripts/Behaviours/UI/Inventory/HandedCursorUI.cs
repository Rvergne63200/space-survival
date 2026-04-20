using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Events;

public class HandedCursorUI : ParentedUI
{
    public GameObject toolBar;
    public Transform slot;
    public int index;

    private UnityEvent<int> ev_UpdateHandedIndex;

    private void Start()
    {
        toolBar.GetComponent<PlayerToolBarUI>().ev_OnSlotsUpdated.AddListener(() => { index = parentUI.UIManager.PlayerInventory?.HandedIndex ?? 0; });
    }

    public override void OnUpdateManager(UIManager manager)
    {
        ev_UpdateHandedIndex?.RemoveListener(SetIndex);
        ev_UpdateHandedIndex = parentUI.UIManager?.PlayerInventory?.ev_UpdateHandedIndex;
        ev_UpdateHandedIndex?.AddListener(SetIndex);
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }

    private void Update()
    {
        slot = toolBar.transform.GetChild(index);

        Vector3 vel = Vector3.zero;

        if (slot != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, slot.position, ref vel, 0.007f);
        }
    }
}
