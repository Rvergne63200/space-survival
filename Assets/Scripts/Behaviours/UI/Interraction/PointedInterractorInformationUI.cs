using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

// Information of the interactable pointed by the player, in the HUD (element "pointed-label" of Hud.uxml):
// its text and key, above the crosshair, which fade in and out
public class PointedInterractorInformationUI : ParentedUI
{
    [SerializeField] private UIDocument document;

    public string input;
    public float fadeDuration = 0.5f;

    private Label text;

    private float alpha;
    private float targetAlpha;

    private UnityEvent<Transform> ev_updatePointedObject;

    private IInterractable pointed = null;

    private void Start()
    {
        ApplyAlpha();
    }

    public override void OnUpdatePlayerContext(PlayerContext context)
    {
        if (context == null)
        {
            return;
        }

        if (ev_updatePointedObject != null)
        {
            ev_updatePointedObject.RemoveListener(UpdatePointedObject);
        }

        ev_updatePointedObject = context.ev_updatePointedObject;
        ev_updatePointedObject.AddListener(UpdatePointedObject);
    }

    protected override void OnDestroy()
    {
        ev_updatePointedObject?.RemoveListener(UpdatePointedObject);
        base.OnDestroy();
    }

    private bool FindElements()
    {
        if (text == null && document != null && document.rootVisualElement != null)
        {
            text = document.rootVisualElement.Q<Label>("pointed-label");
        }

        return text != null;
    }

    public void UpdatePointedObject(Transform pointedObject)
    {
        var oldPointed = pointed;
        pointed = parentUI.UIManager.PlayerContext.Interractable;

        if (pointed == null)
        {
            if (oldPointed != null)
            {
                targetAlpha = 0f;
            }

            return;
        }

        // Also when going directly from an interactable to another one
        RefreshInfo();
        targetAlpha = 1f;
    }

    private void Update()
    {
        // The info can change while pointing at the same object (partial pickup): keep it up to date.
        // A fully picked up object is destroyed until the pointer finds the next one
        if (pointed != null && !(pointed is Object pointedObject && pointedObject == null))
        {
            RefreshInfo();
        }

        if (alpha != targetAlpha)
        {
            float step = fadeDuration > 0f ? Time.deltaTime / fadeDuration : 1f;
            alpha = Mathf.MoveTowards(alpha, targetAlpha, step);
            ApplyAlpha();
        }
    }

    private void ApplyAlpha()
    {
        if (!FindElements()) return;

        text.style.opacity = alpha;
        text.style.display = alpha > 0f ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void RefreshInfo()
    {
        if (!FindElements()) return;

        string info = pointed.GetInfo() + " [" + input.ToUpper() + "]";

        if (text.text != info)
        {
            text.text = info;
        }
    }
}
