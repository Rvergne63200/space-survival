using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PointedInterractorInformationUI : ParentedUI
{
    private TextMeshProUGUI text;
    public string input;

    private UnityEvent<Transform> ev_updatePointedObject;

    protected override void Awake()
    {
        base.Awake();
        text = GetComponent<TextMeshProUGUI>();
    }

    public override void OnUpdatePlayerContext(PlayerContext context)
    {
        if(context == null)
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

    public void UpdatePointedObject(Transform pointedObject)
    {
        IInterractable pointed = parentUI.UIManager.PlayerContext.Interractable;

        if (pointed == null)
        {
            gameObject.SetActive(false);
            return;
        }

        text.text = pointed.GetInfo() + " <" + input.ToUpper() + ">";
        gameObject.SetActive(true);
    }
}
