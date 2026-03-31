using System;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerOxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float oxygenDrainRate = 5f;
    [SerializeField] private int damageWhenEmpty = 1;

    private float currentOxygen;
    private PlayerHealth playerHealth;

    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;
    public event Action<float, float> OnOxygenChanged;

    private void Awake()
    {
        maxOxygen = Mathf.Max(0f, maxOxygen);
        oxygenDrainRate = Mathf.Max(0f, oxygenDrainRate);
        damageWhenEmpty = Mathf.Max(0, damageWhenEmpty);
        playerHealth = GetComponent<PlayerHealth>();
        SetOxygen(maxOxygen);
    }

    private void Update()
    {
        if (playerHealth == null || playerHealth.IsDead || oxygenDrainRate <= 0f)
        {
            return;
        }

        if (currentOxygen > 0f)
        {
            SetOxygen(currentOxygen - oxygenDrainRate * Time.deltaTime);
            return;
        }

        if (damageWhenEmpty > 0)
        {
            playerHealth.TakeDamage(damageWhenEmpty);
        }
    }

    public void AddOxygen(float amount)
    {
        if (amount <= 0f || playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        SetOxygen(currentOxygen + amount);
    }

    public float GetOxygen()
    {
        return currentOxygen;
    }

    public float GetMaxOxygen()
    {
        return maxOxygen;
    }

    private void SetOxygen(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0f, maxOxygen);
        if (Mathf.Abs(currentOxygen - clampedValue) <= 0.0001f)
        {
            return;
        }

        currentOxygen = clampedValue;
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    private void OnValidate()
    {
        maxOxygen = Mathf.Max(0f, maxOxygen);
        oxygenDrainRate = Mathf.Max(0f, oxygenDrainRate);
        damageWhenEmpty = Mathf.Max(0, damageWhenEmpty);
        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);
    }
}
