using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
public class DestroySystem : ISystem
{
    public void Tick(EntityManager manager, float deltaTime)
    {
        var removeList = new List<int>();

        foreach (var (entityID, destoyComp) in manager.GetAllOfType<DestroyComponent>())
        {
            if (manager.HasComponent<EntityEffectorRefComponent>(entityID))
            {
                var effectorRefComp = manager.GetComponent<EntityEffectorRefComponent>(entityID);
                effectorRefComp.entityEffector.Clear();
                manager.AddComponent(entityID, effectorRefComp);
            }

            if (manager.HasComponent<GameObjectRefComponent>(entityID))
            {
                var goRef = manager.GetComponent<GameObjectRefComponent>(entityID);
                var netObj = goRef.gameObject.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Despawn();

                //ObjectPoolManager.Instance.PoolDic[goRef.resourcePath].Enqueue(goRef.gameObject);
            }

            removeList.Add(entityID);
        }

        foreach (var entity in removeList)
            manager.RemoveEntity(entity);
    }
}
