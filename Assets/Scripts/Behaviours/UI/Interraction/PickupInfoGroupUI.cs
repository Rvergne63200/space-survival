using System.Collections.Generic;
using UnityEngine;

public class PickupInfoGroupUI : ParentedUI
{
    public GameObject pickupInfoPrefab;

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

        group.info = Instantiate(pickupInfoPrefab, transform).GetComponent<PickupInfoUI>();
        group.info.SetInfobulle("+" + group.count + " " + item.Name, item.Sprite);
    }
}
