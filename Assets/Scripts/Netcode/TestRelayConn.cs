// Ajustado para asegurarse de que cada cliente sepa cuál es su jugador
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
using Unity.VisualScripting;

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

    private string joinCode;
    private bool hostPlayerSpawned = false; // Controla si el jugador del host ya fue spawneado

    private async void Start()
    {
        // Inicializar servicios y autenticar al usuario de manera anónima
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Asignar listeners a los botones
        hostBtn.onClick.AddListener(async () => await CreateRelay());
        clientBtn.onClick.AddListener(async () => await JoinRelay());

        // Suscribirse al evento de conexión de cliente
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
        // Solo el servidor (host) debe manejar el spawn de los personajes
        if (NetworkManager.Singleton.IsServer)
        {
            // Evitar que el host spawnee su jugador dos veces
            if (clientId == NetworkManager.Singleton.LocalClientId && !hostPlayerSpawned)
            {
                // Spawn del host la primera vez
                SpawnPlayer(clientId);
                hostPlayerSpawned = true;
            }
            else if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                // Spawn de cualquier cliente que no sea el host
                SpawnPlayer(clientId);
            }
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

        // Spawnear el objeto como jugador en la red y asociarlo con el cliente
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            // Spawnear el objeto como el jugador de ese cliente específico
            networkObject.SpawnAsPlayerObject(clientId);
        }
        else
        {
            Debug.LogError("No se encontró NetworkObject en el prefab de jugador.");
        }
    }

    // Obtener el siguiente punto de spawn de manera cíclica
    private Transform GetNextSpawnPoint()
    {
        Transform spawnPoint = spawnPoints[nextSpawnIndex];
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
        return spawnPoint;
    }

    // El host crea una sesión de Relay y comienza el servidor
    public async Task<string> CreateRelay()
    {
        try
        {
            int number;

            if (!int.TryParse(inputField.text, out number))
            {
                number = 4; // Si la conversión falla, asignar el valor por defecto
            }
            // Crear una asignación de relay para el número máximo de jugadores
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(number);

            // Obtener el código de unión para que otros jugadores se puedan unir
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: " + joinCode);

            // Establecer los datos del servidor Relay
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Iniciar el host (servidor)
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host iniciado");

            // El host spawnea su jugador solo si aún no lo ha hecho
            if (!hostPlayerSpawned)
            {
                SpawnPlayer(NetworkManager.Singleton.LocalClientId);
                hostPlayerSpawned = true; // Marcamos que ya se ha spawneado
            }

            inputField.text = joinCode;
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    // El cliente se une a una sesión de Relay
    public async Task JoinRelay()
    {
        try
        {
            // Validar el código de unión
            string enteredJoinCode = inputField.text;
            if (string.IsNullOrEmpty(enteredJoinCode))
            {
                Debug.LogWarning("El código de unión no puede estar vacío");
                return;
            }

            // Unirse a la asignación de relay usando el código de unión
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