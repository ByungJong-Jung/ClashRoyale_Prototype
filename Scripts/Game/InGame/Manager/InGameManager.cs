
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
            damageSystem = _resolver.Resolve<DamageSystem>();
            destroySystem = _resolver.Resolve<DestroySystem>();
            targetingSystem = _resolver.Resolve<TargetingSystem>();
            moveSystem = _resolver.Resolve<MoveSystem>();
            attackSystem = _resolver.Resolve<AttackSystem>();
            projectileSpawnSystem = _resolver.Resolve<ProjectileSpawnSystem>();
            projectileMoveSystem = _resolver.Resolve<ProjectileMoveSystem>();

            animationSystem = _resolver.Resolve<AnimationSystem>();
            effectSystem = _resolver.Resolve<EffectSystem>();
            healthBarSystem = _resolver.Resolve<HealthBarSystem>();
        }

        {
            systems.Add(damageSystem);
            systems.Add(destroySystem);
            systems.Add(targetingSystem);
            systems.Add(moveSystem);
            systems.Add(attackSystem);
            systems.Add(projectileSpawnSystem);
            systems.Add(projectileMoveSystem);

            systems.Add(animationSystem);
            systems.Add(effectSystem);
            systems.Add(healthBarSystem);
        }

        // 초기화.캐싱 임시
        var initMaterial = InGameData.HitEffectMaterial;
    }

    public void GameStart()
    {
        IsGameStart = true;
    }

    void Update()
    {
        if (!IsGameStart)
            return;

        float dt = Time.deltaTime;
        foreach (var sys in systems)
            sys.Tick(manager, dt);
    }
}
