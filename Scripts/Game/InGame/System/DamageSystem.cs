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
            if (manager.HasComponent<DeathFlagComponent>(entityID)) continue;

            // HP 처리
            if (!manager.HasComponent<HealthComponent>(entityID)) continue;
            var healthComp = manager.GetComponent<HealthComponent>(entityID);

            healthComp.hp -= damageComp.amount;
            if (healthComp.hp < 0f)
                healthComp.hp = 0f;

            manager.AddComponent(entityID, healthComp);

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
                            var obj = manager.GetComponent<GameObjectRefComponent>(entity).gameObject;
                            if(obj != null)
                            {
                                obj.SetActive(false);
                                obj.transform.position = InGameData.INFINITY_POS;
                            }
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

