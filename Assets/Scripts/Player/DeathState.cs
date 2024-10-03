using UnityEngine;
using UnityHFSM;

public class DeathState : State
{
    private PlayerController playerController;

    public DeathState(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public override void OnEnter()
    {
        // Play death animation
        playerController.Animator.Play("Death");

        // Disable player controls or any other relevant logic
        playerController.DisableControls(); // Implement this method to disable controls
    }

    public override void OnLogic()
    {
        // Optionally, you can implement logic to handle what happens during death
        // For example, waiting for a certain time before restarting or showing a game over screen
    }

    public override void OnExit()
    {
        // Optional: Cleanup before exiting the death state, if needed
    }
}
