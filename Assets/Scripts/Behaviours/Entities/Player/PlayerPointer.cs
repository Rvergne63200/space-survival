using UnityEngine;

public class PlayerPointer : MonoBehaviour
{
    private PlayerContext playerContext => GetComponent<PlayerContext>();

    private Transform Source
    {
        get => playerContext?.PlayerManager.ViewSource ?? null;
        set => Source = value;
    }

    private void Update()
    {
        Ray ray = new Ray(Source.position, Source.forward);

        // Ignore the player itself and the triggers (scene loaders, zones...)
        int layerMask = ~(1 << gameObject.layer);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, layerMask, QueryTriggerInteraction.Ignore))
        {
            // The collider can be on a child of the interactable (hit.transform is the collider or its Rigidbody):
            // point at the object carrying the IInterractable when there is one
            IInterractable interractable = hit.collider.GetComponentInParent<IInterractable>();
            Transform pointed = interractable is Component component ? component.transform : hit.collider.transform;

            playerContext.SetPointedObject(pointed);
        } else
        {
            playerContext.SetPointedObject(null);
        }
    }
}
