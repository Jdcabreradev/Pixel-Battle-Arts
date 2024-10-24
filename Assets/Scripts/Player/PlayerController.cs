using UnityEngine;
using Unity.Netcode;
using UnityHFSM;
using UnityEngine.ProBuilder;

public class PlayerController : NetworkBehaviour
{
    public StateMachine FSM { get; private set; }
    public Animator Animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public float RunSpeed = 5f;
    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool facingRight = true;  // Tracks if the player is facing right
    private Collider2D attackCollider; // Declare attack collider
    private Collider2D playerCollider;

    public GameObject projectilePrefab; // Prefab del proyectil
    public Transform projectileSpawnPoint; // Punto de salida del proyectil
    public bool isRanged = false; // Define si el personaje es de rango

    public int Health = 100; // Player health
    public float attackCooldown = 1f; // Cooldown for attacks
    private float lastAttackTime = -1f; // Tracks the last time an attack was performed
    private bool isAlive = true;

    public ulong OwnerId { get; private set; } // New: Store the owner/player ID

    // Add audio-related fields
    public AudioSource audioSource;  // The AudioSource to play sounds
    public AudioClip attackClip;     // Sound for attacking
    public AudioClip jumpClip;       // Sound for jumping
    public AudioClip runClip;        // Sound for running
    public AudioClip hitClip;        // Sound for getting hit
    public AudioClip deathClip;      // Sound for dying

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        attackCollider = transform.Find("AttackCollider").GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();  // Ensure we have an AudioSource component
        SetAttackColliderActive(false); // Ensure it's disabled at start

        FSM = new StateMachine();

        var idleState = new IdleState(Animator, this);
        var runState = new RunState(this, rb);
        var jumpState = new JumpState(this, rb);
        var fallState = new FallState(this, rb);
        var attackState = new AttackState(this);
        var hitState = new HitState(this);
        var deathState = new DeathState(this);
        var rangedState = new RangedState(this);

        FSM.AddState("Idle", idleState);
        FSM.AddState("Run", runState);
        FSM.AddState("Jump", jumpState);
        FSM.AddState("Fall", fallState);
        FSM.AddState("Attack", attackState);
        FSM.AddState("Hit", hitState);
        FSM.AddState("Death", deathState);
        FSM.AddState("Ranged", rangedState);  // Añadir el nuevo estado

        FSM.SetStartState("Idle");
        FSM.Init();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OwnerId = OwnerClientId; // Assign the network player owner ID
    }

    private void Start()
    {
        if (IsOwner)
        {
            this.gameObject.tag = "Player";
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            this.gameObject.tag = "Enemy";
        }
    }

    private void Update()
    {
        if (!IsOwner || !isAlive) return;
        FSM.OnLogic();

        if (Input.GetMouseButtonDown(0)) // 0 is the left mouse button
        {
            Attack();
        }
    }

    public void ShootProjectile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // Calculate the direction from the player to the mouse position
        Vector2 direction = (mousePos - projectileSpawnPoint.position).normalized;

        // Spawn the projectile on the server and initialize it with the direction
        FireProjectileServerRpc(direction,this.OwnerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireProjectileServerRpc(Vector2 direction, ulong clientId)
    {
        GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectileInstance.GetComponent<Projectile>().Initialize(direction,clientId);

        // Spawn the projectile across the network
        projectileInstance.GetComponent<NetworkObject>().Spawn();
    }

    public void Flip(float horizontalInput)
    {
        if (horizontalInput > 0 && !facingRight)
        {
            facingRight = true;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (horizontalInput < 0 && facingRight)
        {
            facingRight = false;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    public void ResetJumpCount()
    {
        jumpCount = 0;
    }

    public int JumpCount
    {
        get { return jumpCount; }
        set { jumpCount = value; }
    }

    public bool CanJump()
    {
        return jumpCount < 2;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public void Attack()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        if (isRanged)
        {
            if ((facingRight && mousePos.x < projectileSpawnPoint.position.x) || (!facingRight && mousePos.x > projectileSpawnPoint.position.x))
            {
               return;
            }
            
        }
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            rb.velocity = Vector2.zero;  // Stop movement   
            if (isRanged)
            {
                FSM.RequestStateChange("Ranged");
            }
            else
            {
                SetAttackColliderActive(true);
                FSM.RequestStateChange("Attack");
            }
            
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage, ulong attackerId)
    {
        // Prevent friendly fire (do not take damage from the same owner)
        if (attackerId == OwnerId)
            return;

        Health -= damage;
        if (Health <= 0)
        {
            this.DisableControls();
            FSM.RequestStateChange("Death");
            NotifyDeathClientRpc();
        }
        else
        {
            FSM.RequestStateChange("Hit");
        }
    }

    [ClientRpc]
    private void NotifyDeathClientRpc()
    {
        FSM.RequestStateChange("Death");
        this.DisableControls();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            ResetJumpCount();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public void SetAttackColliderActive(bool isActive)
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = isActive; // Enable or disable the attack collider
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player) && isAlive)
        {
            if (player.OwnerId != OwnerId)
            {
                player.TakeDamageServerRpc(10, OwnerId); // Pass the attacker's OwnerId to the target
            }
        }
    }

    public void DisableControls()
    {
        this.audioSource.enabled = false;
        this.enabled = false;
        this.isAlive = false;
        playerCollider.isTrigger = true;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void UnlockControls()
    {   
        this.audioSource.enabled = true;
        this.enabled = true;
        this.isAlive = true;
        playerCollider.isTrigger = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}