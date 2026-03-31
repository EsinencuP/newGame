using UnityEngine;

public class HeartUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private GameObject[] hearts;

    private void Awake()
    {
        TryResolvePlayer();
    }

    private void OnEnable()
    {
        TryResolvePlayer();
        if (player != null)
        {
            player.OnHealthChanged += UpdateHearts;
            UpdateHearts(player.CurrentHealth, player.MaxHealth);
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHearts;
        }
    }

    private void UpdateHearts(int current, int max)
    {
        if (hearts == null)
        {
            return;
        }

        int visibleHearts = Mathf.Clamp(current, 0, Mathf.Min(max, hearts.Length));
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].SetActive(i < visibleHearts);
            }
        }
    }

    private void TryResolvePlayer()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerHealth>();
        }
    }
}
