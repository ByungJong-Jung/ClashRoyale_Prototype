using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IPool
{
    public Transform ParentTransform { get; set; } 
    public Queue<GameObject> Pool { get; set; }
    public GameObject Dequeue(Action<GameObject> inComplete = null,int inEntityID = -1);
    public void Enqueue(GameObject inObejct, Action<GameObject> inComplete = null);
}

public class ObjectPool : IPool
{
    public Transform ParentTransform { get; set; }
    public Queue<GameObject> Pool { get; set; } = new Queue<GameObject>();

    public GameObject Dequeue(Action<GameObject> inComplete = null, int inEntitiyID = -1)
    {
        GameObject obj = Pool.Dequeue();
        obj.name = AddNumberSuffix(obj.name, inEntitiyID);
        obj.transform.localScale *= InGameData.ENTITY_SIZE_OFFSET;
        obj.SetActive(true);

        inComplete?.Invoke(obj);

        return obj;
    }

    public void Enqueue(GameObject inObejct, Action<GameObject> inComplete = null)
    {
        inObejct.transform.SetParent(ParentTransform);
        inObejct.name = RemoveNumberSuffix(inObejct.name);
        inObejct.SetActive(false);
        inObejct.transform.position = InGameData.INFINITY_POS;
        inObejct.transform.localScale /= InGameData.ENTITY_SIZE_OFFSET;
        Pool.Enqueue(inObejct);

        inComplete?.Invoke(inObejct);
    }

    public string AddNumberSuffix(string baseStr, int number)
    {
        return $"{baseStr}=={number}";
    }
    public string RemoveNumberSuffix(string input)
    {
        return Regex.Replace(input, @"==\d+$", "");
    }

}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public IEnumerator Co_Initailize()
    {
        yield return null;
    }


    public Dictionary<string, IPool> PoolDic => _poolDic;
    private Dictionary<string, IPool> _poolDic = new Dictionary<string, IPool>();

    public IPool GetPoolingObjects(string inPath)
    {
        if(_poolDic.ContainsKey(inPath) == false)
        {
            AddPool(inPath);
        }

        if (_poolDic[inPath].Pool.Count <= 0)
            AddQueue(inPath);

        return _poolDic[inPath];
    }

    private GameObject AddPool(string inPath)
    {
        GameObject obj = new GameObject("==POOL==" + inPath);
        ObjectPool T_Component = new ObjectPool();

        _poolDic.Add(inPath, T_Component);
        T_Component.ParentTransform = obj.transform;
        
        obj.transform.SetParent(transform);
        return obj;
    }

    private void AddQueue(string inPath)
    {
        var obj = Instantiate(Resources.Load<GameObject>(inPath));
        obj.transform.SetParent(_poolDic[inPath].ParentTransform);
        obj.transform.localScale *= InGameData.ENTITY_SIZE_OFFSET;
        _poolDic[inPath].Enqueue(obj);
    }

    public void ReleasePoolingObjects(float inDuration, GameObject inObject, string inPath,Action inComplete = null)
    {
        StartCoroutine(Co_ReleasePoolingObjects(inDuration, inObject, inPath, inComplete));
    }

    private IEnumerator Co_ReleasePoolingObjects(float inDuration, GameObject inObject, string inPath,Action inComplete = null)
    {
        yield return new WaitForSeconds(inDuration);

        if (inObject == null)
        {
            inComplete?.Invoke();
            yield break;
        }

        PoolDic[inPath].Enqueue(inObject);
        inComplete?.Invoke();
    }
}
