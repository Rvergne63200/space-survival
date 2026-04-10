using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static string OPTION_CAMERA_LOCKED = "camera_movement_locked";

    public float sensibility = 0.2f;
    public Vector3 offset;
    private Transform Target
    {
        get => context?.CameraManager.Target ?? null;
    }

    private PlayerInputActions inputActions;
    private Vector2 rotation;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private CameraContext context;

    private bool locked = false;

    public CameraMovement () => ModeManager.Instance.ev_OnUpdateOptions.AddListener(OnUpdateOptions);
    public void OnUpdateOptions()
    {
        locked = ModeManager.Instance.IsEnabled(OPTION_CAMERA_LOCKED);
    }

    void Awake()
    {
        context = GetComponent<CameraContext>();
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Camera.Enable();
        inputActions.Camera.Rotation.performed += ctx => rotation = ctx.ReadValue<Vector2>();
        inputActions.Camera.Rotation.canceled += ctx => rotation = Vector2.zero;
    }

    private void OnDisable()
    {
        inputActions.Camera.Disable();
    }

    void Update()
    {
        if (locked) return;

        Vector2 input = rotation * sensibility;

        verticalRotation -= input.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        horizontalRotation += input.x;
        horizontalRotation = horizontalRotation % 360;

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);

        transform.position = Target.position + offset;
    }
}
