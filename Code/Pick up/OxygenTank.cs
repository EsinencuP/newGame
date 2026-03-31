using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OxygenTank : MonoBehaviour
{
    public float oxygenAmount = 30f;

    private bool isCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }

    private void TryCollect(Component other)
    {
        if (isCollected || oxygenAmount <= 0f || other == null)
        {
            return;
        }

        PlayerOxygen player = other.GetComponentInParent<PlayerOxygen>();
        if (player == null)
        {
            return;
        }

        isCollected = true;
        player.AddOxygen(oxygenAmount);
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        oxygenAmount = Mathf.Max(0f, oxygenAmount);
    }
}
