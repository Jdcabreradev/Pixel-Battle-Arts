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
        // Reproducir la animación de correr
        playerController.Animator.Play("Run");
        playerController.audioSource.loop = true;
        playerController.audioSource.clip = playerController.runClip;
        playerController.audioSource.Play();
    }

    public override void OnLogic()
    {
        HandleMovement();

        // Verificar si el jugador quiere saltar
        if (Input.GetKeyDown(KeyCode.Space) && playerController.CanJump())
        {
            playerController.FSM.RequestStateChange("Jump");
        }

        // Transición a Idle si no hay input
        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            playerController.FSM.RequestStateChange("Idle");
        }
    }

    public override void OnExit()
    {
        // Detener el audio de correr
        playerController.audioSource.loop = false;
        playerController.audioSource.Stop();
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontalInput * playerController.RunSpeed, rb.velocity.y);
        playerController.Flip(horizontalInput);
    }
}
