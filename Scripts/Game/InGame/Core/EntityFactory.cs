
using UnityEngine;
using System.Collections.Generic;
using VContainer;
public class EntityFactory
{
    [Inject] private EntityManager _entityManager;
    public int CreateEntityUnit(EntityData inEntityData, out GameObject outObject, Vector3 inSpawnPos, ETeamType inTeamType = ETeamType.Ally)
    {
        int entityID = _entityManager.CreateEntity();
        GameObject obj = ObjectPoolManager.Instance.GetPoolingObjects(inEntityData.resourcePath).Dequeue(null, entityID);
        obj.transform.position = inSpawnPos;
        obj.transform.rotation = Quaternion.Euler(Vector3.zero);
        outObject = obj;

        var unit = outObject.GetComponent<Unit>();
        Color entityColor = inTeamType.Equals(ETeamType.Ally) ? InGameData.TEAM_BLUE : InGameData.TEAM_RED;
        unit.ChangeColor(entityColor);

        _entityManager.AddComponent(entityID, new TeamComponent { teamType = inTeamType });
        SetComponentsByEntityType(inEntityData, outObject, entityID);

        _entityManager.AddComponent(entityID, new ReadyToSpawnComponent());

        return entityID;
    }

    public void CreateEntityBuildings(EntityData inEntityData, GameObject inBuildingObject, ETeamType inTeamType = ETeamType.Ally)
    {
        GameObject obj = inBuildingObject;
        inBuildingObject = obj;

        var building = inBuildingObject.GetComponent<Building>();
        Color entityColor = inTeamType.Equals(ETeamType.Ally) ? InGameData.TEAM_BLUE : InGameData.TEAM_RED;
        building.ChangeColor(entityColor);

        var entityID = _entityManager.CreateEntity();
        _entityManager.AddComponent(entityID, new TeamComponent { teamType = inTeamType });

        SetComponentsByEntityType(inEntityData, inBuildingObject, entityID);
    }

    public int CreateEntityProjectile(EntityData inEntityData, out GameObject outObject, ETeamType inTeamType = ETeamType.Ally)
    {
        var entityID = _entityManager.CreateEntity();

        GameObject obj = ObjectPoolManager.Instance.GetPoolingObjects(inEntityData.resourcePath).Dequeue(null, entityID);
        outObject = obj;

        var projectile = outObject.GetComponent<Projectile>();
        Color entityColor = inTeamType.Equals(ETeamType.Ally) ? InGameData.TEAM_BLUE : InGameData.TEAM_RED;
        projectile.ChangeColor(entityColor);

        _entityManager.AddComponent(entityID, new TeamComponent { teamType = inTeamType });

        SetComponentsByEntityType(inEntityData, outObject, entityID);
        return entityID;
    }

    public void SetComponentsByEntityType(EntityData inData, GameObject inEntityObject, int inEntityID)
    {
        _entityManager.AddComponents(inEntityID
            , new MetaComponent { entityName = inData.entityName }
            , new GameObjectRefComponent { gameObject = inEntityObject, resourcePath = inData.resourcePath });

        switch (inData.entityType)
        {
            case EEntitiyType.Unit:
                {
                    _entityManager.AddComponents(inEntityID
                        , new UnitTagComponent()
                        , new MoveComponent { speed = inData.moveSpeed }
                        , new PositionComponent { value = inEntityObject.transform.position.SetY(0f) }
                        , new HealthComponent { hp = inData.hp, maxHp = inData.hp });

                    var unit = inEntityObject.GetComponent<Unit>();
                    if (unit != null)
                    {
                        _entityManager.AddComponents(inEntityID
                            , new NavMeshAgentRefComponent { agent = unit.Agent, lastRequestedDestination = InGameData.INFINITY_POS }
                            , new AttackComponent { attackStopDistance = unit.Agent.stoppingDistance, attackDamage = inData.attackDamage, attackRange = inData.attackRange, targetDetectionRange = inData.targetDetectionRange }
                            , new EntityAnimatorRefComponent { entityAnimator = unit.EntityAnimator }
                            , new EntityEffectorRefComponent { entityEffector = unit }
                            , new HealthBarUIRefComponent { uiObject = unit.HealthBar, fillImage = unit.HpImage });
                    }


                    SetComponentsByUnitType(inData, inEntityObject, inEntityID);
                    SetComponentsByUnitAttackType(inData, inEntityObject, inEntityID);
                }

                break;

            case EEntitiyType.Building:
                {
                    _entityManager.AddComponents(inEntityID
                        , new BuildingTagComponent()
                        , new PositionComponent { value = inEntityObject.transform.position }
                        , new HealthComponent { hp = inData.hp, maxHp = inData.hp });

                    var building = inEntityObject.GetComponent<Building>();

                    if (building != null)
                    {
                        _entityManager.AddComponents(inEntityID
                            , new TargetableTagComponent { targetingSize = building.CalculateSize() }
                            , new EntityEffectorRefComponent { entityEffector = building }
                            , new HealthBarUIRefComponent { uiObject = building.HealthBar, fillImage = building.HpImage });

                        var buildingUnit = building.BuildingUnit;

                        if (buildingUnit != null)
                        {
                            var entityAnimator = buildingUnit.EntityAnimator;
                            float attackAnimLength = entityAnimator.GetAnimatorLength("Attack");
                            float attackAnimEventLength = entityAnimator.GetAnimatorEffectEventLength("Attack");
                            attackAnimLength = attackAnimLength - attackAnimEventLength;

                            _entityManager.AddComponents(inEntityID
                                 , new EntityAnimatorRefComponent { entityAnimator = building.BuildingUnit.EntityAnimator }
                                 , new AttackComponent { attackDamage = inData.attackDamage, attackRange = inData.attackRange, targetDetectionRange = inData.targetDetectionRange }
                                 , new AttackRangedComponent { projectileEntityData = inData.projectileEntityData, attackLength = attackAnimLength }
                                 , new BuildingUnitRefComponent { buildingUnit = buildingUnit });
                        }
                        else
                        {
                            _entityManager.AddComponent(inEntityID, new EntityAnimatorRefComponent { });
                        }
                    }
                }

                break;

            case EEntitiyType.Projectile:
                {
                    _entityManager.AddComponents(inEntityID
                        , new ProjectileTagComponent()
                        , new PositionComponent { value = inEntityObject.transform.position });

                    var projectile = inEntityObject.GetComponent<Projectile>();

                    if (projectile != null)
                    {
                        _entityManager.AddComponent(inEntityID, new EntityEffectorRefComponent { entityEffector = projectile });
                    }
                }

                break;
        }
    }

    public void SetComponentsByUnitType(EntityData inData, GameObject inEntityObject, int inEntityID)
    {
        EUnitType unitType = inData.unityType;

        if ((unitType & EUnitType.Normal) != 0)
        {
            var unit = inEntityObject.GetComponent<Unit>();

            _entityManager.AddComponent(inEntityID
                , new TargetableTagComponent { targetingSize = unit.GetAgentSize() });
        }

        if ((unitType & EUnitType.OnlyBuildingAttack) != 0)
        {
            _entityManager.AddComponent(inEntityID, new AttackOnlyBuildingComponent());
        }

        if ((unitType & EUnitType.Air) != 0)
        {
            _entityManager.AddComponent(inEntityID, new AirUnitTagComponent());

        }

        if ((unitType & EUnitType.Invisible) != 0)
        {

        }

        if ((unitType & EUnitType.Hero) != 0)
        {

        }
    }

    public void SetComponentsByUnitAttackType(EntityData inData, GameObject inEntityObject, int inEntityID)
    {
        switch (inData.attackType)
        {
            case EAttackType.Melee:
                {
                    _entityManager.AddComponent(inEntityID, new AttackMeleeComponent { });
                }

                break;

            case EAttackType.Ranged:
                {
                    var entityAnimator = inEntityObject.GetComponent<Unit>().EntityAnimator;
                    float attackAnimLength = entityAnimator.GetAnimatorLength("Attack");
                    float attackAnimEventLength = entityAnimator.GetAnimatorEffectEventLength("Attack");
                    attackAnimLength = attackAnimLength - attackAnimEventLength;

                    _entityManager.AddComponent(inEntityID, new AttackRangedComponent { projectileEntityData = inData.projectileEntityData, attackLength = attackAnimLength });
                }

                break;
        }
    }


}
