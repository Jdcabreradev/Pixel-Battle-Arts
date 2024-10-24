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
        playerController.Animator.Play("Death");
        playerController.audioSource.PlayOneShot(playerController.deathClip);
    }

    public override void OnLogic()
    {
        // No further logic, player is dead and controls are locked
    }

    public override void OnExit()
    {
        // Handle respawn or cleanup logic here if necessary
    }
}
