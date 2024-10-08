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
    
    // El número máximo de jugadores se puede gestionar desde el LobbyManager
    private int maxPlayers = 3;
    private string joinCode;

    private async void Start()
    {
        // Inicializamos los servicios y autenticamos al usuario de forma anónima
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Asignamos listeners a los botones
        hostBtn.onClick.AddListener(async () => await CreateRelay());
        clientBtn.onClick.AddListener(async () => await JoinRelay());
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            // Creamos la sesión con Relay para un máximo de jugadores
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            
            // Obtenemos el código para que los jugadores se unan a la sesión
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Código de unión generado: " + joinCode);
            

            // Configuramos los datos del servidor Relay
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Iniciamos el host (servidor)
            NetworkManager.Singleton.StartHost();
            Debug.Log("Se creeooooop");
            return joinCode;

            
        }
        catch (RelayServiceException e) 
        { 
            Debug.LogException(e); 
             return null;
        }
    }

    public async Task JoinRelay()
    {
        try
        {
            // Validamos que el código ingresado no esté vacío
            string enteredJoinCode = inputField.text;
            if (string.IsNullOrEmpty(enteredJoinCode))
            {
                Debug.LogWarning("El código de unión no puede estar vacío");
                return;
            }

            // Unimos al cliente a la sesión Relay utilizando el código proporcionado
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(enteredJoinCode);
            
            // Configuramos los datos del servidor Relay para el cliente
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Iniciamos el cliente
            NetworkManager.Singleton.StartClient();

            // Podrías notificar al LobbyManager para gestionar la UI o las transiciones
            // LobbyManager.Instance.OnClientJoined(); // Ejemplo opcional
        }
        catch (RelayServiceException e) 
        { 
            Debug.LogException(e); 
        }
    }
}
