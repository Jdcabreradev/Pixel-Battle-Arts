using UnityHFSM;

public class HitState : State
{
    private PlayerController playerController;

    public HitState(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public override void OnEnter()
    {
        // Disable controls as soon as hit state is entered
        playerController.DisableControls();
        playerController.Animator.Play("Hit");
    }

    public override void OnLogic()
    {
        // Controls are locked, so no movement or input is processed
    }

    public override void OnExit()
    {
        // Re-enable controls after hit state ends
        playerController.UnlockControls();
    }
}
