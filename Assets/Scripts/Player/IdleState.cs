using UnityEngine;
using UnityHFSM;

public class IdleState : State
{
    private Animator animator;
    private PlayerController playerController;

    public IdleState(Animator animator, PlayerController playerController)
    {
        this.animator = animator;
        this.playerController = playerController;
    }

    public override void OnEnter()
    {
        // Play idle animation
        animator.Play("Idle");
    }

    public override void OnLogic()
    {
        // Transition to RunState if there is horizontal input
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            playerController.FSM.RequestStateChange("Run");
        }

        // Transition to JumpState if jump input is detected
        if (Input.GetKeyDown(KeyCode.Space) && playerController.CanJump())
        {
            playerController.FSM.RequestStateChange("Jump");
        }
    }

    public override void OnExit()
    {
        // Optional: Cleanup when exiting the idle state
    }
}