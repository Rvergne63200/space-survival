using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

// Frame over the selected tool bar slot (element "handed-cursor" of Hud.uxml), which slides to the new slot
public class HandedCursorUI : ParentedUI
{
    [SerializeField] private UIDocument document;
    [SerializeField] private PlayerToolBarUI toolBar;

    public int index;

    // Time to reach the selected slot, in seconds
    public float smoothTime = 0.05f;

    private Vector2 position;
    private Vector2 velocity = Vector2.zero;
    private bool placed;

    private VisualElement cursor;
    private UnityEvent<int> ev_UpdateHandedIndex;

    private void Start()
    {
        toolBar.ev_OnSlotsUpdated.AddListener(OnSlotsUpdated);
    }

    protected override void OnDestroy()
    {
        ev_UpdateHandedIndex?.RemoveListener(SetIndex);

        if (toolBar != null)
        {
            toolBar.ev_OnSlotsUpdated.RemoveListener(OnSlotsUpdated);
        }

        base.OnDestroy();
    }

    public override void OnUpdateManager(UIManager manager)
    {
        ev_UpdateHandedIndex?.RemoveListener(SetIndex);
        ev_UpdateHandedIndex = manager?.PlayerInventory?.ev_UpdateHandedIndex;
        ev_UpdateHandedIndex?.AddListener(SetIndex);
    }

    private void OnSlotsUpdated()
    {
        index = parentUI.UIManager?.PlayerInventory?.HandedIndex ?? 0;
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }

    private void Update()
    {
        if (cursor == null && document != null && document.rootVisualElement != null)
        {
            cursor = document.rootVisualElement.Q("handed-cursor");
        }

        // The tool bar slots may not be created yet
        if (cursor == null || cursor.parent == null || index < 0 || index >= toolBar.SlotsUI.Count)
        {
            return;
        }

        VisualElement slot = toolBar.SlotsUI[index].Element;

        // Not laid out yet
        if (float.IsNaN(slot.layout.width))
        {
            return;
        }

        Vector2 target = slot.ChangeCoordinatesTo(cursor.parent, Vector2.zero);

        if (!placed)
        {
            position = target;
            placed = true;
        }
        else
        {
            // The velocity must persist between frames, otherwise SmoothDamp does not smooth anything
            position = Vector2.SmoothDamp(position, target, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        cursor.style.left = position.x;
        cursor.style.top = position.y;
    }
}
