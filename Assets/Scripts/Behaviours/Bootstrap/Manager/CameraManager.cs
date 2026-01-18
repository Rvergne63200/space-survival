using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }


    public GameObject prefab;


    private Transform target;
    public Transform Target => target;

    private GameObject currentCamera; 
    public GameObject Camera => currentCamera;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SpawnCamera();
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SpawnCamera()
    {
        if (prefab == null)
        {
            Debug.LogError("Camera prefab is empty.");
            return;
        }

        if (currentCamera != null)
        {
            Destroy(currentCamera);
        }

        currentCamera = Instantiate(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            transform.parent
        );

        currentCamera.GetComponent<CameraContext>().Initialize(this);
    }
}
