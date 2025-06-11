using UnityEngine;
using UnityEngine.InputSystem;

public class Builder : MonoBehaviour
{
    public GameObject build;

    private GameObject currentBuild;

    public Transform source;
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
            Ray ray = new Ray(source.position, source.forward);

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
        if (currentBuild && currentBuild.GetComponent<BuildObject>().Check())
        {
            currentBuild.GetComponent<BuildObject>().Put();
            currentBuild = null;
        }
    }

    private void Update()
    {
        if (currentBuild != null)
        {
            int terrainLayerMask = LayerMask.GetMask("Terrain");
            Ray ray = new Ray(source.position, source.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, 300f, terrainLayerMask))
            {
                bool isTerrain = hit.transform.gameObject.CompareTag("Terrain");
                currentBuild.transform.position = new Vector3(hit.point.x, Mathf.Ceil(hit.point.y / 10f) * 10f, hit.point.z);
            }
        } 
    }
}
