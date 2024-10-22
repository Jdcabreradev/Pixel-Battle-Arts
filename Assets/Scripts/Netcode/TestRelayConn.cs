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

    // Lista de prefabs de jugadores asignados desde el Inspector
    public List<GameObject> playerPrefabs;

    // Puntos de spawn asignados desde el Inspector
    public Transform[] spawnPoints;
    private int nextSpawnIndex = 0; // Rastrea el siguiente punto de spawn a utilizar

    private int maxPlayers = 3; // M�ximo n�mero de jugadores en la partida
    private string joinCode;
    private bool hostPlayerSpawned = false; // Controla si el jugador del host ya fue spawneado

    private async void Start()
    {
        // Inicializar servicios y autenticar al usuario de manera an�nima
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Asignar listeners a los botones
        hostBtn.onClick.AddListener(async () => await CreateRelay());
        clientBtn.onClick.AddListener(async () => await JoinRelay());

        // Suscribirse al evento de conexi�n de cliente
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento cuando se destruye el objeto
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // Se llama cuando un cliente se conecta (incluyendo el host)
    private void OnClientConnected(ulong clientId)
    {
        // El servidor spawnea el jugador solo para los clientes que no sean el host
        if (NetworkManager.Singleton.IsServer && clientId != NetworkManager.Singleton.LocalClientId)
        {
            SpawnPlayer(clientId);
        }
    }

    // Spawnear un jugador en el siguiente punto de spawn disponible
    private void SpawnPlayer(ulong clientId)
    {
        if (playerPrefabs == null || playerPrefabs.Count == 0)
        {
            Debug.LogError("No hay prefabs de jugador asignados!");
            return;
        }

        // Obtener un prefab de jugador aleatorio
        GameObject randomPlayerPrefab = playerPrefabs[Random.Range(0, playerPrefabs.Count)];

        // Obtener el siguiente punto de spawn
        Transform spawnTransform = GetNextSpawnPoint();

        // Instanciar el objeto jugador
        GameObject playerInstance = Instantiate(randomPlayerPrefab, spawnTransform.position, spawnTransform.rotation);

        // Spawnear el objeto como jugador en la red
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    // Obtener el siguiente punto de spawn de manera c�clica
    private Transform GetNextSpawnPoint()
    {
        Transform spawnPoint = spawnPoints[nextSpawnIndex];
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
        return spawnPoint;
    }

    // El host crea una sesi�n de Relay y comienza el servidor
    public async Task<string> CreateRelay()
    {
        try
        {
            // Crear una asignaci�n de relay para el n�mero m�ximo de jugadores
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            // Obtener el c�digo de uni�n para que otros jugadores se puedan unir
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: " + joinCode);

            // Establecer los datos del servidor Relay
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Iniciar el host (servidor)
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host iniciado");

            // El host spawnea su jugador solo si a�n no lo ha hecho
            if (!hostPlayerSpawned)
            {
                SpawnPlayer(NetworkManager.Singleton.LocalClientId);
                hostPlayerSpawned = true; // Marcamos que ya se ha spawneado
            }

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    // El cliente se une a una sesi�n de Relay
    public async Task JoinRelay()
    {
        try
        {
            // Validar el c�digo de uni�n
            string enteredJoinCode = inputField.text;
            if (string.IsNullOrEmpty(enteredJoinCode))
            {
                Debug.LogWarning("El c�digo de uni�n no puede estar vac�o");
                return;
            }

            // Unirse a la asignaci�n de relay usando el c�digo de uni�n
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(enteredJoinCode);

            // Establecer los datos del servidor Relay para el cliente
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Iniciar el cliente
            NetworkManager.Singleton.StartClient();

            Debug.Log("Cliente unido");
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }
}