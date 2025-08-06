using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public partial class NetworkController : MonoBehaviour
{
    public async void JoinGameWithCode(string InInputJoinCode)
    {
        if (InInputJoinCode.IsNullOrEmpty())
        {
            Debug.LogError("유효하지 않은 join code");
            return;
        }

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(InInputJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);

            StartClient();
            Debug.Log("join code로 게임에 접속 성공! ");
        }
        catch (RelayServiceException e)
        {
            Debug.Log($"게임 접속 실패 : {e}");
        }
    }

    public async void StartMatchMaking()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("로그인 되지 않았습니다.");
            return;
        }

        currentLobby = await FindAvailableLobby();

        if (currentLobby == null)
        {
            await CreateNewLobby();
        }
        else
        {
            await JoinLobby(currentLobby.Id);
        }
    }

    private async Task<Lobby> FindAvailableLobby()
    {
        try
        {
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            if (queryResponse.Results.Count > 0)
            {
                return queryResponse.Results[0];
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"로비 찾기 실패 : {e}");
        }

        return null;
    }

    private async Task CreateNewLobby()
    {
        try
        {
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("랜덤 매칭 방", MAX_PLAYER);
            Debug.Log($"새로운 방 생성됨 : {currentLobby.Id}");

            await AllocateRelayServerAndJoin(currentLobby);

            StartHost();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"로비 생성 실패 : {e}");
        }
    }

    private async Task JoinLobby(string inLobbyID)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(inLobbyID);
            Debug.Log($"방에 접속 되었습니다 : {currentLobby.Id}");
            StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"로비 찾기 실패 : {e}");
        }
    }

    private async Task AllocateRelayServerAndJoin(Lobby inLobby)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(inLobby.MaxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            _txtJoinCode.text = $"{joinCode}";

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                 allocation.RelayServer.IpV4,
                 (ushort)allocation.RelayServer.Port,
                 allocation.AllocationIdBytes,
                 allocation.Key,
                 allocation.ConnectionData,
                 allocation.ConnectionData // 호스트는 본인 connection data
             );

            Debug.Log($"Relay 서버 할당 완료 . Join Code : {joinCode}");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay 서버 할당 실패 : {e}");
        }
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("호스트 시작");

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnHostDisconnected;
    }

    private void OnClientConnected(ulong inClientID)
    {
        OnPlayerJoined();
    }

    private void OnHostDisconnected(ulong inClientID)
    {
        if(inClientID == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnHostDisconnected;
        }
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("클라이언트 시작");
    }
}
