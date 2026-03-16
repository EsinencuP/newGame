using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashTime = 0.15f;
    public float dashCooldown = 1f;

    [Header("Collision")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.6f;

    [Header("Effects")]
    public TrailRenderer trail;
    public ParticleSystem dashParticles;

    float moveInput;

    bool isDashing;
    bool canDash = true;
    float dashTimer;
    float dashDirection;

    Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            dashDirection = moveInput;

            if (dashDirection == 0)
                dashDirection = Mathf.Sign(transform.localScale.x);

            StartDash();
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            Move();
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(moveInput) * Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
    }

    void HandleDash()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right * dashDirection,
            wallCheckDistance,
            wallLayer
        );

        if (hit)
        {
            StopDash();
            return;
        }

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);

        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0)
            StopDash();
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimer = dashTime;

        rb.gravityScale = 0;

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }

        if (dashParticles != null)
            dashParticles.Play();
    }

    void StopDash()
    {
        isDashing = false;

        rb.gravityScale = 1;
        rb.linearVelocity = Vector2.zero;

        if (trail != null)
            trail.emitting = false;

        Invoke(nameof(ResetDash), dashCooldown);
    }

    void ResetDash()
    {
        canDash = true;
    }
}