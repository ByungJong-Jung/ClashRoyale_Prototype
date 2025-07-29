
using UnityEngine;
using System.Collections.Generic;
using VContainer;

public class ProjectileSpawnSystem : ISystem
{
    private readonly EntityFactory _entityFactory;
    public ProjectileSpawnSystem(EntityFactory inEntityFactory)
    {
        _entityFactory = inEntityFactory;
    }

    public void Tick(EntityManager manager, float deltaTime)
    {
        var removeList = new List<int>();
        foreach (var (shooterEntityID, spawnDataComp) in manager.GetAllOfType<ProjectileSpawnDataComponent>())
        {
            if (!manager.HasComponent<TeamComponent>(shooterEntityID)) continue;
            var teamComp = manager.GetComponent<TeamComponent>(shooterEntityID);

            // 투사체 엔티티 생성 및 컴포넌트 부착
            int entityID = _entityFactory.CreateEntityProjectile(spawnDataComp.projectileEntityData, out GameObject projectileObject, teamComp.teamType);
            projectileObject.transform.position = spawnDataComp.position;

            manager.AddComponent(entityID
                , new ProjectileMoveComponent()
                {
                    attackEntityID = spawnDataComp.attackEntityID,
                    targetEntityID = spawnDataComp.targetEntityID,
                    speed = spawnDataComp.projectileEntityData.moveSpeed,
                    damage = spawnDataComp.projectileEntityData.attackDamage,
                    moveLength = spawnDataComp.projectileMoveLength
                });

            // 삭제 리스트에 추가 (1회성)
            removeList.Add(shooterEntityID);
        }

        foreach (var entity in removeList)
        {
            manager.RemoveComponent<ProjectileSpawnDataComponent>(entity);
        }
    }
}
