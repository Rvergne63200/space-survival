using UnityEngine.Events;
using UnityEngine;

public static class LocalPlayerEvents
{
    private static UnityEvent<GameObject> _onLocalPlayerSpawned;
    public static UnityEvent<GameObject> OnLocalPlayerSpawned
    {
        get
        {
            if(_onLocalPlayerSpawned == null)
            {
                _onLocalPlayerSpawned = new UnityEvent<GameObject>();
            }

            return _onLocalPlayerSpawned;
        }
    }
}
