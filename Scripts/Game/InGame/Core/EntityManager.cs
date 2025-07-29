using System.Collections.Generic;

public class EntityManager
{
    public static bool IsValid(int inEntityID)
    {
        return inEntityID > 0;
    }

    private int _nextId = 1;
    private List<int> _allEntities = new List<int>();
    private SparseSetManager _componentSets = new SparseSetManager();

    public int CreateEntity()
    {
        int id = _nextId++;
        _allEntities.Add(id);
        return id;
    }

    public void RemoveEntity(int entityId)
    {
        _allEntities.Remove(entityId);

        foreach (var set in _componentSets.AllSets)
            set.Remove(entityId);
    }

    public void AddComponent<T>(int inEntityId, in T inComp) where T : struct
        => _componentSets.GetSet<T>().Add(inEntityId, inComp);

    public bool HasComponent<T>(int inEntityId) where T : struct
        => _componentSets.GetSet<T>().Has(inEntityId);

    public T GetComponent<T>(int inEntityId) where T : struct
        => _componentSets.GetSet<T>().Get(inEntityId);

    public void RemoveComponent<T>(int inEntityId) where T : struct
        => _componentSets.GetSet<T>().Remove(inEntityId);


    public void AddComponents<T1, T2>(int inEntityId, in T1 inComp1, in T2 inComp2)
        where T1 : struct where T2 : struct
    {
        AddComponent(inEntityId, inComp1);
        AddComponent(inEntityId, inComp2);
    }
    public void AddComponents<T1, T2, T3>(int inEntityId, in T1 inComp1, in T2 inComp2, in T3 inComp3)
        where T1 : struct where T2 : struct where T3 : struct
    {
        AddComponent(inEntityId, inComp1);
        AddComponent(inEntityId, inComp2);
        AddComponent(inEntityId, inComp3);
    }
    public void AddComponents<T1, T2, T3, T4>(int inEntityId, in T1 inComp1, in T2 inComp2, in T3 inComp3, in T4 inComp4)
        where T1 : struct where T2 : struct where T3 : struct where T4 : struct
    {
        AddComponent(inEntityId, inComp1);
        AddComponent(inEntityId, inComp2);
        AddComponent(inEntityId, inComp3);
        AddComponent(inEntityId, inComp4);
    }
    public void AddComponents<T1, T2, T3, T4, T5>(int inEntityId, in T1 inComp1, in T2 inComp2, in T3 inComp3, in T4 inComp4, in T5 inComp5)
        where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        AddComponent(inEntityId, inComp1);
        AddComponent(inEntityId, inComp2);
        AddComponent(inEntityId, inComp3);
        AddComponent(inEntityId, inComp4);
        AddComponent(inEntityId, inComp5);
    }
    public void AddComponents<T1, T2, T3, T4, T5, T6>(int inEntityId, in T1 inComp1, in T2 inComp2, in T3 inComp3, in T4 inComp4, in T5 inComp5, in T6 inComp6)
        where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
    {
        AddComponent(inEntityId, inComp1);
        AddComponent(inEntityId, inComp2);
        AddComponent(inEntityId, inComp3);
        AddComponent(inEntityId, inComp4);
        AddComponent(inEntityId, inComp5);
        AddComponent(inEntityId, inComp6);
    }

    public IEnumerable<(int entityId, T component)> GetAllOfType<T>() where T : struct
        => _componentSets.GetSet<T>().All();
}
