using UnityEngine;
using UnityEngine.UIElements;

// One picked up item notification (element "pickup" of PickupInfo.uxml), created by PickupInfoGroupUI.
// Fades out during its duration, then is removed
public class PickupInfoUI
{
    private readonly VisualElement element;
    private readonly VisualElement icon;
    private readonly Label label;
    private readonly float durability;

    private float countDown;

    public PickupInfoUI(VisualElement element, float durability)
    {
        this.element = element;
        this.durability = durability;

        icon = element.Q("pickup-icon");
        label = element.Q<Label>("pickup-label");

        Restart();
    }

    // Shown again for its whole duration (grouped pickups, see PickupInfoGroupUI)
    public void Restart()
    {
        countDown = durability;
        element.style.opacity = 1f;
    }

    public void SetInfobulle(string text, Sprite sprite)
    {
        if (label != null)
        {
            label.text = text;
        }

        if (icon != null)
        {
            icon.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground(StyleKeyword.None);
        }
    }

    // Returns false once removed
    public bool Tick(float deltaTime)
    {
        countDown -= deltaTime;

        if (countDown <= 0f)
        {
            element.RemoveFromHierarchy();
            return false;
        }

        element.style.opacity = countDown / durability;
        return true;
    }
}
