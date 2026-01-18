using UnityEngine;

public class ParentedUI : MonoBehaviour
{
    public ParentUI parentUI;

    protected virtual void Awake()
    {
        if(parentUI == null)
        {
            return;
        }

        OnUpdateManager(parentUI.UIManager);
        parentUI.ev_updateManager.AddListener(OnUpdateManager);
        parentUI.ev_updateManager.AddListener(SetupManager);

        SetupManager(parentUI.UIManager);
    }

    private void SetupManager(UIManager manager)
    {
        OnUpdatePlayerContext(manager?.PlayerContext ?? null);
        manager?.ev_updatePlayerContext.AddListener(OnUpdatePlayerContext);
    }

    public virtual void OnUpdateManager(UIManager manager)
    {
        
    }

    public virtual void OnUpdatePlayerContext(PlayerContext context)
    {

    }
}
