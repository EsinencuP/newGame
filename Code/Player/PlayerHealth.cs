using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float damageCooldown = 1f;

    private int currentHealth;
    private float lastDamageTime = float.NegativeInfinity;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        damageCooldown = Mathf.Max(0f, damageCooldown);
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    public bool TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return false;
        }

        if (Time.time < lastDamageTime + damageCooldown)
        {
            return false;
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        lastDamageTime = Time.time;
        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            Die();
        }

        return true;
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        int healedHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (healedHealth == currentHealth)
        {
            return;
        }

        currentHealth = healedHealth;
        NotifyHealthChanged();
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        damageCooldown = Mathf.Max(0f, damageCooldown);
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Destroy(gameObject);
    }
}
