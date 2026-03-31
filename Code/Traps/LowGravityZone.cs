using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LowGravityZone : MonoBehaviour
{
    [SerializeField] private float gravityMultiplier = 0.35f;

    private readonly Dictionary<movePlayer, int> playersInside = new Dictionary<movePlayer, int>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        movePlayer player = collision.GetComponentInParent<movePlayer>();
        if (player == null)
        {
            return;
        }

        if (playersInside.TryGetValue(player, out int overlapCount))
        {
            playersInside[player] = overlapCount + 1;
            return;
        }

        playersInside[player] = 1;
        player.EnterLowGravityZone(gravityMultiplier);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        movePlayer player = collision.GetComponentInParent<movePlayer>();
        if (player == null || !playersInside.TryGetValue(player, out int overlapCount))
        {
            return;
        }

        if (overlapCount > 1)
        {
            playersInside[player] = overlapCount - 1;
            return;
        }

        playersInside.Remove(player);
        player.ExitLowGravityZone(gravityMultiplier);
    }

    private void OnDisable()
    {
        foreach (KeyValuePair<movePlayer, int> entry in playersInside)
        {
            if (entry.Key != null)
            {
                entry.Key.ExitLowGravityZone(gravityMultiplier);
            }
        }

        playersInside.Clear();
    }

    private void OnValidate()
    {
        gravityMultiplier = Mathf.Clamp(gravityMultiplier, 0.01f, 1f);
    }
}
