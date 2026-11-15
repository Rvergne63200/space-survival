using UnityEngine;
using UnityEngine.InputSystem;

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

    public float sprintFovIncrease = 10f;
    public float fovChangeSpeed = 8f;

    private new Camera camera;
    private float baseFov;
    private Transform fovTarget;
    private PlayerMovement fovTargetMovement;

    public void OnUpdateOptions()
    {
        locked = ModeManager.Instance.IsEnabled(OPTION_CAMERA_LOCKED);
    }

    void Awake()
    {
        context = GetComponent<CameraContext>();
        inputActions = new PlayerInputActions();
        camera = GetComponent<Camera>();
        baseFov = camera.fieldOfView;
    }

    private void OnEnable()
    {
        ModeManager.Instance.ev_OnUpdateOptions.AddListener(OnUpdateOptions);
        OnUpdateOptions();

        inputActions.Camera.Rotation.performed += OnRotationPerformed;
        inputActions.Camera.Rotation.canceled += OnRotationCanceled;
        inputActions.Camera.Enable();
    }

    private void OnDisable()
    {
        ModeManager.Instance.ev_OnUpdateOptions.RemoveListener(OnUpdateOptions);

        // Disable before unsubscribing so that the canceled callback still resets the rotation
        inputActions.Camera.Disable();

        inputActions.Camera.Rotation.performed -= OnRotationPerformed;
        inputActions.Camera.Rotation.canceled -= OnRotationCanceled;
    }

    private void OnRotationPerformed(InputAction.CallbackContext ctx)
    {
        rotation = ctx.ReadValue<Vector2>();
    }

    private void OnRotationCanceled(InputAction.CallbackContext ctx)
    {
        rotation = Vector2.zero;
    }

    void Update()
    {
        UpdateFov();

        if (locked) return;

        Vector2 input = rotation * sensibility;

        verticalRotation -= input.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        horizontalRotation += input.x;
        horizontalRotation = horizontalRotation % 360;

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);

        transform.position = Target.position + offset;
    }

    private void UpdateFov()
    {
        Transform target = Target;

        // The player is respawned on each scene load, so the cached movement must follow the target
        if (target != fovTarget)
        {
            fovTarget = target;
            fovTargetMovement = target != null ? target.GetComponent<PlayerMovement>() : null;
        }

        bool sprinting = fovTargetMovement != null && fovTargetMovement.IsSprinting;
        float targetFov = baseFov + (sprinting ? sprintFovIncrease : 0f);

        // Frame rate independent smoothing
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, targetFov, 1f - Mathf.Exp(-fovChangeSpeed * Time.deltaTime));
    }
}
