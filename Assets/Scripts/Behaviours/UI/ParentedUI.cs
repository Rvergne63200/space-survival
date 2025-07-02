using UnityEngine;

public abstract class ParentedUI : MonoBehaviour
{
    public ParentUI parentUI;

    protected virtual void Start()
    {
        parentUI.ev_BeforeUpdatePlayer.AddListener(BeforeUpdatePlayer);
        parentUI.ev_AfterUpdatePlayer.AddListener(AfterUpdatePlayer);
    }

    public virtual void AfterUpdatePlayer()
    {
        return;
    }

    public virtual void BeforeUpdatePlayer()
    {
        return;
    }
}
