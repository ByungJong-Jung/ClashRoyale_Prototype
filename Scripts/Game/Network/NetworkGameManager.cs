using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    private Dictionary<ulong, ETeamType> _playerTeams = new();

    public IReadOnlyDictionary<ulong, ETeamType> PlayerTeams => _playerTeams;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AssignTeams();
        }
    }

    private void AssignTeams()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        if (clients.Count != 2)
        {
            Debug.LogWarning("1vs1 게임이 아니거나 플레이어가 부족합니다.");
            return;
        }

        _playerTeams.Clear();
        _playerTeams[clients[0].ClientId] = ETeamType.Ally;
        _playerTeams[clients[1].ClientId] = ETeamType.Enemy;
    }

    /// <summary>
    /// 특정 클라이언트의 절대 팀 반환
    /// </summary>
    public ETeamType GetPlayerTeam(ulong clientId)
    {
        if (_playerTeams.TryGetValue(clientId, out var team))
            return team;

        return ETeamType.Ally;
    }

    /// <summary>
    /// 내 팀 반환 (절대 팀 기준)
    /// </summary>
    public ETeamType GetMyTeam()
    {
        return GetPlayerTeam(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// 특정 팀이 내 팀인지 판단
    /// </summary>
    public bool IsMine(ETeamType targetTeam)
    {
        return targetTeam == GetMyTeam();
    }

    /// <summary>
    /// 절대 팀을 내 입장에서 변환 (Ally/Enemy)
    /// </summary>
    public ETeamType ToRelativeTeam(ETeamType absoluteTeam)
    {
        return absoluteTeam == GetMyTeam() ? ETeamType.Ally : ETeamType.Enemy;
    }
}
