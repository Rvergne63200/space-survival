using TMPro;
using UnityEngine;

public class PointedInterractorInformationUI : ParentedUI
{
    private TextMeshProUGUI text;
    public string input;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void UpdatePointed(IInterractable pointed)
    {
        if(pointed == null)
        {
            gameObject.SetActive(false);
            return;
        }

        text.text = pointed.GetInfo() + " <" + input.ToUpper() + ">";
        gameObject.SetActive(true);
    }

    public override void BeforeUpdatePlayer()
    {
        base.BeforeUpdatePlayer();

        PlayerInterractor interractor = parentUI.Player?.GetComponent<PlayerInterractor>();

        if (interractor != null)
        {
            interractor.ev_UpdatePointed.RemoveListener(UpdatePointed);
        }

        UpdatePointed(null);
    }

    public override void AfterUpdatePlayer()
    {
        base.AfterUpdatePlayer();

        PlayerInterractor interractor = parentUI.Player?.GetComponent<PlayerInterractor>();

        if (interractor != null)
        {
            parentUI.Player.GetComponent<PlayerInterractor>().ev_UpdatePointed.AddListener(UpdatePointed);
        }

        UpdatePointed(null);
    }
}
