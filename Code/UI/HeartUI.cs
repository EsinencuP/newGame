using UnityEngine;

public class HeartUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private GameObject[] hearts;

    private void OnEnable()
    {
        player.OnHealthChanged += UpdateHearts;
    }

    private void OnDisable()
    {
        player.OnHealthChanged -= UpdateHearts;
    }

    private void Start()
    {
        UpdateHearts(player.CurrentHealth, player.MaxHealth);
    }

    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(i < current);
        }
    }
}