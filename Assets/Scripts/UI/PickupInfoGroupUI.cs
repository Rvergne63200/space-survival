using UnityEngine;

public class PickupInfoGroupUI : ParentedUI
{
    public GameObject pickupInfoPrefab;

    public void OnPickupItems(int count, ItemData item)
    {
        if(count <= 0) return;

        GameObject infoUI = Instantiate(pickupInfoPrefab, transform);
        string text = ((count >= 0) ? "+" : "-") + count.ToString() + " " + item.Name;
        infoUI.GetComponent<PickupInfoUI>().SetInfobulle(text, item.Sprite);
    }
}
