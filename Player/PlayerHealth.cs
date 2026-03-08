using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5; //* Максимальное количество здоровья
    public float damageCooldown = 1f; //* Время между получением урона

    private int currentHealth; //* Текущее количество здоровья
    private float lastDamageTime; //* Время последнего полученного урона
    

    void Start() //! Инициализируем здоровье при старте игры
    {
        currentHealth = maxHealth; //? Инициализируем здоровье при старте игры
        Debug.Log("HP: " + currentHealth); //? Выводим текущее здоровье в консоль при старте игры
    }

    public void TakeDamage(int damage)
    {
     if (Time.time < lastDamageTime + damageCooldown) //? Проверяем, прошло ли достаточно времени с последнего полученного урона    
            return; //? Если не прошло, то не наносим урон

        currentHealth -= damage; // ? Уменьшаем текущее здоровье на количество урона

        Debug.Log("HP: " + currentHealth); //? Выводим текущее здоровье в консоль после получения урона
        lastDamageTime = Time.time; //? Обновляем время последнего полученного урона

        if (currentHealth <= 0) //? Если здоровье меньше или равно нулю, вызываем метод смерти
        {
            Die(); //? Вызываем метод смерти
        }
    }

    public void Heal(int amount) //! Метод для лечения игрока
    {
        currentHealth += amount; //? Увеличиваем текущее здоровье на количество лечения

        if (currentHealth > maxHealth) //? Если текущее здоровье превышает максимальное, устанавливаем его равным максимальному
        {
            currentHealth = maxHealth; //? Устанавливаем текущее здоровье равным максимальному, если оно превышает его
        }

        Debug.Log("HP: " + currentHealth); //? Выводим текущее здоровье в консоль после лечения
    }

    public int GetHealth() //! Метод для получения текущего здоровья игрока
{
    return currentHealth; //? Возвращаем текущее здоровье игрока
}

    void Die() //! Метод, вызываемый при смерти игрока
    {
        Debug.Log("Игрок умер");   //? Выводим сообщение о смерти игрока в консоль

        Destroy(gameObject); //? Уничтожаем объект игрока, чтобы он исчез из игры
    }
}