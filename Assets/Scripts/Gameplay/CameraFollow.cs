using UnityEngine;
using Cinemachine;

public class CameraFollow : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // La c�mara virtual de Cinemachine
    private GameObject player; // El jugador que queremos seguir

    void Start()
    {
        // Llamar a la funci�n cada cierto tiempo para revisar si el jugador ha sido spawneado
        InvokeRepeating("CheckForPlayer", 0f, 1f); // Revisa cada 1 segundo
    }

    void CheckForPlayer()
    {
        // Intenta encontrar el jugador en la escena por su tag
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Si el jugador ha sido encontrado, asigna el seguimiento a la c�mara y det�n las repeticiones
            virtualCamera.Follow = player.transform;
            virtualCamera.LookAt = player.transform; // Opcional: si tambi�n deseas que la c�mara mire al jugador
            CancelInvoke("CheckForPlayer");
        }
    }
}