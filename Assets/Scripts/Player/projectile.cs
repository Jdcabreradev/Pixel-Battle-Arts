using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float speed = 10f; // Velocidad del proyectil
    private PlayerController owner; // Referencia al jugador que lanz� el proyectil
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(PlayerController player)
    {
        owner = player;
        // Encuentra al primer enemigo
        GameObject enemy = GameObject.FindWithTag("Enemy");

        if (enemy != null)
        {
            // Calcula la direcci�n hacia el enemigo
            Vector2 direction = (enemy.transform.position - transform.position).normalized;
            rb.velocity = direction * speed;
        }
        else
        {
            // Si no hay enemigo, el proyectil avanza hacia la direcci�n actual
            rb.velocity = transform.right * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Aplica da�o al enemigo llamando la funci�n de da�o del controlador
            collision.gameObject.GetComponent<PlayerController>().TakeDamageServerRpc(10);
        }

        // Destruye el proyectil tras la colisi�n
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}