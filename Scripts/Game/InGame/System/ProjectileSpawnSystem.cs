
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

            var gameObjComp = manager.GetComponent<GameObjectRefComponent>(shooterEntityID);
            int teamIndex = 0; 
            if(manager.HasComponent<UnitTagComponent>(shooterEntityID))
            {
                teamIndex = gameObjComp.gameObject.GetComponent<Unit>().TeamIndex.Value;
            }
            else if(manager.HasComponent<BuildingTagComponent>(shooterEntityID))
            {
                teamIndex = gameObjComp.gameObject.GetComponent<Building>().TeamIndex.Value;
            }

            NetworkService.Instance.RequestSpawnProjectile(
                  spawnDataComp.projectileEntityData.entityName
                , spawnDataComp.projectileEntityData.resourcePath
                , spawnDataComp.position, teamIndex
                , new ProjectileMoveComponent()
                {
                    attackEntityID = spawnDataComp.attackEntityID,
                    targetEntityID = spawnDataComp.targetEntityID,
                    speed = spawnDataComp.projectileEntityData.moveSpeed,
                    damage = spawnDataComp.projectileEntityData.attackDamage,
                    moveLength = spawnDataComp.projectileMoveLength
                });

            removeList.Add(shooterEntityID);
        }

        foreach (var entity in removeList)
        {
            manager.RemoveComponent<ProjectileSpawnDataComponent>(entity);
        }
    }
}
