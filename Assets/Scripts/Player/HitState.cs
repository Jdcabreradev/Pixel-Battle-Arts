using UnityEngine;
using UnityHFSM;

public class HitState : State
{
    private PlayerController playerController;
    private float hitCooldown = 1f; // Cooldown for hit state
    private float lastHitTime = -1f;

    public HitState(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public override void OnEnter()
    {
        // Play hit animation
        playerController.Animator.Play("Hit");
        lastHitTime = Time.time; // Record the time when the player was hit
    }

    public override void OnLogic()
    {
        // Check if the hit state has finished and if enough time has passed
        if (!playerController.Animator.GetCurrentAnimatorStateInfo(0).IsName("Hit") &&
            Time.time >= lastHitTime + hitCooldown)
        {
            playerController.FSM.RequestStateChange("Idle");
        }
    }

    public override void OnExit()
    {
        // Optional: Reset any hit-related logic here
    }
}
