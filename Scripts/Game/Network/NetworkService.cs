using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;

public class NetworkService : NetworkBehaviour
{
    public static NetworkService Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AssignTeamsToAllBuildings()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            foreach (var building in UnitPlacer.Instance.MapData.Buildings)
            {
                building.TeamIndex.Value = (int)building.TeamType;
            }

            NotifyTeamAssignmentDoneClientRpc();
        }
    }

    [ClientRpc]
    private void NotifyTeamAssignmentDoneClientRpc()
    {
        StartCoroutine(Co_ApplyColorsDelayed());
    }

    private IEnumerator Co_ApplyColorsDelayed()
    {
        yield return null; 
        foreach (var building in UnitPlacer.Instance.MapData.Buildings)
        {
            building.ApplyTeamColor();
        }
    }


    public void SetupPlayerCamera()
    {
        bool isHost = NetworkManager.Singleton.IsHost;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[NetworkService] Main Camera가 존재하지 않습니다.");
            return;
        }

        if (!isHost)
        {
            cam.transform.position = new Vector3(0f, 105f, 32.5f);     
            cam.transform.rotation = Quaternion.Euler(75f, 180f, 0f);   
        }
        else
        {
            cam.transform.position = new Vector3(0f, 105f, -32.5f);    
            cam.transform.rotation = Quaternion.Euler(75f, 0f, 0f);     
        }
    }

    public void RequestSpawnUnit(string inEntityName, string inResourcePath, Vector3 inSpawnPos,int inTeamIndex)
    {
        SpawnUnitServerRpc(inEntityName,inResourcePath, inSpawnPos, inTeamIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc(string inEntityName, string inResourcePath, Vector3 inSpawnPos,int inTeamIndex, ServerRpcParams rpcParams = default)
    {
        GameObject prefab = Resources.Load<GameObject>(inResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[NetworkService] '{inResourcePath}' 경로의 프리팹을 찾을 수 없습니다!");
            return;
        }

        Quaternion rotation;
        if (inTeamIndex == 0) 
            rotation = Quaternion.identity;
        else
            rotation = Quaternion.Euler(Vector3.up * 180f);

        var go = Instantiate(prefab, inSpawnPos, rotation);
        var netObj = go.GetComponent<NetworkObject>();
        var unit = go.GetComponent<Unit>();
  
        if (unit != null && netObj != null)
        {
            unit.SpawnAnimationCompleteEvent = () =>
            {
                var scope = LifetimeScope.Find<GameLifetimeScope>();
                var entityFactory = scope.Container.Resolve<EntityFactory>();
                var entityManager = scope.Container.Resolve<EntityManager>();
                EntityData data = CardDeckManager.Instance.GetEntityData(inEntityName);

                int entityID = entityFactory.CreateEntityUnit(data, unit.gameObject, GetRelation(inTeamIndex));
                unit.SetEnable(true, data.unityType);
                entityManager.RemoveComponent<ReadyToSpawnComponent>(entityID);
            };

            netObj.Spawn(true);
            unit.TeamIndex.Value = inTeamIndex;
        }
    }
    public void RequestSpawnProjectile(string inEntityName, string inResourcePath, Vector3 inSpawnPos, int inTeamIndex, ProjectileMoveComponent inProjectileMoveComponent)
    {
        SpawnProjectileServerRpc(inEntityName, inResourcePath, inSpawnPos, inTeamIndex, inProjectileMoveComponent);
    }

    [ServerRpc(RequireOwnership = false)]

    public void SpawnProjectileServerRpc(string inEntityName, string inResourcePath, Vector3 inSpawnPos, int inTeamIndex, ProjectileMoveComponent inProjectileMoveComponent, ServerRpcParams rpcParams = default)
    {
        GameObject prefab = Resources.Load<GameObject>(inResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[NetworkService][SpawnProjectileServerRpc] '{inResourcePath}' 경로의 프리팹 확인!");
            return;
        }

        var go = Instantiate(prefab, inSpawnPos, Quaternion.identity);
        var netObj = go.GetComponent<NetworkObject>();
        var projectile = go.GetComponent<Projectile>();

        if (projectile != null && netObj != null)
        {
            netObj.Spawn(true);
            projectile.TeamIndex.Value = inTeamIndex;

            var scope = LifetimeScope.Find<GameLifetimeScope>();
            var entityFactory = scope.Container.Resolve<EntityFactory>();
            var entityManager = scope.Container.Resolve<EntityManager>();
            EntityData data = CardDeckManager.Instance.GetEntityData(inEntityName);

            int entityID = entityFactory.CreateEntityProjectile(data, projectile.gameObject, inSpawnPos, GetRelation(inTeamIndex));
            entityManager.AddComponent(entityID, inProjectileMoveComponent);

            projectile.EntityID.Value = entityID;
        }
    }


    public int MyTeamIndex
    {
        get
        {
            return NetworkManager.Singleton.IsHost ? 0 : 1;
        }
    }


    public ETeamType GetRelation(int inTeamIndex)
    {
        return (MyTeamIndex == inTeamIndex) ? ETeamType.Ally : ETeamType.Enemy;
    }


    #region 동기화

    private HashSet<ulong> readyClients = new HashSet<ulong>();
    private NetworkVariable<bool> allPlayersReady = new NetworkVariable<bool>(false);
    public bool AllPlayersReady => allPlayersReady.Value;
    public System.Action StartEvent = null;

    public void NotifyReadyToServer()
    {
        NotifyReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        readyClients.Add(clientId);

        if (readyClients.Count >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            allPlayersReady.Value = true; 
        }
    }

    [ClientRpc]
    private void GameStartClientRpc()
    {
        StartEvent?.Invoke();
    }

    public void GameStart()
    {
        GameStartClientRpc();
    }

    #endregion
}
