using Mirror;
using UnityEngine;

public class PlayerInitializer : NetworkBehaviour
{
    public static GameObject LocalPlayer;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        LocalPlayer = gameObject;

        LocalPlayerEvents.OnLocalPlayerSpawned.Invoke(LocalPlayer);
    }
}
