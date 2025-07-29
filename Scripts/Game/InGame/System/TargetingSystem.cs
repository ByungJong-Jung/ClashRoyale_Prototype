using UnityEngine;
using DG.Tweening;

public class TargetingSystem : ISystem
{
    public void Tick(EntityManager manager, float deltaTime)
    {
        foreach (var (entityID,unitTagComp) in manager.GetAllOfType<UnitTagComponent>()) // 타겟팅 주체
        {
            if (manager.HasComponent<ReadyToSpawnComponent>(entityID)) 
                continue;

            if (!manager.HasComponent<PositionComponent>(entityID) || !manager.HasComponent<AttackComponent>(entityID)) 
                continue;

            if (manager.HasComponent<DeathFlagComponent>(entityID) || manager.HasComponent<DestroyComponent>(entityID)) 
                continue;

            var myPosComp = manager.GetComponent<PositionComponent>(entityID);
            var attackComp = manager.GetComponent<AttackComponent>(entityID);

            if (manager.HasComponent<AttackTargetComponent>(entityID))
            {
                var attackTargetComp = manager.GetComponent<AttackTargetComponent>(entityID);
                if (IsInAttackRange(manager, entityID, myPosComp.value, attackTargetComp.targetEntityID)
                    && !manager.HasComponent<DeathFlagComponent>(attackTargetComp.targetEntityID))
                    continue;
            }


            int targetEntityID = FindClosestTarget(manager, entityID, myPosComp.value, attackComp.targetDetectionRange);
            if (manager.HasComponent<MoveTargetComponent>(entityID))
            {
                var existingComp = manager.GetComponent<MoveTargetComponent>(entityID);
                existingComp.targetEntityID = targetEntityID;
                manager.AddComponent(entityID, existingComp);
            }
            else
            {
                manager.AddComponent(entityID, new MoveTargetComponent { targetEntityID = targetEntityID });
            }

            if (EntityManager.IsValid(targetEntityID) && IsInAttackRange(manager,entityID,myPosComp.value,targetEntityID))
            {
                if(manager.HasComponent<AirUnitTagComponent>(entityID))
                {
                    StopAirUnit(manager, entityID, targetEntityID);
                }
                else
                {
                    StopGroundUnit(manager, entityID, targetEntityID);
                }
            }
            else
            {
                manager.RemoveComponent<AttackTargetComponent>(entityID);

                if (EntityManager.IsValid(targetEntityID) == false)
                    manager.RemoveComponent<MoveTargetComponent>(entityID);
            }
        }

        foreach (var (entityID,buildingTagComp) in manager.GetAllOfType<BuildingTagComponent>()) // 타겟팅 주체
        {
            if (!manager.HasComponent<PositionComponent>(entityID) || !manager.HasComponent<AttackComponent>(entityID)) 
                continue;

            var myPosComp = manager.GetComponent<PositionComponent>(entityID);
            var attackComp = manager.GetComponent<AttackComponent>(entityID);

            if (manager.HasComponent<AttackTargetComponent>(entityID)
                    && IsInAttackRange(manager, entityID, myPosComp.value, manager.GetComponent<AttackTargetComponent>(entityID).targetEntityID)
                    && !manager.HasComponent<DeathFlagComponent>(manager.GetComponent<AttackTargetComponent>(entityID).targetEntityID))
                continue;

            int targetEntityID = FindClosestTargetForBuilding(manager, entityID, myPosComp.value, attackComp.targetDetectionRange);

            if (EntityManager.IsValid(targetEntityID) && IsInAttackRange(manager, entityID, myPosComp.value, targetEntityID))
            {
                manager.AddComponent(entityID, new AttackTargetComponent { targetEntityID = targetEntityID });
            }
            else
            {
                manager.RemoveComponent<AttackTargetComponent>(entityID);
            }
        }
    }

    private int FindClosestTarget(EntityManager inManager, int inSeekerEntityID, Vector3 inSeekerPos, float inTargetDetectionRange)
    {
        float minDist = float.MaxValue;
        int closestEntityID = default;

        var myTeam = inManager.HasComponent<TeamComponent>(inSeekerEntityID) ? inManager.GetComponent<TeamComponent>(inSeekerEntityID).teamType : ETeamType.Ally;

        if (inManager.HasComponent<AttackOnlyBuildingComponent>(inSeekerEntityID) == false)
        {
            // 유닛 먼저 탐색
            foreach (var (targetEntityID, targetableTagComp) in inManager.GetAllOfType<TargetableTagComponent>())
            {
                if (targetEntityID == inSeekerEntityID) 
                    continue;

                if (inManager.HasComponent<DeathFlagComponent>(targetEntityID)) 
                    continue;

                if (!inManager.HasComponent<PositionComponent>(targetEntityID)) continue;
                var targetPosComp = inManager.GetComponent<PositionComponent>(targetEntityID);

                var targetTeam = inManager.HasComponent<TeamComponent>(targetEntityID) ? inManager.GetComponent<TeamComponent>(targetEntityID).teamType : ETeamType.Ally;
                if (myTeam.Equals(targetTeam)) 
                    continue;

                if(inManager.HasComponent<AirUnitTagComponent>(targetEntityID))
                {
                    if (inManager.HasComponent<AttackRangedComponent>(inSeekerEntityID) == false)
                        continue;
                }

                // 공격 가능한 범위 내에 있는지만 체크
                float dist = Vector3.Distance(inSeekerPos, targetPosComp.value);
                if (dist < minDist && dist <= inTargetDetectionRange)
                {
                    minDist = dist;
                    closestEntityID = targetEntityID;
                }
            }
        }

        if (!EntityManager.IsValid(closestEntityID))
        {
            minDist = float.MaxValue;
            foreach (var (buildingEntityID,buildingTagComp) in inManager.GetAllOfType<BuildingTagComponent>())
            {
                if (!inManager.HasComponent<PositionComponent>(buildingEntityID)) continue;
                var buildingPosComp = inManager.GetComponent<PositionComponent>(buildingEntityID);

                if (inManager.HasComponent<DeathFlagComponent>(buildingEntityID)) continue;

                var buildingTeam = inManager.HasComponent<TeamComponent>(buildingEntityID) ? inManager.GetComponent<TeamComponent>(buildingEntityID).teamType : ETeamType.Ally;
                if (myTeam.Equals(buildingTeam)) continue;

                float dist = Vector3.Distance(inSeekerPos, buildingPosComp.value);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestEntityID = buildingEntityID;
                }
            }
        }

        return closestEntityID;
    }

    private int FindClosestTargetForBuilding(EntityManager inManager, int inSeekerEntityID, Vector3 inSeekerPos, float inTargetDetectionRange)
    {
        float minDist = float.MaxValue;
        int closestEntityID = default;

        var myTeam = inManager.HasComponent<TeamComponent>(inSeekerEntityID) ? inManager.GetComponent<TeamComponent>(inSeekerEntityID).teamType : ETeamType.Ally;

        if (inManager.HasComponent<AttackOnlyBuildingComponent>(inSeekerEntityID) == false)
        {
            foreach (var (targetEntityID,targetableTagComp) in inManager.GetAllOfType<TargetableTagComponent>())
            {
                if (targetEntityID == inSeekerEntityID) 
                    continue;

                if (inManager.HasComponent<DeathFlagComponent>(targetEntityID)) 
                    continue;

                if (!inManager.HasComponent<PositionComponent>(targetEntityID)) continue;
                var targetPosComp = inManager.GetComponent<PositionComponent>(targetEntityID);

                var targetTeam = inManager.HasComponent<TeamComponent>(targetEntityID) ? inManager.GetComponent<TeamComponent>(targetEntityID).teamType : ETeamType.Ally;
                if (myTeam.Equals(targetTeam)) 
                    continue;

                if (inManager.HasComponent<AirUnitTagComponent>(targetEntityID))
                {
                    if (inManager.HasComponent<AttackRangedComponent>(inSeekerEntityID) == false)
                        continue;
                }

                // 공격 가능한 범위 내에 있는지만 체크
                float dist = Vector3.Distance(inSeekerPos, targetPosComp.value);
                if (dist < minDist && dist <= inTargetDetectionRange)
                {
                    minDist = dist;
                    closestEntityID = targetEntityID;
                }
            }
        }

        return closestEntityID;
    }

    private bool IsInAttackRange(EntityManager manager, int entityID, Vector3 myPos, int targetEntityID)
    {
        if (!manager.HasComponent<AttackComponent>(entityID) || !manager.HasComponent<PositionComponent>(targetEntityID))
            return false;

        var attackComp = manager.GetComponent<AttackComponent>(entityID);
        var targetPosComp = manager.GetComponent<PositionComponent>(targetEntityID);

        if (!manager.HasComponent<TargetableTagComponent>(targetEntityID)) return false;
        var targetable = manager.GetComponent<TargetableTagComponent>(targetEntityID);
        float targetSize = targetable.targetingSize;

        float attackDistance = attackComp.attackStopDistance;
        float attackRange = attackComp.attackRange;

        float dist = Vector3.Distance(myPos, targetPosComp.value);
        return dist <= attackDistance + attackRange + targetSize;
    }


    private void StopGroundUnit(EntityManager inEntityManager, int inEntityID, int inTargetEntityID)
    {
        if (!inEntityManager.HasComponent<NavMeshAgentRefComponent>(inEntityID)) return;
        var agentRefComp = inEntityManager.GetComponent<NavMeshAgentRefComponent>(inEntityID);

        var agent = agentRefComp.agent;
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        agentRefComp.agent.isStopped = true;
        agentRefComp.agent.ResetPath();
        agentRefComp.agent.velocity = Vector3.zero;

        agentRefComp.lastRequestedDestination = InGameData.INFINITY_POS;
        inEntityManager.RemoveComponent<MoveTargetComponent>(inEntityID);
        inEntityManager.AddComponents(inEntityID
            , agentRefComp
            , new AttackTargetComponent { targetEntityID = inTargetEntityID });
    }


    private void StopAirUnit(EntityManager inEntityManager, int inEntityID, int inTargetEntityID)
    {
        var gameObjRefComp = inEntityManager.GetComponent<GameObjectRefComponent>(inEntityID);
        if (gameObjRefComp.gameObject == null) return;

        gameObjRefComp.gameObject.transform.DOKill();

        if (!inEntityManager.HasComponent<PositionComponent>(inEntityID)) return;
        var posComp = inEntityManager.GetComponent<PositionComponent>(inEntityID);

        posComp.value = gameObjRefComp.gameObject.transform.position.SetY(0f);
        inEntityManager.AddComponent(inEntityID, posComp);

        inEntityManager.RemoveComponent<MoveTargetComponent>(inEntityID);
        inEntityManager.AddComponents(inEntityID
            , gameObjRefComp
            , new AttackTargetComponent { targetEntityID = inTargetEntityID });
    }

}
