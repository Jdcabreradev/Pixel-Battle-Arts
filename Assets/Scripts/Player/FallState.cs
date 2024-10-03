using UnityEngine;
using UnityHFSM;

public class FallState : State
{
    private PlayerController playerController;
    private Rigidbody2D rb;

    public FallState(PlayerController playerController, Rigidbody2D rb)
    {
        this.playerController = playerController;
        this.rb = rb;
    }

    public override void OnEnter()
    {
        // Play fall animation
        playerController.Animator.Play("Fall");
    }

    public override void OnLogic()
    {
        // Allow movement during fall
        HandleMovement();

        // Check if the player has landed to transition back to Run or Idle
        if (playerController.IsGrounded())
        {
            playerController.ResetJumpCount();
            playerController.FSM.RequestStateChange("Run");
        }
    }

    public override void OnExit()
    {
        // Cleanup if necessary
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontalInput * playerController.RunSpeed, rb.velocity.y);
    }
}
