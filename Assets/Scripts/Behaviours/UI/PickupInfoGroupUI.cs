using UnityEngine;

public class PickupInfoGroupUI : ParentedUI
{
    public GameObject pickupInfoPrefab;
    private Pickuper pickuper;

    public override void AfterUpdatePlayer()
    {
        base.AfterUpdatePlayer();

        if (pickuper != null)
        {
            pickuper.ev_pickup.RemoveListener(OnPickupItems);
        }

        pickuper = parentUI.Player.GetComponent<Pickuper>();

        if (pickuper != null)
        {
            pickuper.ev_pickup.AddListener(OnPickupItems);
        }
    }

    public void OnPickupItems(int count, ItemData item)
    {
        if(count <= 0) return;

        GameObject infoUI = Instantiate(pickupInfoPrefab, transform);
        infoUI.GetComponent<PickupInfoUI>().parentUI = parentUI;

        string text = ((count >= 0) ? "+" : "-") + count.ToString() + " " + item.Name;       
        infoUI.GetComponent<PickupInfoUI>().SetInfobulle(text, item.Sprite);
    }
}
