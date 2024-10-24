using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    // A dictionary to store keys and associated prefabs (set in the Unity editor)
    public Transform spawnpoint;
    [SerializeField] private List<KeyPrefabPair> keyPrefabList = new List<KeyPrefabPair>();
    private Dictionary<KeyCode, GameObject> prefabDictionary = new Dictionary<KeyCode, GameObject>();

    private void Start()
    {
        // Populate the dictionary from the list
        foreach (var pair in keyPrefabList)
        {
            prefabDictionary[pair.key] = pair.prefab;
        }
    }

    private void Update()
    {        
            // Check for any key press in the dictionary
            foreach (var key in prefabDictionary.Keys)
            {
                if (Input.GetKeyDown(key))
                {
                    RequestPrefabChangeServerRpc(key);
                    break;
                }
            }
        
    }

    // ServerRpc to handle the prefab change request from the client
    [ServerRpc(RequireOwnership = false)]
    private void RequestPrefabChangeServerRpc(KeyCode key, ServerRpcParams rpcParams = default)
    {
        // Check if the key is associated with a prefab
        if (prefabDictionary.ContainsKey(key))
        {
            // Get the client that sent the request
            ulong clientId = rpcParams.Receive.SenderClientId;

            // Find the player's current object based on clientId
            foreach (var player in FindObjectsOfType<PlayerController>())
            {
                if (player.OwnerClientId == clientId)
                {
                    // Delete the current player object
                    player.GetComponent<NetworkObject>().Despawn(true); // Ensure it's removed across all clients
                    break;
                }
            }

            // Spawn the new prefab
            GameObject newPlayerPrefab = prefabDictionary[key];
            GameObject newPlayer = Instantiate(newPlayerPrefab, spawnpoint.position, Quaternion.identity);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true); // Spawn and assign ownership to the client
        }
    }
}

// Class to associate a key with a prefab in the Unity editor
[System.Serializable]
public class KeyPrefabPair
{
    public KeyCode key;
    public GameObject prefab;
}
