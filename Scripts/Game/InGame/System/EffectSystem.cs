
using UnityEngine;
using System.Collections.Generic;
public class EffectSystem : ISystem
{
    public void Tick(EntityManager manager, float deltaTime)
    {
        var removeList = new List<int>();
        foreach (var (entityID, effectComp) in manager.GetAllOfType<EffectDataComponent>())
        {
            string effectPath = ResourceEffectPath.GetEffectResourcePath(effectComp.effectNameKey);
            if (effectPath.IsNullOrEmpty()) continue;

            float particleDuration = 0f;
            GameObject effectObject = null;
            ObjectPoolManager.Instance.GetPoolingObjects(effectPath).Dequeue(
                (effect) =>
                {
                    effectObject = effect;
                    effectObject.transform.position = effectComp.position;
                    particleDuration = effectObject.GetComponent<Effect>()?.GetEffectDuration ?? 0f;
                });

            removeList.Add(entityID);
            ObjectPoolManager.Instance.ReleasePoolingObjects(particleDuration, effectObject, effectPath,
                inComplete: ()=> 
                {
                    effectComp.completeCallback?.Invoke();
                });
        }

        foreach (var entity in removeList)
        {
            manager.RemoveComponent<EffectDataComponent>(entity);
        }
    }
}
