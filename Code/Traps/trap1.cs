using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TrapDamage : MonoBehaviour
{
    public int damage = 1;

    private PlayerHealth playerInside;
    private int overlapCount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponentInParent<PlayerHealth>();
        if (player == null)
        {
            return;
        }

        if (playerInside == null || playerInside == player)
        {
            playerInside = player;
            overlapCount++;
            TryApplyDamage(playerInside);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponentInParent<PlayerHealth>();
        if (player == null)
        {
            return;
        }

        if (playerInside == null || playerInside == player)
        {
            playerInside = player;
            overlapCount = Mathf.Max(overlapCount, 1);
            TryApplyDamage(playerInside);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponentInParent<PlayerHealth>();
        if (player == null || player != playerInside)
        {
            return;
        }

        overlapCount = Mathf.Max(0, overlapCount - 1);
        if (overlapCount == 0)
        {
            playerInside = null;
        }
    }

    private void TryApplyDamage(PlayerHealth player)
    {
        if (damage <= 0 || player == null || player.IsDead)
        {
            return;
        }

        player.TakeDamage(damage);
    }

    private void OnDisable()
    {
        playerInside = null;
        overlapCount = 0;
    }

    private void OnValidate()
    {
        damage = Mathf.Max(0, damage);
    }
}
