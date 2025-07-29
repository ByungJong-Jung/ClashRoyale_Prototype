using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<EntityManager>(Lifetime.Singleton);

        builder.Register<TargetingSystem>(Lifetime.Singleton);
        builder.Register<MoveSystem>(Lifetime.Singleton);
        builder.Register<AttackSystem>(Lifetime.Singleton);
        builder.Register<DamageSystem>(Lifetime.Singleton);
        builder.Register<AnimationSystem>(Lifetime.Singleton);
        builder.Register<EffectSystem>(Lifetime.Singleton);
        builder.Register<ProjectileSpawnSystem>(Lifetime.Singleton);
        builder.Register<ProjectileMoveSystem>(Lifetime.Singleton);
        builder.Register<HealthBarSystem>(Lifetime.Singleton);
        builder.Register<DestroySystem>(Lifetime.Singleton);

        builder.Register<EntityFactory>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<UnitPlacer>();
        builder.RegisterComponentInHierarchy<InGameManager>();
        builder.RegisterComponentInHierarchy<EnemySpawner>();

        // (선택) 유닛 생성, 버튼 처리, 스폰 매니저 등도 여기에 등록 가능
        // builder.RegisterComponentInHierarchy<SomeMonoBehaviour>();
    }
}
