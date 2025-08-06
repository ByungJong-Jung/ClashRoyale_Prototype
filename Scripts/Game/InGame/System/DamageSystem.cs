using System.Collections.Generic;
using System.Collections;
using UnityEngine;
public class DamageSystem : ISystem
{
    public void Tick(EntityManager manager, float deltaTime)
    {
        var removeList = new List<int>();
        foreach (var (entityID, damageComp) in manager.GetAllOfType<TakeDamageComponent>())
        {
            // 이미 죽음 처리죽이면 패스
            if (manager.HasComponent<DeathFlagComponent>(entityID)) continue;

            // 1. HP 감소
            if (!manager.HasComponent<HealthComponent>(entityID)) continue;
            var healthComp = manager.GetComponent<HealthComponent>(entityID);

            healthComp.hp -= damageComp.amount;
            if (healthComp.hp < 0f)
                healthComp.hp = 0f;

            manager.AddComponent(entityID, healthComp);

            // 2. HP Bar 표시 트리거 컴포넌트 추가
            if (!manager.HasComponent<HealthBarActivatedComponent>(entityID))
                manager.AddComponent(entityID, new HealthBarShowTriggerComponent());

            // 죽음 처리
            if (healthComp.hp <= 0f && !manager.HasComponent<DeathAnimationTriggerComponent>(entityID))
            {
                var effectorRefComp = manager.GetComponent<EntityEffectorRefComponent>(entityID);
                effectorRefComp.entityEffector?.Clear();

                manager.AddComponents(entityID
                    , effectorRefComp
                    , new DeathFlagComponent()
                    , new DeathAnimationTriggerComponent
                    {
                        triggerEvent =
                        (manager, entity, effectRefComp) =>
                        {
                            var effectorRefComp = manager.GetComponent<EntityEffectorRefComponent>(entity);
                            effectorRefComp.entityEffector.SetActive(false);
                            manager.AddComponent(entity, new DestroyComponent());
                        }
                    });
            }

            removeList.Add(entityID);
        }

        foreach (var entity in removeList)
            manager.RemoveComponent<TakeDamageComponent>(entity);
    }
}

