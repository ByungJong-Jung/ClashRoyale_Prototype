using UnityEngine;
using VContainer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// 서버 모드 에서는 사용 안함. 
/// </summary>
public class EnemySpawner : Singleton<EnemySpawner> 
{
    //[Inject] private EntityManager _entityManager;
    //private const string UnitCardDataFolder = "ScriptableObject";

    //public List<UnitCardData> enemyCardDeck = new List<UnitCardData>(); // Inspector에서 프리팹 등 할당 가능
    //private IObjectResolver _resolver;

    //// Enemy 전용 엘릭서
    //private float _enemyElixir = 0f;
    //private const float ENEMY_MAX_ELIXIR = 10f;
    //private const float ENEMY_ELIXIR_REGEN_INTERVAL = 2.8f;
  
    //private Queue<UnitCardData> _recentSpawnedQueue = new Queue<UnitCardData>();
    //private const int RECENT_SPAWN_LOCK_COUNT = 9;

    //public IEnumerator Co_Initialzie()
    //{
    //    yield return null;
    //    UnitCardData[] unitCardArray = Resources.LoadAll<UnitCardData>(UnitCardDataFolder).ToArray();
    //    unitCardArray.ShuffleArray();
    //    enemyCardDeck.AddRange(unitCardArray.ToList());


    //    foreach (var unitCard in unitCardArray)
    //    {
    //        var infoList = unitCard.unitCardDataInfoList;

    //        foreach(var info in infoList)
    //        {
    //            ObjectPoolManager.Instance.GetPoolingObjects(info.entityData.resourcePath);
    //            yield return null;
    //        }
    //    }
    //}

    //public void GameStart()
    //{
    //    StartCoroutine(Co_EnemyElixirRegenRoutine());
    //}

    //[Inject]
    //public void Construct(IObjectResolver resolver)
    //{
    //    _resolver = resolver;
    //}

    //private IEnumerator Co_EnemyElixirRegenRoutine()
    //{
    //    while (true)
    //    {
    //        while (_enemyElixir >= ENEMY_MAX_ELIXIR)
    //            yield return null;

    //        float elapsed = 0f;
    //        float interval = ENEMY_ELIXIR_REGEN_INTERVAL;
    //        while (elapsed < interval && _enemyElixir < ENEMY_MAX_ELIXIR)
    //        {
    //            float delta = Time.deltaTime / interval;
    //            _enemyElixir = Mathf.Min(_enemyElixir + delta, ENEMY_MAX_ELIXIR);
    //            elapsed += Time.deltaTime;
    //            yield return null;
    //        }

    //        _enemyElixir = Mathf.Min(Mathf.Floor(_enemyElixir), ENEMY_MAX_ELIXIR);
    //        yield return TryAutoSpawnEnemy();
    //    }
    //}

    //private IEnumerator TryAutoSpawnEnemy()
    //{
    //    // 소환 가능한 카드만 추림
    //    var spawnableList = enemyCardDeck
    //        .Where(card => card.requiredElixir <= _enemyElixir && !_recentSpawnedQueue.Contains(card))
    //        .OrderByDescending(card => card.requiredElixir)
    //        .ToList();

    //    if (spawnableList.Count == 0)
    //        yield break;

    //    int maxElixir = spawnableList[0].requiredElixir;
    //    var maxCostCards = spawnableList.Where(card => card.requiredElixir == maxElixir).ToList();
    //    var cardToSpawn = maxCostCards[Random.Range(0, maxCostCards.Count)];

    //    _recentSpawnedQueue.Enqueue(cardToSpawn);
    //    if (_recentSpawnedQueue.Count > RECENT_SPAWN_LOCK_COUNT)
    //        _recentSpawnedQueue.Dequeue();

    //    Vector3 spawnPos = GetSpawnPositionNearAlly();
    //    foreach (var cardDataInfo in cardToSpawn.unitCardDataInfoList)
    //    {
    //        var factory = _resolver.Resolve<EntityFactory>();
    //        GameObject outObject;

    //        Vector3 spawnPosition = spawnPos + cardDataInfo.spawnOffset;
    //        int entityID = factory.CreateEntityUnit(cardDataInfo.entityData, out outObject, spawnPosition, ETeamType.Enemy);

    //        yield return null;
    //        yield return null;

    //        if (outObject != null)
    //        {
    //            outObject.transform.position = spawnPosition;
    //            outObject.transform.rotation = Quaternion.Euler(Vector3.up * 180f);
    //            outObject.GetComponent<Unit>().SetEnable(true, cardDataInfo.entityData.unityType);
    //        }

    //        _entityManager.RemoveComponent<ReadyToSpawnComponent>(entityID);
    //        yield return null;
    //    }

    //    _enemyElixir -= cardToSpawn.requiredElixir;
    //    _enemyElixir = Mathf.Max(_enemyElixir, 0f);
    //}


    //private Vector3 GetRandomSpawnPosition()
    //{
    //    Vector3 checkPos = InGameData.INFINITY_POS;
    //    do
    //    {
    //        float x = Random.Range(-15f, 15f);
    //        float z = Random.Range(2.5f, 19f);
    //        float y = 0f;
    //        checkPos = new Vector3(x, y, z);
    //    } while (!MapData.Instance.CanEnemySpawn(checkPos));

    //    return checkPos;
    //}

    //private Vector3 GetSpawnPositionNearAlly()
    //{
    //    var allyList = _entityManager.GetAllOfType<TeamComponent>();
    //    List<Vector3> allyPositions = new List<Vector3>();
    //    foreach (var (entityID, teamComp) in allyList)
    //    {
    //        var posComp = _entityManager.GetComponent<PositionComponent>(entityID);

    //        if (_entityManager.HasComponent<UnitTagComponent>(entityID) == false)
    //            continue;

    //        if (_entityManager.HasComponent<BuildingTagComponent>(entityID)
    //            || _entityManager.HasComponent<ProjectileTagComponent>(entityID))
    //            continue;

    //        if (teamComp.teamType.Equals(ETeamType.Ally))
    //            allyPositions.Add(posComp.value);
    //    }
    //    if (allyPositions.Count == 0)
    //        return GetRandomSpawnPosition(); 

    //    const int MAX_ATTEMPTS = 10;
    //    for (int attempt = 0; attempt < MAX_ATTEMPTS; ++attempt)
    //    {
    //        int idx = Random.Range(0, allyPositions.Count);
    //        Vector3 basePos = allyPositions[idx];
    //        float angle = Random.Range(0f, Mathf.PI * 2);
    //        float dist = Random.Range(4f, 6f); 
    //        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;
    //        Vector3 spawnPos = basePos + offset;

    //        if (MapData.Instance.CanEnemySpawn(spawnPos))
    //            return spawnPos;
    //    }

    //    return GetRandomSpawnPosition();
    //}
}
