using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float speed = 10f;
    private Rigidbody2D rb;
    public ulong projectileOwner;

    private Vector2 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Initialize the projectile with direction and the owner's ID
    public void Initialize(Vector2 targetDirection,ulong projectileOwner)
    {
        this.projectileOwner = projectileOwner;
        direction = targetDirection.normalized;

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

        if (other.TryGetComponent(out PlayerController targetPlayer))
        {
            if (targetPlayer.OwnerId != this.projectileOwner)
            {
                targetPlayer.TakeDamageServerRpc(10, this.projectileOwner);
                NetworkObject.Despawn(true);
            }
        }

        if (other.CompareTag("Ground") && IsOwner)
        {
            NetworkObject.Despawn(true);
        }
    }
}