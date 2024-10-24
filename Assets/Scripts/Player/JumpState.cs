using UnityEngine;
using UnityHFSM;

public class JumpState : State
{
    private PlayerController playerController;
    private Rigidbody2D rb;
    private float jumpForce = 9f;

    public JumpState(PlayerController playerController, Rigidbody2D rb)
    {
        this.playerController = playerController;
        this.rb = rb;
    }

    public override void OnEnter()
    {
        // Play jump animation
        playerController.Animator.Play("Jump");
        playerController.audioSource.PlayOneShot(playerController.jumpClip);

        // Apply jump force only on the first jump
        if (playerController.JumpCount == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Increment jump count
        playerController.JumpCount++;
    }

    public override void OnLogic()
    {
        // Allow movement during jump
        HandleMovement();

        // Transition to FallState if falling
        if (rb.velocity.y < 0)
        {
            playerController.FSM.RequestStateChange("Fall");
        }
    }

    public override void OnExit()
    {
        // Cleanup or reset as necessary
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontalInput * playerController.RunSpeed, rb.velocity.y);

        playerController.Flip(horizontalInput);
    }
}