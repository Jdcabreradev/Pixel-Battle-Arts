using UnityEngine;
using Cinemachine;

public class CameraFollow : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // La cámara virtual de Cinemachine
    private GameObject player; // El jugador que queremos seguir

    void Start()
    {
        // Llamar a la función cada cierto tiempo para revisar si el jugador ha sido spawneado
        InvokeRepeating("CheckForPlayer", 0f, 1f); // Revisa cada 1 segundo
    }

    void CheckForPlayer()
    {
        // Intenta encontrar el jugador en la escena por su tag
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Si el jugador ha sido encontrado, asigna el seguimiento a la cámara y detén las repeticiones
            virtualCamera.Follow = player.transform;
            virtualCamera.LookAt = player.transform; // Opcional: si también deseas que la cámara mire al jugador
            CancelInvoke("CheckForPlayer");
        }
    }
}