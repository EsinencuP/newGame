using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 7f; //* Сила прыжка
    public int maxJumps = 2; //* Максимальное количество прыжков (например, двойной прыжок)

    private Rigidbody2D rb; //* Ссылка на компонент Rigidbody2D, который отвечает за физическое поведение объекта
    private int jumpCount; //* Счетчик прыжков
    private bool isGrounded;//*  Флаг, указывающий, находится ли игрок на земле

    void Start() //! Получаем компонент Rigidbody2D при старте игры     
    {
        rb = GetComponent<Rigidbody2D>(); //? Получаем компонент Rigidbody2D при старте игры
    }

    void Update() //! Проверяем ввод для прыжка
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps) //? Если нажата клавиша пробела и количество прыжков меньше максимального
{
    float force = jumpCount == 0 ? jumpForce : jumpForce * 0.8f; //? Первый прыжок - полная сила, второй - 80% от силы первого

    rb.linearVelocity = new Vector2(rb.linearVelocity.x, force); //? Устанавливаем вертикальную скорость для прыжка, сохраняя горизонтальную скорость
    jumpCount++; //? Увеличиваем счетчик прыжков
}
    }

    void OnCollisionEnter2D(Collision2D collision) //! Проверяем столкновение с землей
    {
        if (collision.gameObject.CompareTag("Ground")) //? Если столкнулись с объектом, который имеет тег "Ground"
        {
            isGrounded = true; //? Устанавливаем флаг, что игрок на земле
            jumpCount = 0; //?   Сбрасываем счетчик прыжков при касании земли
        }
    }

    void OnCollisionExit2D(Collision2D collision) //! Проверяем, когда игрок покидает землю
    {
        if (collision.gameObject.CompareTag("Ground")) //? Если покидаем объект с тегом "Ground"
        {
            isGrounded = false; //? Сбрасываем флаг, что игрок не на земле
        }
    }
}