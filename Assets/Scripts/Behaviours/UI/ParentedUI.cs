using UnityEngine;

public class ParentedUI : MonoBehaviour
{
    public ParentUI parentUI;

    private UIManager subscribedManager;

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

    protected virtual void OnDestroy()
    {
        if (parentUI != null)
        {
            parentUI.ev_updateManager.RemoveListener(OnUpdateManager);
            parentUI.ev_updateManager.RemoveListener(SetupManager);
        }

        UnsubscribeManager();
    }

    private void SetupManager(UIManager manager)
    {
        UnsubscribeManager();

        OnUpdatePlayerContext(manager?.PlayerContext ?? null);

        if (manager != null)
        {
            manager.ev_updatePlayerContext.AddListener(OnUpdatePlayerContext);
            subscribedManager = manager;
        }
    }

    private void UnsubscribeManager()
    {
        if (subscribedManager != null)
        {
            subscribedManager.ev_updatePlayerContext.RemoveListener(OnUpdatePlayerContext);
        }

        subscribedManager = null;
    }

    public virtual void OnUpdateManager(UIManager manager)
    {
        
    }

    public virtual void OnUpdatePlayerContext(PlayerContext context)
    {

    }
}
