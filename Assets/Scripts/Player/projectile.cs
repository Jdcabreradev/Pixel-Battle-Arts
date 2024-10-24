using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float speed = 10f;
    private PlayerController owner;
    private Rigidbody2D rb;

    private Vector2 direction;
    public ulong OwnerId { get; private set; } // Track the owner's ID

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Initialize the projectile with direction and the owner's ID
    public void Initialize(PlayerController player, Vector2 targetDirection)
    {
        owner = player;
        direction = targetDirection.normalized;
        OwnerId = player.OwnerId; // Set the owner ID

        rb.velocity = direction * speed;
        RotateProjectile();
    }

    private void Update()
    {
        if (rb.velocity != Vector2.zero)
        {
            RotateProjectile();
        }
    }

    private void RotateProjectile()
    {
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Use trigger instead of collision for projectile detection
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the hit object is a player and has a PlayerController
        if (other.TryGetComponent(out PlayerController targetPlayer))
        {
            // Apply damage only if the hit player is not the owner of the projectile
            if (targetPlayer.OwnerId != OwnerId)
            {
                targetPlayer.TakeDamageServerRpc(10, OwnerId);
                NetworkObject.Despawn(true);
            }
        }

        // Destroy the projectile after trigger collision
        if (other.CompareTag("Ground") && IsServer) 
        {
            NetworkObject.Despawn(true);
        }
    }
}