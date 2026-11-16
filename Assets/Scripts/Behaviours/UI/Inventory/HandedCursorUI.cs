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

    // Time to reach the selected slot, in seconds
    public float smoothTime = 0.05f;
    private Vector3 velocity = Vector3.zero;

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
        // The tool bar slots may not be created yet
        if (index < 0 || index >= toolBar.transform.childCount)
        {
            return;
        }

        slot = toolBar.transform.GetChild(index);

        // The velocity must persist between frames, otherwise SmoothDamp does not smooth anything
        transform.position = Vector3.SmoothDamp(transform.position, slot.position, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
    }
}
