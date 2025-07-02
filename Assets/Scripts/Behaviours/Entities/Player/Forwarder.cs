using UnityEngine;

public class Forwarder : MonoBehaviour
{
    public float sensibility;
    private PlayerInputActions inputActions;

    private Vector2 rotation;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    public Vector3 Forward { get => new Vector3(verticalRotation, horizontalRotation, 0); }


    private void Awake()
    {
        inputActions = new PlayerInputActions();
        verticalRotation = transform.rotation.eulerAngles.x;
        horizontalRotation = transform.rotation.eulerAngles.y;
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


    private void Update()
    {
        Vector2 input = rotation * sensibility;

        verticalRotation -= input.y;
        verticalRotation = Mathf.Clamp(verticalRotation, transform.eulerAngles.x - 90f, transform.eulerAngles.x + 90f);

        horizontalRotation += input.x;
        horizontalRotation = horizontalRotation % 360;
    }
}
