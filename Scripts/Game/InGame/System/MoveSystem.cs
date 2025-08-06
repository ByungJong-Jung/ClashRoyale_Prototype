using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class MoveSystem : ISystem
{
    public void Tick(EntityManager manager, float deltaTime)
    {
        foreach (var (entityID, moveComp) in manager.GetAllOfType<MoveComponent>())
        {
            var posComp = manager.GetComponent<PositionComponent>(entityID);
            var attackComp = manager.GetComponent<AttackComponent>(entityID);

            if (manager.HasComponent<MoveTargetComponent>(entityID))
            {
                var targetComp = manager.GetComponent<MoveTargetComponent>(entityID);

                if (EntityManager.IsValid(targetComp.targetEntityID))
                {
                    if (!manager.HasComponent<PositionComponent>(targetComp.targetEntityID)) continue;
                    var targetPosComp = manager.GetComponent<PositionComponent>(targetComp.targetEntityID);

                    Vector3 targetPos = GetAttackStopPosition(posComp.value, targetPosComp.value, attackComp.attackRange);

                    if (manager.HasComponent<AirUnitTagComponent>(entityID))
                    {
                        MoveAirUnit(manager, entityID, moveComp, posComp, targetPos);
                    }
                    else
                    {
                        MoveGroundUnit(manager, entityID, moveComp, posComp, targetPos);
                    }
                }
            }
        }
    }

    private void MoveAirUnit(EntityManager inManager, int inEntityID, MoveComponent inMoveComp, PositionComponent inPosComp, Vector3 inTargetPos)
    {
        var gameObjRefComp = inManager.GetComponent<GameObjectRefComponent>(inEntityID);
        if (gameObjRefComp.gameObject == null) return;

        Vector3 fromPos = gameObjRefComp.gameObject.transform.position;
        Vector3 toPos = new Vector3(inTargetPos.x, fromPos.y, inTargetPos.z); 

        float moveDuration = CalculateMoveDuration(fromPos, toPos, inMoveComp.speed);
        if (!DOTween.IsTweening(gameObjRefComp.gameObject.transform))
        {
            gameObjRefComp.gameObject.transform.DOKill();
            gameObjRefComp.gameObject.transform.DOMove(toPos, moveDuration)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    Vector3 currentPos = gameObjRefComp.gameObject.transform.position;
                    Vector3 moveDir = (toPos - currentPos).normalized;
                    if (moveDir.sqrMagnitude > 0.0001f)
                        gameObjRefComp.gameObject.transform.forward = moveDir;
                    inPosComp.value = currentPos.SetY(0f);
                    inManager.AddComponents(inEntityID, gameObjRefComp, inPosComp);
                })
                .OnComplete(() =>
                {
                    Vector3 moveDir = (toPos - fromPos).normalized;
                    if (moveDir.sqrMagnitude > 0.0001f)
                        gameObjRefComp.gameObject.transform.forward = moveDir;

                    inPosComp.value = toPos;
                    inManager.AddComponents(inEntityID, gameObjRefComp, inPosComp);
                });
        }

        float CalculateMoveDuration(Vector3 from, Vector3 to, float speed)
        {
            if (speed <= 0f) return 0.01f; 
            float distance = Vector3.Distance(from, to);
            return distance / speed;
        }
    }

    private void MoveGroundUnit(EntityManager inManager, int inEntityID, MoveComponent inMoveComp, PositionComponent inPosComp, Vector3 inTargetPos)
    {
        if (!inManager.HasComponent<NavMeshAgentRefComponent>(inEntityID)) return;
        var agentRef = inManager.GetComponent<NavMeshAgentRefComponent>(inEntityID);


        if (CheckAgent(agentRef.agent) && Vector3.Distance(agentRef.lastRequestedDestination, inTargetPos) > 0.3f)
        {
            agentRef.agent.speed = inMoveComp.speed;
            agentRef.agent.isStopped = false;
            agentRef.agent.SetDestination(inTargetPos);
            agentRef.lastRequestedDestination = inTargetPos;

            inManager.AddComponent(inEntityID, agentRef);
        }

        inPosComp.value = agentRef.agent.transform.position.SetY(0f);
        inManager.AddComponent(inEntityID, inPosComp);
    }

    public static Vector3 GetAttackStopPosition(Vector3 inMyPos, Vector3 inTargetPos, float inAttackRange)
    {
        Vector3 myPosXZ = new Vector3(inMyPos.x, 0f, inMyPos.z);
        Vector3 targetPosXZ = new Vector3(inTargetPos.x, 0f, inTargetPos.z);
        Vector3 dir = (myPosXZ - targetPosXZ).normalized;
        Vector3 stopPosXZ = targetPosXZ + dir * inAttackRange * 0.5f;
        return new Vector3(stopPosXZ.x, inTargetPos.y, stopPosXZ.z);
    }

    public bool CheckAgent(NavMeshAgent inAgent)
    {
        return inAgent != null && inAgent.isActiveAndEnabled && inAgent.isOnNavMesh;
    }
}
