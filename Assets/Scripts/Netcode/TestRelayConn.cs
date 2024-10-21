using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class TestRelayConn : MonoBehaviour
{
    public Button hostBtn;
    public Button clientBtn;
    public TMP_InputField inputField;

    // Assign the player prefab via the Inspector or programmatically
    public GameObject playerPrefab;

    // Assign spawn points via the Inspector
    public Transform[] spawnPoints;
    private int nextSpawnIndex = 0; // Tracks the next spawn point to use

    private int maxPlayers = 3; // Max players for the Relay
    private string joinCode;

    private async void Start()
    {
        // Initialize services and authenticate user anonymously
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Assign listeners to buttons
        hostBtn.onClick.AddListener(async () => await CreateRelay());
        clientBtn.onClick.AddListener(async () => await JoinRelay());

        // Subscribe to client connection event
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // Called when a client connects (either host or client)
    private void OnClientConnected(ulong clientId)
    {
        // Only spawn a player for clients other than the host
        if (NetworkManager.Singleton.IsServer && clientId != NetworkManager.Singleton.LocalClientId)
        {
            SpawnPlayer(clientId);
        }
    }

    // Spawn a player at the next available spawn point
    private void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not assigned!");
            return;
        }

        Transform spawnTransform = GetNextSpawnPoint();

        // Instantiate the player object
        GameObject playerInstance = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    // Get the next spawn point in a round-robin manner
    private Transform GetNextSpawnPoint()
    {
        Transform spawnPoint = spawnPoints[nextSpawnIndex];
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
        return spawnPoint;
    }

    // Host creates a relay session and starts the server
    public async Task<string> CreateRelay()
    {
        try
        {
            // Create a relay allocation for maxPlayers
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            // Get the join code for players to connect
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: " + joinCode);

            // Set relay server data
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Start the host (server)
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started");

            // The host spawns its player immediately after starting the host
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    // Client joins a relay session
    public async Task JoinRelay()
    {
        try
        {
            // Validate the join code
            string enteredJoinCode = inputField.text;
            if (string.IsNullOrEmpty(enteredJoinCode))
            {
                Debug.LogWarning("Join code cannot be empty");
                return;
            }

            // Join the relay allocation using the join code
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(enteredJoinCode);

            // Set relay server data for the client
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Start the client
            NetworkManager.Singleton.StartClient();

            Debug.Log("Client joined");
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }
}