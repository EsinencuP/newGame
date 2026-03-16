using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public Rigidbody2D rb; //* Ссылка на компонент Rigidbody2D, который отвечает за физическое поведение объекта

    [Header("Dash Settings")] //? Заголовок для настроек рывка в инспекторе Unity
    public float dashSpeed = 20f; //* Скорость рывка
    public float dashTime = 0.15f; //* Длительность рывка
    public float dashCooldown = 1f; //* Время восстановления рывка после использования

    [Header("Collision")] //? Заголовок для настроек столкновений в инспекторе Unity
    public LayerMask wallLayer; //* Слой, который будет использоваться для проверки столкновений с стенами во время рывка
    public float wallCheckDistance = 0.6f; //* Расстояние для проверки столкновений с стенами во время рывка

    [Header("Effects")] //? Заголовок для настроек эффектов в инспекторе Unity
    public TrailRenderer trail; //* Ссылка на компонент TrailRenderer, который отвечает за визуальный эффект следа во время рывка
    public ParticleSystem dashParticles; //* Ссылка на компонент ParticleSystem, который отвечает за визуальный эффект частиц во время рывка

    private bool isDashing; //* Флаг, указывающий, находится ли игрок в состоянии рывка
    private bool canDash = true; //* Флаг, указывающий, может ли игрок использовать рывок (например, после использования рывка и до его восстановления)
    private float dashTimer; //* Таймер, который отслеживает оставшееся время рывка
    private float direction; //* Направление рывка (1 для вправо, -1 для влево)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) //? Если нажата клавиша Left Shift и игрок может использовать рывок
        {
            direction = Input.GetAxisRaw("Horizontal"); //? Получаем направление рывка на основе горизонтального ввода (1 для вправо, -1 для влево, 0 если нет ввода)

            if (direction == 0) //? Если нет горизонтального ввода, используем направление, в котором смотрит игрок (на основе масштаба по X)
                direction = transform.localScale.x; //? Если нет горизонтального ввода, используем направление, в котором смотрит игрок (на основе масштаба по X)

            StartDash(); //? Запускаем рывок, вызывая метод StartDash, который устанавливает флаг isDashing в true, отключает гравитацию и запускает эффекты рывка
        }
    }

    void FixedUpdate() //? В методе FixedUpdate мы проверяем, находится ли игрок в состоянии рывка, и если да, то выполняем логику рывка, включая проверку столкновений с стенами и управление таймером рывка
    {
        if (isDashing) //? Если игрок находится в состоянии рывка
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.right * direction,
                wallCheckDistance,
                wallLayer
            );

            if (hit)
            {
                StopDash();
                return;
            }

            rb.linearVelocity = new Vector2(direction * dashSpeed, 0);

            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0)
                StopDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimer = dashTime;

        rb.gravityScale = 0;

        trail.emitting = true;

        if (dashParticles != null)
            dashParticles.Play();
    }

    void StopDash()
    {
        isDashing = false;

        rb.gravityScale = 1;
        rb.linearVelocity = Vector2.zero;

        trail.emitting = false;

        Invoke(nameof(ResetDash), dashCooldown);
    }

    void ResetDash()
    {
        canDash = true;
    }
}