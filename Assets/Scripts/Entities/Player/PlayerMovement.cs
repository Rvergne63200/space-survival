using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    public static string OPTION_DISABLED = "player_movement_disabled";

    public PlayerStats stats;
    private new Rigidbody rigidbody;

    private PlayerInputActions inputActions;
    public Transform forwardDirection;
    private Vector2 moveInput;

    public float baseSpeed = 1.5f;
    public float sprintSpeed = 3f;
    public float acceleration = 30f;


    private float speed;
    private bool sprint;

    public Item item;

    private Stat endurance;

    private bool active = true;

    public float jumpIntensity = 5f;

    public float sprintEnduranceConsumeSpeed = 5f;

    public float jumpEnduranceCost = 5f;
    public float jumpEnduranceConsumeSpeed = 20f;

    private float jumpConsumingCountdown = 0f;
    private float jumpCountdown = 0f;


    private Collider playerCollider;
    private Vector3 boxExtents = new Vector3(0.4f, 0.5f, 0.4f);
    private float maxDistance = 0.8f;
    private float angleThreshold = 80f;

    public PlayerMovement() => ModeManager.Instance.ev_OnUpdateOptions.AddListener(OnUpdateOptions);
    public void OnUpdateOptions()
    {
        active = !ModeManager.Instance.IsEnabled(OPTION_DISABLED);
    }


    void Awake()
    {
        inputActions = new PlayerInputActions();
        speed = baseSpeed;
        rigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        endurance = stats.Get("endurance");
    }


    private void OnEnable()
    {
        inputActions.Movement.Enable();

        // Movement
        inputActions.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        // Sprint
        inputActions.Movement.Sprint.performed += ctx => { 
            if (endurance.Value > sprintEnduranceConsumeSpeed && !sprint)
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

        // Jump
        inputActions.Movement.Jump.performed += ctx => Jump();
    }

    private void OnDisable()
    {
        inputActions.Movement.Disable();
    }




    void Update()
    {
        if (playerCollider != null)
        {
            Collider[] overlaps = Physics.OverlapBox(transform.position + Vector3.up * 0.1f, boxExtents, transform.rotation);
            foreach (var col in overlaps)
            {
                if (col == playerCollider || col.isTrigger)
                    continue;

                if (Physics.ComputePenetration(
                    playerCollider, transform.position, transform.rotation,
                    col, col.transform.position, col.transform.rotation,
                    out Vector3 directionPenetration, out float distancePenetration) && distancePenetration > 0f)
                {
                    transform.position += directionPenetration * distancePenetration;
                }
            }
        }

        if (endurance.Value < 0.5)
        {
            sprint = false;
        }

        speed = sprint ? sprintSpeed : baseSpeed;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 targetVelocity = transform.TransformDirection(move) * speed;

        if(!active) targetVelocity = Vector3.zero;

        if (targetVelocity != Vector3.zero)
        {
            KeepDistanceWithObstacles(ref targetVelocity);

            if (sprint)
            {
                endurance.Consume(sprintEnduranceConsumeSpeed, Time.deltaTime);
            }

            Vector3 cameraForward = forwardDirection.forward;
            cameraForward.y = 0f;

            if (cameraForward != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(cameraForward);
            }
        }

        Vector3 currentVelocity = rigidbody.velocity;
        Vector3 velocityChange = targetVelocity - new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 force = velocityChange * acceleration;

        rigidbody.AddForce(force, ForceMode.Force);


        if (jumpConsumingCountdown > 0)
        {
            jumpConsumingCountdown -= Time.deltaTime;
            endurance.Consume(jumpEnduranceConsumeSpeed * Time.deltaTime);
        }

        if (jumpCountdown > 0)
        {
            jumpCountdown -= Time.deltaTime;
        }
    }

    private void KeepDistanceWithObstacles(ref Vector3 targetVelocity)
    {
        if (playerCollider == null || targetVelocity.sqrMagnitude < 0.001f)
            return;

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Vector3 direction = targetVelocity.normalized;

        if (Physics.BoxCast(origin, boxExtents, direction, out RaycastHit hit, Quaternion.LookRotation(direction), maxDistance))
        {
            if (!hit.collider.isTrigger)
            {
                Vector3 normal = hit.normal;
                float angle = Vector3.Angle(-normal, direction);

                if (angle < angleThreshold)
                {
                    bool compute = Physics.ComputePenetration(
                        playerCollider, transform.position, transform.rotation,
                        hit.collider, hit.collider.transform.position, hit.collider.transform.rotation,
                        out Vector3 directionPenetration, out float distancePenetration);

                    if ( compute && distancePenetration > 0f)
                    {
                        transform.position += directionPenetration.normalized * 0.3f;
                    }

                    Vector3 pushIntoWall = Vector3.Project(targetVelocity, -normal);
                    if (Vector3.Dot(pushIntoWall, -normal) > 0.01f)
                    {
                        targetVelocity -= pushIntoWall;
                    }
                }
            }
        }
    }


    public void Jump()
    {
        if(endurance.Value > jumpEnduranceCost && IsGrounded() && jumpCountdown <= 0 && active)
        {
            rigidbody.AddForce(Vector3.up * jumpIntensity * 0.2f, ForceMode.Impulse);
            jumpConsumingCountdown = jumpEnduranceCost / jumpEnduranceConsumeSpeed;
            jumpCountdown = 1.2f;
        }
    }

    public bool IsGrounded()
    {
        float checkDistance = transform.localScale.y;
        float checkingRadius = transform.localScale.x * 0.48f;
        Vector3 origin = transform.position;

        return Physics.SphereCast(origin, checkingRadius, Vector3.down, out RaycastHit hit, checkDistance);
    }
}
