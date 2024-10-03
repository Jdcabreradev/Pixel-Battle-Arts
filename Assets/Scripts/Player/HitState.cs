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
        playerController.Animator.Play("Hit");
    }

    public override void OnLogic()
    {
        // Controls are locked, so no movement or input is processed
    }

    public override void OnExit()
    {

    }
}
