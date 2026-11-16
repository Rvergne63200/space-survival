using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static string OPTION_DISABLED = "player_movement_disabled";

    private PlayerStats Stats
    {
        get => context?.PlayerManager.Stats ?? null;
        set => Stats = value;
    }

    private PlayerContext context;

    private new Rigidbody rigidbody;

    private PlayerInputActions inputActions;
    public Transform forwardDirection => context.PlayerManager.ViewSource;
    private Vector2 moveInput;

    public float baseSpeed = 4.5f;
    public float sprintSpeed = 7f;
    public float acceleration = 50f;
    public float airControl = 0.3f;

    // Extra gravity applied to the player only, for a snappier jump than real gravity
    public float gravityMultiplier = 2f;


    private float speed;
    private bool sprint;

    public Item item;

    private Stat endurance;

    private bool active = true;

    public float jumpHeight = 1.1f;
    public float jumpCooldown = 0.2f;

    public float sprintEnduranceConsumeSpeed = 5f;

    public float jumpEnduranceCost = 5f;
    public float jumpEnduranceConsumeSpeed = 20f;

    private float jumpCountdown = 0f;

    // Makes each jump marker key unique, so a new jump does not replace the marker of the previous one
    private int jumpCount = 0;

    public float maxSlopeAngle = 45f;
    public float groundCheckDistance = 0.1f;

    private CapsuleCollider playerCollider;
    private readonly List<Vector3> wallNormals = new List<Vector3>();

    // True only when the player actually moves faster than walking speed
    public bool IsSprinting
    {
        get
        {
            Vector3 velocity = rigidbody.linearVelocity;
            return sprint && active && new Vector2(velocity.x, velocity.z).magnitude > baseSpeed + 0.1f;
        }
    }

    public void OnUpdateOptions()
    {
        active = !ModeManager.Instance.IsEnabled(OPTION_DISABLED);
    }


    void Awake()
    {
        context = GetComponent<PlayerContext>();
        inputActions = new PlayerInputActions();
        speed = baseSpeed;
        rigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        // Without a frictionless material, friction against walls holds the player up while pushing into them
        if (playerCollider.sharedMaterial == null)
        {
            playerCollider.sharedMaterial = new PhysicsMaterial("PlayerFrictionless")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
        }
    }

    private void Start()
    {
        endurance = Stats.Get(StatName.Endurance);
    }


    private void OnEnable()
    {
        ModeManager.Instance.ev_OnUpdateOptions.AddListener(OnUpdateOptions);
        OnUpdateOptions();

        // Movement
        inputActions.Movement.Move.performed += OnMovePerformed;
        inputActions.Movement.Move.canceled += OnMoveCanceled;

        // Sprint
        inputActions.Movement.Sprint.performed += OnSprintPerformed;
        inputActions.Movement.Sprint.canceled += OnSprintCanceled;

        // Jump
        inputActions.Movement.Jump.performed += OnJumpPerformed;

        inputActions.Movement.Enable();
    }

    private void OnDisable()
    {
        ModeManager.Instance.ev_OnUpdateOptions.RemoveListener(OnUpdateOptions);

        // Disable before unsubscribing so that canceled callbacks still reset the inputs
        inputActions.Movement.Disable();

        inputActions.Movement.Move.performed -= OnMovePerformed;
        inputActions.Movement.Move.canceled -= OnMoveCanceled;
        inputActions.Movement.Sprint.performed -= OnSprintPerformed;
        inputActions.Movement.Sprint.canceled -= OnSprintCanceled;
        inputActions.Movement.Jump.performed -= OnJumpPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        if (endurance.Value > sprintEnduranceConsumeSpeed && !sprint)
        {
            sprint = true;
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        if (sprint)
        {
            sprint = false;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        Jump();
    }


    void Update()
    {
        if (jumpCountdown > 0f)
        {
            jumpCountdown -= Time.deltaTime;
        }

        if (endurance.Value < 0.5)
        {
            sprint = false;
        }
    }

    void FixedUpdate()
    {
        speed = sprint ? sprintSpeed : baseSpeed;

        // Move relative to the camera, on the horizontal plane
        Vector3 cameraForward = forwardDirection.forward;
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude < 0.0001f)
        {
            cameraForward = transform.forward;
        }

        cameraForward.Normalize();
        Vector3 cameraRight = new Vector3(cameraForward.z, 0f, -cameraForward.x);

        Vector3 move = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
        Vector3 targetVelocity = active ? move * speed : Vector3.zero;

        if (targetVelocity != Vector3.zero)
        {
            RemoveWallComponent(ref targetVelocity);

            if (sprint)
            {
                endurance.Consume(sprintEnduranceConsumeSpeed * Time.deltaTime);
            }

            rigidbody.MoveRotation(Quaternion.LookRotation(cameraForward));
        }

        // Right after a jump the ground is still detected: treat the player as airborne so the jump is not damped
        bool grounded = TryGetGround(out Vector3 groundNormal) && jumpCountdown <= 0f;

        ApplyMoveVelocity(targetVelocity, grounded, groundNormal);

        rigidbody.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);

        // The collider is frictionless: cancel the part of gravity pulling along the slope, or the player slides down
        if (grounded)
        {
            rigidbody.AddForce(-Vector3.ProjectOnPlane(Physics.gravity * gravityMultiplier, groundNormal), ForceMode.Acceleration);
        }

        // Contacts are collected again by the next physics step
        wallNormals.Clear();
    }

    private void ApplyMoveVelocity(Vector3 targetVelocity, bool grounded, Vector3 groundNormal)
    {
        // In the air, keep the momentum when there is no input
        if (!grounded && targetVelocity == Vector3.zero) return;

        float maxVelocityChange = acceleration * (grounded ? 1f : airControl) * Time.fixedDeltaTime;

        Vector3 currentVelocity = rigidbody.linearVelocity;
        Vector3 planeVelocity;

        if (grounded)
        {
            // On the ground, move along the slope at full speed, and stop completely when there is no input
            planeVelocity = Vector3.ProjectOnPlane(currentVelocity, groundNormal);
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, groundNormal).normalized * targetVelocity.magnitude;
        }
        else
        {
            planeVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        }

        Vector3 velocityChange = Vector3.MoveTowards(planeVelocity, targetVelocity, maxVelocityChange) - planeVelocity;

        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    // Prevent pushing into walls and too steep slopes, which lets the player climb them
    private void RemoveWallComponent(ref Vector3 targetVelocity)
    {
        foreach (Vector3 normal in wallNormals)
        {
            Vector3 horizontalNormal = new Vector3(normal.x, 0f, normal.z);

            if (horizontalNormal.sqrMagnitude < 0.0001f) continue;

            horizontalNormal.Normalize();

            float intoWall = Vector3.Dot(targetVelocity, horizontalNormal);

            if (intoWall < 0f)
            {
                targetVelocity -= intoWall * horizontalNormal;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CollectWallNormals(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CollectWallNormals(collision);
    }

    private void CollectWallNormals(Collision collision)
    {
        float minGroundNormalY = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            if (normal.y < minGroundNormalY)
            {
                wallNormals.Add(normal);
            }
        }
    }


    public void Jump()
    {
        if(endurance.Value > jumpEnduranceCost && jumpCountdown <= 0f && active && IsGrounded())
        {
            // Velocity needed to reach jumpHeight with the player's gravity
            float gravity = Physics.gravity.magnitude * gravityMultiplier;
            Vector3 velocity = rigidbody.linearVelocity;
            velocity.y = Mathf.Sqrt(2f * gravity * jumpHeight);
            rigidbody.linearVelocity = velocity;
            // Negative marker: consumes jumpEnduranceCost over 0.25 s
            endurance.AddMarker("player_jump_" + jumpCount++, -jumpEnduranceCost * 4f, 0.25f);
            jumpCountdown = jumpCooldown;
        }
    }

    public bool IsGrounded() => TryGetGround(out _);

    private bool TryGetGround(out Vector3 groundNormal)
    {
        groundNormal = Vector3.up;

        Vector3 scale = transform.lossyScale;
        float radius = playerCollider.radius * Mathf.Max(scale.x, scale.z) * 0.95f;
        float halfHeight = Mathf.Max(playerCollider.height * 0.5f * scale.y, radius);

        Vector3 origin = transform.TransformPoint(playerCollider.center);
        float distance = halfHeight - radius + groundCheckDistance;
        // The player walks through the structure plans (Blueprint layer): they are not ground
        int layerMask = ~((1 << gameObject.layer) | LayerMask.GetMask("Blueprint"));

        if (!Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        // A steep surface (wall, side of a dome) is not ground
        if (hit.normal.y < Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad))
        {
            return false;
        }

        groundNormal = hit.normal;
        return true;
    }
}
