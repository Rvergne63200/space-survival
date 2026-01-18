using UnityEngine;
using UnityEngine.Events;

public class PlayerContext : MonoBehaviour
{
    public PlayerManager PlayerManager { get; private set; }

    public void Initialize(PlayerManager manager)
    {
        PlayerManager = manager;
    }

    public Transform PointedObject { get; private set; }
    public IInterractable Interractable => PointedObject?.GetComponent<IInterractable>();
    public UnityEvent<Transform> ev_updatePointedObject;

    public void SetPointedObject(Transform pointedObject)
    {
        if (PointedObject == pointedObject)
        {
            return;
        }

        PointedObject = pointedObject;
        ev_updatePointedObject.Invoke(PointedObject);
    }
}
