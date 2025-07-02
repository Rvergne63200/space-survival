using UnityEngine;

public class CursorSource : MonoBehaviour
{
    void Awake()
    {
        LocalPlayerEvents.OnLocalPlayerSpawned.AddListener(InitializePlayer);
    }

    public void InitializePlayer(GameObject player)
    {
        player.GetComponent<PlayerInterractor>().source = transform;
        player.GetComponent<Builder>().source = transform;
    }
}
