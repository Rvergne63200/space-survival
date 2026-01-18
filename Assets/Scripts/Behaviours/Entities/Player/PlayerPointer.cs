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

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            playerContext.SetPointedObject(hit.transform);
        } else
        {
            playerContext.SetPointedObject(null);
        }
    }
}
