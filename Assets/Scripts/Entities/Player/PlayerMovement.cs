using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    public static string OPTION_DISABLED = "player_movement_disabled";

    public PlayerStats stats;

    private PlayerInputActions inputActions;
    public Transform forwardDirection;
    private Vector2 moveInput;

    public float baseSpeed = 5f;
    public float sprintSpeed = 8f;

    private float speed;
    private bool sprint;

    public Item item;

    private Stat endurance;

    private bool active = true;

    public PlayerMovement() => ModeManager.Instance.ev_OnUpdateOptions.AddListener(OnUpdateOptions);
    public void OnUpdateOptions()
    {
        active = !ModeManager.Instance.IsEnabled(OPTION_DISABLED);
    }


    void Awake()
    {
        inputActions = new PlayerInputActions();
        speed = baseSpeed;
    }

    private void Start()
    {
        endurance = stats.Get("endurance");
    }


    private void OnEnable()
    {
        inputActions.Movement.Enable();
        inputActions.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Movement.Sprint.performed += ctx => { 
            if (endurance.Value > 5 && !sprint)
            {
                sprint = true;
            }
        };

        inputActions.Movement.Sprint.canceled += ctx =>
        {
            if(sprint)
            {
                sprint = false;
            }
        };
    }

    private void OnDisable()
    {
        inputActions.Movement.Disable();
    }



    void Update()
    {
        if (!active) return;

        if (endurance.Value < 0.5)
        {
            sprint = false;
        }

        speed = sprint ? sprintSpeed : baseSpeed;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        transform.Translate(move * Time.deltaTime * speed);

        if (move != Vector3.zero)
        {
            if (sprint)
            {
                endurance.Consume(5f, Time.deltaTime);
            }

            Vector3 cameraForward = forwardDirection.forward;
            cameraForward.y = 0f;

            if (cameraForward != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(cameraForward);
            }
        }
    }
}
