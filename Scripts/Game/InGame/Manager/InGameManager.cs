
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using System.Collections;

public class InGameManager : Singleton<InGameManager> , IManager
{
    private IObjectResolver _resolver;
    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        _resolver = resolver;
    }

    public EntityManager manager;
    public EntityFactory unitFactory;

    public TargetingSystem targetingSystem;
    public MoveSystem moveSystem;
    public AttackSystem attackSystem;
    public AnimationSystem animationSystem;
    public ProjectileSpawnSystem projectileSpawnSystem;
    public ProjectileMoveSystem projectileMoveSystem;
    public EffectSystem effectSystem;
    public DamageSystem damageSystem;
    public HealthBarSystem healthBarSystem;
    public DestroySystem destroySystem;

    private List<ISystem> systems = new List<ISystem>();

    public bool IsGameStart { get; private set; }
    public IEnumerator Co_Initialize()
    {
        yield return null;

        manager = _resolver.Resolve<EntityManager>();
        unitFactory = _resolver.Resolve<EntityFactory>();

        // 시스템 순서 중요 !!
        {
            targetingSystem = _resolver.Resolve<TargetingSystem>();
            moveSystem = _resolver.Resolve<MoveSystem>();
            attackSystem = _resolver.Resolve<AttackSystem>();
            projectileSpawnSystem = _resolver.Resolve<ProjectileSpawnSystem>();
            projectileMoveSystem = _resolver.Resolve<ProjectileMoveSystem>();

            animationSystem = _resolver.Resolve<AnimationSystem>();
            // effectSystem = _resolver.Resolve<EffectSystem>();
            healthBarSystem = _resolver.Resolve<HealthBarSystem>();
            damageSystem = _resolver.Resolve<DamageSystem>();
            destroySystem = _resolver.Resolve<DestroySystem>();
        }

        {

            systems.Add(targetingSystem);
            systems.Add(moveSystem);
            systems.Add(attackSystem);
            systems.Add(damageSystem);
            systems.Add(projectileSpawnSystem);
            systems.Add(projectileMoveSystem);

            systems.Add(animationSystem);
            //systems.Add(effectSystem); // 서버 모드에서는 rpc를 이용해서 바로 생성
            systems.Add(healthBarSystem);
            systems.Add(destroySystem);
        }

        // 초기화. 
        var initMaterial = InGameData.HitEffectMaterial;
    }

    public void GameStart()
    {
        NetworkService.Instance.SetupPlayerCamera();
        IsGameStart = true;
    }

    void Update()
    {
        if (!IsGameStart)
            return;
        

        if (!Unity.Netcode.NetworkManager.Singleton.IsHost)
            return;

        float dt = Time.deltaTime;

        foreach (var sys in systems)
            sys.Tick(manager, dt);

    }
}
