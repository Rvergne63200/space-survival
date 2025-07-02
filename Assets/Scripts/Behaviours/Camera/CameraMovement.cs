using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static string OPTION_CAMERA_LOCKED = "camera_movement_locked";

    public Vector3 offset;
    private Transform target;
    private Forwarder forwarder;
    private bool locked = false;

    public CameraMovement () => ModeManager.Instance.ev_OnUpdateOptions.AddListener(OnUpdateOptions);

    public void OnUpdateOptions()
    {
        locked = ModeManager.Instance.IsEnabled(OPTION_CAMERA_LOCKED);
    }

    public void InitializePlayer(GameObject player)
    {
        forwarder = player.GetComponentInChildren<Forwarder>();

        if(forwarder != null ) 
        {
            target = player.GetComponentInChildren<Forwarder>().gameObject.transform;
        }
        else
        {
            target = player.transform;
        }       
    }

    void Awake()
    {
        LocalPlayerEvents.OnLocalPlayerSpawned.AddListener(InitializePlayer);
    }

    void Update()
    {
        if (locked || target == null) return;

        transform.rotation = Quaternion.Euler(forwarder.Forward);
        transform.position = target.position + offset;
    }
}
