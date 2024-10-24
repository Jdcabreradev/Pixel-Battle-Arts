using UnityEngine;
using UnityHFSM;

public class JumpState : State
{
    private PlayerController playerController;
    private Rigidbody2D rb;

    // Usamos los mismos valores que en RunState
    private float jumpForce = 10f; // Fuerza del salto

    public JumpState(PlayerController playerController, Rigidbody2D rb)
    {
        this.playerController = playerController;
        this.rb = rb;
    }

    public override void OnEnter()
    {
        // Reproducir animación de salto
        playerController.Animator.Play("Jump");
        playerController.audioSource.PlayOneShot(playerController.jumpClip);

        // Aplicar fuerza de salto solo si no hemos alcanzado el máximo de saltos
        if (playerController.JumpCount < 2)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0); // Reiniciar velocidad en Y para un salto consistente
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            playerController.JumpCount++;
        }
    }

    public override void OnLogic()
    {
        // Permitir movimiento horizontal durante el salto
        HandleMovement();

        // Transición a estado de caída si la velocidad vertical es negativa (cayendo)
        if (rb.velocity.y < 0)
        {
            playerController.FSM.RequestStateChange("Fall");
        }
    }

    public override void OnExit()
    {
        // Limpieza si es necesario
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontalInput * playerController.RunSpeed, rb.velocity.y);
        playerController.Flip(horizontalInput);
    }
}
