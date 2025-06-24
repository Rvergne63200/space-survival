using UnityEngine;
using UnityEngine.Events;

public class BuildVerifier : MonoBehaviour
{
    public UnityEvent<int> ev_updateInvalidCollision;

    private int _ivalidCollision = 0;
    private int InvalidCollision { 
        get => _ivalidCollision; 
        set {
            bool update = _ivalidCollision != value && (value <= 0 || _ivalidCollision <= 0);
            _ivalidCollision = value;
            ev_updateInvalidCollision?.Invoke(value);
        } 
    }

    private void Awake()
    {
        ev_updateInvalidCollision = new UnityEvent<int>();
    }

    public bool Check()
    {
        return InvalidCollision <= 0;
    }

    public void OnTriggerEnter(Collider other)
    {
        InvalidCollision++;
        return;
    }

    public void OnTriggerExit(Collider other)
    {
        InvalidCollision--;
        return;
    }
}
