using UnityEngine;
public class AttackSystem : ISystem
{
    public void Tick(EntityManager manager, float deltaTime)
    {
        foreach (var (entityID, attackComp) in manager.GetAllOfType<AttackComponent>())
        {
            if (!manager.HasComponent<AttackTargetComponent>(entityID)) continue;
            var attackTargetComp = manager.GetComponent<AttackTargetComponent>(entityID);
           
            if (!EntityManager.IsValid(attackTargetComp.targetEntityID))
                continue;

            if (manager.HasComponent<DeathFlagComponent>(attackTargetComp.targetEntityID)) 
                continue;

            if (manager.HasComponent<AttackingFlagComponent>(entityID))
                continue;

            if (manager.HasComponent<AttackMeleeComponent>(entityID))
            {
                ProcessMeleeAttack(manager, entityID, attackTargetComp.targetEntityID, attackComp);
                continue;
            }

            if (manager.HasComponent<AttackRangedComponent>(entityID))
            {
                if (manager.HasComponent<ProjectilePendingComponent>(entityID))
                    continue;

                var attackRangeComp = manager.GetComponent<AttackRangedComponent>(entityID);
                ProcessRangedAttack(manager, entityID, attackRangeComp, attackTargetComp.targetEntityID);
                continue;
            }
        }
    }

    private void ProcessMeleeAttack(EntityManager inManager, int inEntityID, int inTargetEntityID, AttackComponent inAttackComp)
    {
        var myObject = inManager.GetComponent<GameObjectRefComponent>(inEntityID);
        var targetObject = inManager.GetComponent<GameObjectRefComponent>(inTargetEntityID);
       
        myObject.gameObject.transform.LookAt(targetObject.gameObject.transform);
        inManager.AddComponents(inEntityID, new AttackingFlagComponent(),
            new AttackAnimationTriggerComponent
            {
                triggerEvent =
                (manager, entity, effectRefComp) =>
                {
                    var animatroRefComp = manager.GetComponent<EntityAnimatorRefComponent>(entity);
                    animatroRefComp.entityAnimator.PlayHitEffectClientRpc(new EffectDataComponent
                    {
                        effectNameKey = "Hit",
                        position = effectRefComp.entityEffector.GetEffectTransformPos()
                    });

                    manager.AddComponents(inTargetEntityID
                        , new GetHitTriggerComponent()
                        , new TakeDamageComponent {amount = inAttackComp.attackDamage });
                }
            });
    }

    private void ProcessRangedAttack(EntityManager inManager, int inEntityID, AttackRangedComponent inAttackRangedComponent, int inTargetEntityID)
    {
        var entityObjRefComp = inManager.GetComponent<GameObjectRefComponent>(inEntityID);
        var targetObjRefComp = inManager.GetComponent<GameObjectRefComponent>(inTargetEntityID);

        GameObject attackObject = entityObjRefComp.gameObject;
        GameObject targetObject = targetObjRefComp.gameObject;

        if (inManager.HasComponent<BuildingUnitRefComponent>(inEntityID))
        {
            var buildingUnitComp = inManager.GetComponent<BuildingUnitRefComponent>(inEntityID);
            attackObject = buildingUnitComp.buildingUnit.gameObject;
            LookAtWithoutPitch(attackObject.transform, targetObject.transform.position);
        }
        else
        {
            attackObject.transform.LookAt(targetObject.transform);
        }

        inManager.AddComponents(inEntityID, new AttackingFlagComponent(),
         new AttackAnimationTriggerComponent
         {
             triggerEvent =
             (manager, entity, effectRefComp) =>
             {
                 manager.AddComponents(inEntityID
                     , new ProjectilePendingComponent()
                     , new ProjectileSpawnDataComponent
                     {
                         attackEntityID = inEntityID,
                         targetEntityID = inTargetEntityID,
                         projectileEntityData = inAttackRangedComponent.projectileEntityData,
                         position = effectRefComp.entityEffector.GetEffectTransformPos(),
                         projectileMoveLength = inAttackRangedComponent.attackLength
                     });
             }
         });
    }
    private void LookAtWithoutPitch(Transform self, Vector3 targetPosition)
    {
        Vector3 lookPos = targetPosition;
        lookPos.y = self.position.y;
        self.LookAt(lookPos);
    }
}

