using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public GameObject[] characterPrefabs; // Prefabs de los diferentes personajes que pueden spawnear
    public Transform spawnPoint; // El punto donde el personaje va a respawnear

    void Update()
    {
        // Verificamos si el jugador presiona una tecla específica para respawnear (por ejemplo, la tecla "R")
        if (IsOwner && Input.GetKeyDown(KeyCode.R))
        {
            RequestRespawnServerRpc();
        }
    }

    // RPC para pedir al servidor que respawnee el personaje
    [ServerRpc]
    void RequestRespawnServerRpc(ServerRpcParams rpcParams = default)
    {
        RespawnCharacterClientRpc(OwnerClientId);
    }

    // RPC que se ejecuta en todos los clientes para destruir y respawnear el personaje
    [ClientRpc]
    void RespawnCharacterClientRpc(ulong clientId, ClientRpcParams rpcParams = default)
    {
        // Buscamos todos los objetos que tengan el componente PlayerController
        var players = FindObjectsOfType<PlayerController>();

        foreach (var player in players)
        {
            // Buscamos el objeto que pertenece al cliente actual (basado en OwnerClientId)
            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                // Destruimos el personaje actual
                Destroy(player.gameObject);

                // Instanciamos un nuevo personaje en el spawnpoint designado
                GameObject newCharacter = Instantiate(characterPrefabs[0], spawnPoint.position, spawnPoint.rotation);
                newCharacter.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

                break; // Rompemos el bucle una vez encontramos el personaje correcto
            }
        }
    }
}