using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class Builder : MonoBehaviour
{
    public GameObject build;
    private GameObject currentBuild;

    private PlayerContext playerContext => GetComponent<PlayerContext>();

    private Transform Source
    {
        get => playerContext?.PlayerManager.ViewSource ?? null;
        set => Source = value;
    }

    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Actions.Enable();
        inputActions.Actions.Build.performed += OnBuildPerformed;
        inputActions.Actions.Use1.performed += OnValidatePosition;
    }

    private void OnDisable()
    {
        inputActions.Actions.Build.performed -= OnBuildPerformed;
        inputActions.Actions.Use1.performed -= OnValidatePosition;
        inputActions.Actions.Disable();
    }

    private void OnBuildPerformed(InputAction.CallbackContext context)
    {
        if(currentBuild == null)
        {
            int terrainLayerMask = LayerMask.GetMask("Terrain");
            Ray ray = new Ray(Source.position, Source.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, 300f, terrainLayerMask))
            {
                currentBuild = Instantiate(build, hit.point, Quaternion.identity);
            }
        }
        else
        {
            Destroy(currentBuild);
        }
    }

    private void OnValidatePosition(InputAction.CallbackContext context)
    {
        if (currentBuild == null) return;

        BuildObject buildObject = currentBuild.GetComponent<BuildObject>();

        if (!buildObject.Check()) return;

        Vector3 position = currentBuild.transform.position;
        float targetHeight = PutHeight(position.y);

        if (ChunkManager.Instance == null) return;

        ChunkManager.Instance.FlattenTerrainAt(position, (Mathf.Max(currentBuild.transform.localScale.x, currentBuild.transform.localScale.z) / 1.8f) + 0.1f, targetHeight);

        buildObject.Put();

        currentBuild = null;
    }

    private void Update()
    {
        if (currentBuild != null)
        {
            int terrainLayerMask = LayerMask.GetMask("Terrain");
            Ray ray = new Ray(Source.position, Source.forward);

            Debug.DrawRay(Source.position, Source.forward * 300f, Color.red, 1f);

            if (Physics.Raycast(ray, out RaycastHit hit, 300f, terrainLayerMask))
            {
                bool isTerrain = hit.transform.gameObject.CompareTag("Terrain");
                currentBuild.transform.position = new Vector3(hit.point.x, PutHeight(hit.point.y), hit.point.z);
            }
        } 
    }

    private float PutHeight(float height)
    {
        return height + 0.02f;
    }
}
