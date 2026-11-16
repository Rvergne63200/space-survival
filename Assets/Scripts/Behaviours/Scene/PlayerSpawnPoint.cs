using UnityEngine;

// Where the player appears in a new game. A scene without spawn point (main menu) has no player.
// With a save, the player appears at its saved position instead.
public class PlayerSpawnPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}
