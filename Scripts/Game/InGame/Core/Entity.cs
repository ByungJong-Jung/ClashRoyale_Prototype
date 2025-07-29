using System;
using System.Collections.Generic;
using UnityEngine;

public struct Entity
{
    public int id;
    public bool IsValid => id > 0;
}
public interface IEntitySetRemovable
{
    void Remove(int entityId);
}

public class SparseSet<T> : IEntitySetRemovable  where T : struct
{
    private Dictionary<int, int> _entityToIndex = new Dictionary<int, int>();
    private List<T> _data = new List<T>();

    public void Add(int entityId, in T comp)
    {
        if (!_entityToIndex.ContainsKey(entityId))
        {
            _entityToIndex[entityId] = _data.Count;
            _data.Add(comp);
        }
        else
        {
            _data[_entityToIndex[entityId]] = comp;
        }
    }

    public T Get(int entityId) => _data[_entityToIndex[entityId]];
    public bool Has(int entityId) => _entityToIndex.ContainsKey(entityId);

    public void Remove(int entityId)
    {
        if (!_entityToIndex.TryGetValue(entityId, out int idx)) return;
        int lastIdx = _data.Count - 1;
        if (idx != lastIdx)
        {
            _data[idx] = _data[lastIdx];
            foreach (var kv in _entityToIndex)
            {
                if (kv.Value == lastIdx)
                {
                    _entityToIndex[kv.Key] = idx;
                    break;
                }
            }
        }
        _data.RemoveAt(lastIdx);
        _entityToIndex.Remove(entityId);
    }

    public IEnumerable<(int entityId, T component)> All()
    {
        foreach (var kv in _entityToIndex)
            yield return (kv.Key, _data[kv.Value]);
    }
}

public class SparseSetManager
{
    public IEnumerable<IEntitySetRemovable> AllSets => _sets.Values;
    private Dictionary<Type, IEntitySetRemovable> _sets = new();

    public SparseSet<T> GetSet<T>() where T : struct
    {
        var type = typeof(T);
        if (!_sets.TryGetValue(type, out var set))
        {
            set = new SparseSet<T>();
            _sets[type] = set;
        }
        return (SparseSet<T>)set;
    }
}
