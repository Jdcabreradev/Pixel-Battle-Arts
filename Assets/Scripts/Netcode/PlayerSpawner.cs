using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerChanger : NetworkBehaviour
{
    public GameObject[] availablePrefabs; // Lista de prefabs disponibles
    private int currentPrefabIndex = 0;

    private GameObject currentPlayerInstance;
    private Transform spawnPoint; // Guardar� el spawnPoint pasado al script
    private bool isSpawned = false; // Verifica si ya se ha spawneado el jugador

    // M�todo que debes llamar para pasar el spawnPoint inicial
    public void SetSpawnPoint(Transform spawnTransform)
    {
        spawnPoint = spawnTransform;
    }

    private void Update()
    {
        // Aseg�rate de que la red est� conectada antes de hacer cualquier acci�n
        if (!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsServer)
        {
            return; // No hacer nada si no est� conectado o no es servidor
        }

        // Solo el cliente local debe poder cambiar su propio prefab
        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.C))
        {
            ChangePlayerPrefabServerRpc(NetworkManager.Singleton.LocalClientId, currentPrefabIndex);
        }

        // Si no hemos spawneado el jugador, asegurarnos de hacerlo despu�s de la conexi�n
        if (IsLocalPlayer && !isSpawned && NetworkManager.Singleton.IsConnectedClient)
        {
            SpawnInitialPlayer();
        }
    }

    private void SpawnInitialPlayer()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint no ha sido asignado.");
            return;
        }

        ChangePlayerPrefabServerRpc(NetworkManager.Singleton.LocalClientId, currentPrefabIndex);
        isSpawned = true; // Marcar como spawneado
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerPrefabServerRpc(ulong clientId, int prefabIndex)
    {
        // Asegurarse de que solo el cliente local cambie su propio personaje
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return; // Salir si se intenta cambiar el prefab de otro cliente
        }

        // Obtener el objeto actual del cliente
        NetworkObject currentNetworkObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        // Despawner el jugador actual si existe
        if (currentNetworkObject != null)
        {
            currentNetworkObject.Despawn();
            Destroy(currentNetworkObject.gameObject);
        }

        // Verificar que el �ndice del prefab sea v�lido
        if (prefabIndex < 0 || prefabIndex >= availablePrefabs.Length)
        {
            Debug.LogError("�ndice de prefab no v�lido.");
            return;
        }

        // Spawnear el nuevo jugador en el spawnPoint asignado
        GameObject newPlayerPrefab = availablePrefabs[prefabIndex];
        SpawnNewPlayerForClient(clientId, newPlayerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnNewPlayerForClient(ulong clientId, GameObject playerPrefab, Vector3 position, Quaternion rotation)
    {
        // Instanciar el nuevo prefab en la posici�n dada
        GameObject newPlayerInstance = Instantiate(playerPrefab, position, rotation);

        // Asignar ownership del objeto al cliente correcto
        NetworkObject networkObject = newPlayerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);

        // Actualizar la referencia del jugador actual para el cliente local
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            currentPlayerInstance = newPlayerInstance;
        }
    }
}
