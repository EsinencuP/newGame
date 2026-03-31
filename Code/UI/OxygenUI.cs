using UnityEngine;
using UnityEngine.UI;

public class OxygenUI : MonoBehaviour
{
    public PlayerOxygen player;
    public Image oxygenBar;

    private void Awake()
    {
        TryResolvePlayer();
    }

    private void OnEnable()
    {
        TryResolvePlayer();
        if (player != null)
        {
            player.OnOxygenChanged += UpdateBar;
            UpdateBar(player.CurrentOxygen, player.MaxOxygen);
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnOxygenChanged -= UpdateBar;
        }
    }

    private void UpdateBar(float current, float max)
    {
        if (oxygenBar == null)
        {
            return;
        }

        oxygenBar.fillAmount = max > 0f ? Mathf.Clamp(current / max, 0f, 1f) : 0f;
    }

    private void TryResolvePlayer()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerOxygen>();
        }
    }
}
