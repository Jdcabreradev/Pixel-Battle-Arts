using UnityHFSM;
using UnityEngine;

public class RangedState : State
{
    private PlayerController playerController;

    public RangedState(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public override void OnEnter()
    {
        // Reproduce la animaci�n de ataque a distancia
        playerController.Animator.Play("Attack1");
    }

    public override void OnLogic()
    {
        // Verifica si la animaci�n ha terminado
        if (playerController.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            // Dispara el proyectil cuando termina la animaci�n
            playerController.ShootProjectile();

            // Vuelve al estado Idle
            playerController.FSM.RequestStateChange("Idle");
        }
    }

    public override void OnExit()
    {
        // L�gica de limpieza si es necesario
    }
}
