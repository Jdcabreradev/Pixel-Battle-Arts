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
        // Check if the animation is no longer playing
        if (playerController.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            playerController.FSM.RequestStateChange("Idle");
        }
    }

    public override void OnExit()
    {

    }
}
