
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class HealthBarSystem : ISystem
{
    public void Tick(EntityManager manager, float deltaTime)
    {
        // hp 바를 먼저 킴. 
        var triggerList = new List<int>();
        foreach (var (entityID, healthBarShowComp) in manager.GetAllOfType<HealthBarShowTriggerComponent>())
        {
            if (manager.HasComponent<HealthBarUIRefComponent>(entityID))
            {
                var healthBarComp = manager.GetComponent<HealthBarUIRefComponent>(entityID);
                healthBarComp.entityHealthBar.SetActiveHealthBar(true);

                if (manager.HasComponent<HealthBarActivatedComponent>(entityID) == false)
                    manager.AddComponent(entityID, new HealthBarActivatedComponent());
            }

            triggerList.Add(entityID);
        }

        if (triggerList.Count > 0)
        {
            foreach (var entity in triggerList)
                manager.RemoveComponent<HealthBarShowTriggerComponent>(entity);
        }

        // 체력 처리
        foreach (var (entityID, healthComp) in manager.GetAllOfType<HealthComponent>())
        {
            var healthBarComp = manager.GetComponent<HealthBarUIRefComponent>(entityID);

            if (!manager.HasComponent<HealthBarUIRefComponent>(entityID)) continue;
            healthBarComp.entityHealthBar.ProcessHealthBar(healthComp);

            if (manager.HasComponent<HealthBarActivatedComponent>(entityID))
            {
                healthBarComp.entityHealthBar.ProcessHearBarRotation();
            }
        }
    }

    private void ProcessHearBarRotation(Transform inHealbarTransform)
    {
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        inHealbarTransform.forward = camForward;
    }
}
