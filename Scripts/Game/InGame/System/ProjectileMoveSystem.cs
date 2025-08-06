using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
public class ProjectileMoveSystem : ISystem
{
    public void Tick(EntityManager manager, float deltaTime)
    {
        var removeEntityList = new List<int>();
        var removeList = new List<int>();
        foreach (var (projectileEntityID, projectileMoveComp) in manager.GetAllOfType<ProjectileMoveComponent>())
        {
            if (!manager.HasComponent<GameObjectRefComponent>(projectileEntityID) ||
                !manager.HasComponent<GameObjectRefComponent>(projectileMoveComp.targetEntityID) ||
                !manager.HasComponent<TargetableTagComponent>(projectileMoveComp.targetEntityID))
                continue;

            var projectileObjRefComp = manager.GetComponent<GameObjectRefComponent>(projectileEntityID);
            var targetObjRefComp = manager.GetComponent<GameObjectRefComponent>(projectileMoveComp.targetEntityID);
            var targetTargetableComp = manager.GetComponent<TargetableTagComponent>(projectileMoveComp.targetEntityID);

            if (manager.HasComponent<DeathFlagComponent>(projectileMoveComp.targetEntityID))
            {
                PoolAndRemove(manager, projectileObjRefComp, projectileEntityID, projectileMoveComp.attackEntityID);
                removeEntityList.Add(projectileEntityID);
                continue;
            }

            GameObject projectileObject = projectileObjRefComp.gameObject;
            Vector3 moveTargetPos = CalculateProjectileTargetPos(
                projectileObject.transform.position,
                targetObjRefComp.gameObject.transform.position,
                targetTargetableComp.targetingSize
            );

            float moveTime = projectileMoveComp.moveLength;

            projectileObject.transform
                .DOMove(moveTargetPos, moveTime)
                .SetEase(Ease.Linear)
                .OnUpdate(() => SetProjectileDirection(projectileObject.transform, moveTargetPos))
                .OnComplete(() =>
                {
                    OnProjectileArrive(manager, projectileEntityID, projectileMoveComp, projectileObjRefComp);
                });

            removeList.Add(projectileEntityID);
        }

        foreach (var entity in removeList)
            manager.RemoveComponent<ProjectileMoveComponent>(entity);

        foreach (var entity in removeEntityList)
            manager.RemoveEntity(entity);
    }

    private Vector3 CalculateProjectileTargetPos(Vector3 inFromPos, Vector3 inTargetCenter, float inTargetRadius)
    {
        Vector3 dir = (inTargetCenter - inFromPos).normalized;
        return inTargetCenter - dir * inTargetRadius;
    }

    private void SetProjectileDirection(Transform inTransform, Vector3 inTargetPos)
    {
        Vector3 dir = (inTargetPos - inTransform.position).normalized;
        inTransform.up = dir;
    }

    private void OnProjectileArrive(EntityManager inManager, int inProjectileEntityId, ProjectileMoveComponent inProjectileMoveComp, GameObjectRefComponent inProjectileObjRefComp)
    {
        if (inManager.HasComponent<EntityEffectorRefComponent>(inProjectileEntityId))
        {
            var projectileEffectRefComp = inManager.GetComponent<EntityEffectorRefComponent>(inProjectileEntityId);
            
            var effectRefComp = inManager.GetComponent<EntityEffectorRefComponent>(inProjectileEntityId);
            effectRefComp.entityEffector.PlayEffects(new EffectDataComponent
            {
                effectNameKey = "Hit",
                position = projectileEffectRefComp.entityEffector.GetEffectTransformPos()
            });

            inManager.AddComponents(inProjectileMoveComp.targetEntityID,
                new GetHitTriggerComponent(),
                new TakeDamageComponent { amount = inProjectileMoveComp.damage }
            );
        }

        PoolAndRemove(inManager, inProjectileObjRefComp, inProjectileEntityId, inProjectileMoveComp.attackEntityID);
        inManager.RemoveEntity(inProjectileEntityId);
    }

    private void PoolAndRemove(EntityManager inManager, GameObjectRefComponent inObjRefComp, int inEntityId, int inAttackEntityId)
    {
        inObjRefComp.gameObject.transform.DOKill();

        var projectileEffectRefComp = inManager.GetComponent<EntityEffectorRefComponent>(inEntityId);
        projectileEffectRefComp.entityEffector.SetActive(false);

        var goRef = inManager.GetComponent<GameObjectRefComponent>(inEntityId);
        var netObj = goRef.gameObject.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Despawn();

        inManager.RemoveComponent<ProjectilePendingComponent>(inAttackEntityId);
    }

}
