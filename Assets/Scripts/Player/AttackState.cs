using UnityEngine;
using UnityHFSM;

public class AttackState : State
{
    private PlayerController playerController;

    public AttackState(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public override void OnEnter()
    {
        // Play attack animation
        playerController.Animator.Play("Attack1");
        // Optionally activate a collider for the attack hitbox
        // e.g., attackCollider.SetActive(true);
    }

    public override void OnLogic()
    {
        // Check if the animation is no longer playing
        if (playerController.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            // Transition back to Idle state after the attack finishes
            playerController.SetAttackColliderActive(false);
            playerController.FSM.RequestStateChange("Idle");
        }
    }

    public override void OnExit()
    {
        // Deactivate attack collider if necessary
        // e.g., attackCollider.SetActive(false);
    }
}
