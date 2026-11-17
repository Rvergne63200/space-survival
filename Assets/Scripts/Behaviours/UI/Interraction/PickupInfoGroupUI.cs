using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Picked up items, in the HUD (element "pickups" of Hud.uxml): one PickupInfo.uxml per item
public class PickupInfoGroupUI : ParentedUI
{
    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset pickupInfoTemplate;

    // Display time of a notification, in seconds
    public float durability = 5f;

    // The pickups of an item are added up while they follow each other within this time (seconds), even when its
    // notification has already disappeared (the picked up objects can be far from each other)
    public float groupDuration = 10f;

    private class Group
    {
        public PickupInfoUI info;
        public int count;
        public float lastPickupTime;
    }

    private readonly Dictionary<ItemData, Group> groups = new Dictionary<ItemData, Group>();

    public void OnPickupItems(int count, ItemData item)
    {
        if (count <= 0 || item == null) return;

        if (!groups.TryGetValue(item, out Group group) || Time.unscaledTime - group.lastPickupTime > groupDuration)
        {
            group = new Group();
            groups[item] = group;
        }

        group.count += count;
        group.lastPickupTime = Time.unscaledTime;

        // Its notification is still shown: updated and shown again for its whole duration
        if (group.info != null)
        {
            group.info.SetInfobulle("+" + group.count + " " + item.Name, item.Sprite);
            group.info.Restart();
            return;
        }

        VisualElement container = document != null && document.rootVisualElement != null ? document.rootVisualElement.Q("pickups") : null;

        if (container == null || pickupInfoTemplate == null) return;

        VisualElement element = pickupInfoTemplate.Instantiate().Q("pickup");
        container.Add(element);

        group.info = new PickupInfoUI(element, durability);
        group.info.SetInfobulle("+" + group.count + " " + item.Name, item.Sprite);
    }

    private void Update()
    {
        foreach (Group group in groups.Values)
        {
            if (group.info != null && !group.info.Tick(Time.deltaTime))
            {
                group.info = null;
            }
        }
    }
}
