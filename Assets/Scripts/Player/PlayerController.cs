using UnityEngine;
using Unity.Netcode;
using UnityHFSM;

public class PlayerController : NetworkBehaviour
{
    public StateMachine FSM { get; private set; }
    public Animator Animator { get; private set; }
    private Rigidbody2D rb;
    public float RunSpeed = 5f;
    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool facingRight = true;  // Tracks if the player is facing right
    private Collider2D attackCollider; // Declare attack collider
    private Collider2D playerCollider;

    public int Health = 100; // Player health
    public float attackCooldown = 1f; // Cooldown for attacks
    private float lastAttackTime = -1f; // Tracks the last time an attack was performed
    private bool isAlive = true;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        attackCollider = transform.Find("AttackCollider").GetComponent<Collider2D>();
        SetAttackColliderActive(false); // Ensure it's disabled at start

        FSM = new StateMachine();

        var idleState = new IdleState(Animator, this);
        var runState = new RunState(this, rb);
        var jumpState = new JumpState(this, rb);
        var fallState = new FallState(this, rb);
        var attackState = new AttackState(this);
        var hitState = new HitState(this);
        var deathState = new DeathState(this);

        FSM.AddState("Idle", idleState);
        FSM.AddState("Run", runState);
        FSM.AddState("Jump", jumpState);
        FSM.AddState("Fall", fallState);
        FSM.AddState("Attack", attackState);
        FSM.AddState("Hit", hitState);
        FSM.AddState("Death", deathState);

        FSM.SetStartState("Idle");
        FSM.Init();
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
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            SetAttackColliderActive(true);
            FSM.RequestStateChange("Attack");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            this.DisableControls();
            FSM.RequestStateChange("Death");
            NotifyDeathClientRpc();
        }
        else
        {
            FSM.RequestStateChange("Hit");  // Only change state for hit, no locking
        }
    }

    [ClientRpc]
    private void NotifyDeathClientRpc()
    {
        this.DisableControls();
        FSM.RequestStateChange("Death");
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
        if (other.CompareTag("Enemy")) // Check if the collided object is an enemy
        {
            Debug.Log("attack! sended");
            other.GetComponent<PlayerController>().TakeDamageServerRpc(10); // Replace with your damage logic
        }
    }

    public void DisableControls()
    {
        this.enabled = false;
        playerCollider.isTrigger = true;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void UnlockControls()
    {
        this.isAlive = false;
        playerCollider.isTrigger = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}