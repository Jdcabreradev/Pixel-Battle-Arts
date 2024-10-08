using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;

public class RelayManager : MonoBehaviour
{
    private int maxPlayers = 3;
    private string joinCode;

    


    public async Task<string> CreateRelay()
    {
        try
        {
            Debug.Log("Si quiera si iniciar esta");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            Debug.Log("Paso la allocation");
            
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Código de unión generado: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            return joinCode; // Retorna el código para que el LobbyManager lo utilice
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
            return null; // Devuelve null si ocurre algún error
        }
    }
    public event Action OnRelayJoined;
    public async Task JoinRelay(string joinCode)
    
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            Debug.Log("Successfully joined Relay");
            OnRelayJoined?.Invoke();

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }
}
