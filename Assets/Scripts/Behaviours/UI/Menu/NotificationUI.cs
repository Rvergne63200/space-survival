using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum NotificationType
{
    Info,
    Warning
}

// Short messages to the player ("Inventory full", "Missing resources"...), at the bottom of the screen, which fade out.
// Layout: Notifications.uxml (container "notifications"), style in Menus.uss (.notification, .notification-warning),
// editable in the UI Builder. Usage: NotificationUI.Show("...", NotificationType.Warning)
public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    [SerializeField] private UIDocument document;
    [SerializeField] private float duration = 2.5f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private int maxMessages = 4;

    private VisualElement container;

    private class Message
    {
        public Label label;
        public string text;
        public float remaining;
    }

    private readonly List<Message> messages = new List<Message>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Shown without a NotificationUI too (in the console), so the callers do not have to check
    public static void Show(string text, NotificationType type = NotificationType.Info)
    {
        if (Instance == null)
        {
            Debug.Log(text);
            return;
        }

        Instance.Add(text, type);
    }

    private void Add(string text, NotificationType type)
    {
        if (container == null && document != null)
        {
            container = document.rootVisualElement.Q("notifications");
        }

        if (container == null) return;

        // The same message again: its display time is reset instead of showing it twice
        foreach (Message message in messages)
        {
            if (message.text == text)
            {
                message.remaining = duration;
                return;
            }
        }

        Label label = new Label(text);
        label.pickingMode = PickingMode.Ignore;
        label.AddToClassList("notification");

        if (type == NotificationType.Warning)
        {
            label.AddToClassList("notification-warning");
        }

        container.Add(label);
        messages.Add(new Message { label = label, text = text, remaining = duration });

        while (messages.Count > maxMessages)
        {
            messages[0].label.RemoveFromHierarchy();
            messages.RemoveAt(0);
        }
    }

    private void Update()
    {
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            Message message = messages[i];
            message.remaining -= Time.unscaledDeltaTime;

            if (message.remaining <= 0f)
            {
                message.label.RemoveFromHierarchy();
                messages.RemoveAt(i);
                continue;
            }

            message.label.style.opacity = Mathf.Clamp01(message.remaining / fadeDuration);
        }
    }
}
