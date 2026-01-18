using UnityEngine;

public class CameraContext : MonoBehaviour
{
    public CameraManager CameraManager { get; private set; }

    public void Initialize(CameraManager cameraManager)
    {
        CameraManager = cameraManager;
    }
}
