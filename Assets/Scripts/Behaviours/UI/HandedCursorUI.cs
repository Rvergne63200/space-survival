using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class HandedCursorUI : ParentedUI
{
    public GameObject toolBar;
    public Transform slot;
    public int index;

    private void Start()
    {
        parentUI.playerInventory.ev_UpdateHandedIndex.AddListener((int index) => { this.index = index; } );
        toolBar.GetComponent<PlayerToolBarUI>().ev_OnSlotsUpdated.AddListener(() => { index = parentUI.playerInventory.HandedIndex; });
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
