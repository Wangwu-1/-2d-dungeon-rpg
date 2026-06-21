using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other) => HandleSpikeContact(other);
    private void OnTriggerStay2D(Collider2D other) => HandleSpikeContact(other);

    private void HandleSpikeContact(Collider2D other)
    {
        if (!other.TryGetComponent<Player>(out Player player))
            return;
        if (!player.isInvincible && !player.isDead)
            player.TakeSpikeDamage(damage);
    }
}
