using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class movePlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float secondJumpMultiplier = 0.8f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.6f;

    [Header("Wall Movement")]
    [SerializeField] private float wallSlideSpeed = 2.5f;
    [SerializeField] private float wallJumpHorizontalForce = 6f;
    [SerializeField] private float wallJumpVerticalForce = 8f;
    [SerializeField] private float wallJumpControlLockTime = 0.12f;

    [Header("Effects")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private ParticleSystem dashParticles;

    private readonly List<float> lowGravityMultipliers = new List<float>();

    private Rigidbody2D rb;
    private float defaultGravityScale;

    private float horizontalInput;
    private bool jumpPressed;
    private bool dashPressed;
    private bool dashQueued;

    private bool isGrounded;
    private bool wasGrounded;
    private int jumpsUsed;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool isDashing;
    private bool canDash = true;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;

    private bool isTouchingWall;
    private bool isWallSliding;
    private int wallDirection;
    private float wallJumpControlLockTimer;

    private bool facingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = Mathf.Max(0f, rb.gravityScale);
        ApplyGravityScale();
    }

    private void Update()
    {
        ReadInput();
        UpdateGroundState();
        UpdateWallState();
        UpdateTimers();
        HandleFlip();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDash();
            return;
        }

        HandleMovement();
        HandleWallSlide();
        HandleJump();
        HandleDashStart();
        dashQueued = false;
    }

    public void EnterLowGravityZone(float gravityMultiplier)
    {
        lowGravityMultipliers.Add(NormalizeGravityMultiplier(gravityMultiplier));
        ApplyGravityScale();
    }

    public void ExitLowGravityZone(float gravityMultiplier)
    {
        float normalizedMultiplier = NormalizeGravityMultiplier(gravityMultiplier);
        for (int i = 0; i < lowGravityMultipliers.Count; i++)
        {
            if (Mathf.Abs(lowGravityMultipliers[i] - normalizedMultiplier) <= 0.0001f)
            {
                lowGravityMultipliers.RemoveAt(i);
                break;
            }
        }

        ApplyGravityScale();
    }

    private void ReadInput()
    {
        horizontalInput = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f);
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        dashPressed = Input.GetKeyDown(KeyCode.LeftShift);

        if (jumpPressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }

        if (dashPressed && !isDashing)
        {
            dashQueued = true;
        }
    }

    private void UpdateGroundState()
    {
        wasGrounded = isGrounded;
        isGrounded = groundCheckPoint != null && Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;

            if (!wasGrounded)
            {
                jumpsUsed = 0;
                wallJumpControlLockTimer = 0f;
            }
        }
    }

    private void UpdateWallState()
    {
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);

        if (rightHit.collider != null)
        {
            wallDirection = 1;
        }
        else if (leftHit.collider != null)
        {
            wallDirection = -1;
        }
        else
        {
            wallDirection = 0;
        }

        isTouchingWall = wallDirection != 0;
        isWallSliding = !isGrounded && !isDashing && isTouchingWall && rb.linearVelocity.y < 0f;
    }

    private void UpdateTimers()
    {
        if (!isGrounded)
        {
            coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        }

        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
        }

        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                canDash = true;
                dashCooldownTimer = 0f;
            }
        }

        if (wallJumpControlLockTimer > 0f)
        {
            wallJumpControlLockTimer = Mathf.Max(0f, wallJumpControlLockTimer - Time.deltaTime);
        }
    }

    private void HandleMovement()
    {
        if (wallJumpControlLockTimer > 0f)
        {
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleWallSlide()
    {
        if (!isWallSliding)
        {
            return;
        }

        float clampedVerticalSpeed = Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedVerticalSpeed);
    }

    private void HandleJump()
    {
        if (jumpBufferTimer <= 0f)
        {
            return;
        }

        if (TryWallJump())
        {
            return;
        }

        bool canUseGroundJump = coyoteTimer > 0f && jumpsUsed == 0;
        bool canUseAirJump = jumpsUsed > 0 && jumpsUsed < maxJumps;

        if (!canUseGroundJump && jumpsUsed == 0 && maxJumps > 1)
        {
            canUseAirJump = true;
        }

        if (!canUseGroundJump && !canUseAirJump)
        {
            return;
        }

        float currentJumpForce = jumpsUsed > 0 ? jumpForce * secondJumpMultiplier : jumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentJumpForce);

        jumpsUsed++;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        isWallSliding = false;
    }

    private bool TryWallJump()
    {
        if (isGrounded || !isTouchingWall || wallDirection == 0)
        {
            return false;
        }

        int jumpDirection = -wallDirection;
        rb.linearVelocity = new Vector2(jumpDirection * wallJumpHorizontalForce, wallJumpVerticalForce);

        wallJumpControlLockTimer = wallJumpControlLockTime;
        jumpsUsed = 1;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        isWallSliding = false;

        if (jumpDirection > 0 && !facingRight)
        {
            Flip(true);
        }
        else if (jumpDirection < 0 && facingRight)
        {
            Flip(false);
        }

        return true;
    }

    private void HandleDashStart()
    {
        dashPressed = dashQueued;

        if (!dashPressed || !canDash)
        {
            return;
        }

        dashDirection = horizontalInput;
        if (dashDirection == 0f)
        {
            dashDirection = facingRight ? 1f : -1f;
        }

        StartDash();
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        isWallSliding = false;

        ApplyGravityScale();
        rb.linearVelocity = Vector2.zero;

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }

        if (dashParticles != null)
        {
            dashParticles.Play();
        }
    }

    private void HandleDash()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection > 0f ? Vector2.right : Vector2.left, wallCheckDistance, wallLayer);
        if (hit.collider != null)
        {
            StopDash();
            return;
        }

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0f)
        {
            StopDash();
        }
    }

    private void StopDash()
    {
        isDashing = false;
        ApplyGravityScale();
        rb.linearVelocity = Vector2.zero;

        if (trail != null)
        {
            trail.emitting = false;
        }
    }

    private void HandleFlip()
    {
        if (isDashing)
        {
            return;
        }

        if (horizontalInput > 0f && !facingRight)
        {
            Flip(true);
        }
        else if (horizontalInput < 0f && facingRight)
        {
            Flip(false);
        }
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }

    private float GetGravityMultiplier()
    {
        float gravityMultiplier = 1f;

        for (int i = 0; i < lowGravityMultipliers.Count; i++)
        {
            gravityMultiplier = Mathf.Min(gravityMultiplier, lowGravityMultipliers[i]);
        }

        return gravityMultiplier;
    }

    private float NormalizeGravityMultiplier(float gravityMultiplier)
    {
        return Mathf.Clamp(gravityMultiplier, 0.01f, 1f);
    }

    private void ApplyGravityScale()
    {
        if (rb == null)
        {
            return;
        }

        rb.gravityScale = isDashing ? 0f : defaultGravityScale * GetGravityMultiplier();
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        jumpForce = Mathf.Max(0f, jumpForce);
        maxJumps = Mathf.Max(1, maxJumps);
        secondJumpMultiplier = Mathf.Max(0f, secondJumpMultiplier);
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
        groundCheckRadius = Mathf.Max(0.01f, groundCheckRadius);
        dashSpeed = Mathf.Max(0f, dashSpeed);
        dashDuration = Mathf.Max(0.01f, dashDuration);
        dashCooldown = Mathf.Max(0f, dashCooldown);
        wallCheckDistance = Mathf.Max(0.01f, wallCheckDistance);
        wallSlideSpeed = Mathf.Max(0f, wallSlideSpeed);
        wallJumpHorizontalForce = Mathf.Max(0f, wallJumpHorizontalForce);
        wallJumpVerticalForce = Mathf.Max(0f, wallJumpVerticalForce);
        wallJumpControlLockTime = Mathf.Max(0f, wallJumpControlLockTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance);
    }
}
