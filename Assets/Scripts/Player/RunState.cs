using UnityEngine;
using UnityHFSM;

public class RunState : State
{
    private PlayerController playerController;
    private Rigidbody2D rb;

    public RunState(PlayerController playerController, Rigidbody2D rb)
    {
        this.playerController = playerController;
        this.rb = rb;
    }

    public override void OnEnter()
    {
        // Optional: Play running animation
        playerController.Animator.Play("Run");
    }

    public override void OnLogic()
    {
        // Handle horizontal movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontalInput * playerController.RunSpeed, rb.velocity.y);

        // Flip the player based on movement direction
        playerController.Flip(horizontalInput);

        // Check for jump input
        if (Input.GetKeyDown(KeyCode.Space) && playerController.CanJump())
        {
            playerController.FSM.RequestStateChange("Jump");
        }

        // Transition to Idle if no input
        if (horizontalInput == 0)
        {
            playerController.FSM.RequestStateChange("Idle");
        }
    }

    public override void OnExit()
    {
        // Optional: Cleanup
    }
}
