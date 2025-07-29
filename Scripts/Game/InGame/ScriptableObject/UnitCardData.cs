using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct UnitCardDataInfo
{
    public EntityData entityData;
    public Vector3 spawnOffset;
}


[CreateAssetMenu(fileName = "UnitCardData", menuName = "Scriptable Objects/UnitCardData")]
public class UnitCardData : ScriptableObject
{
    public string id;
    public string resourcePath;
    public string unitPrefabPath;
    public List<UnitCardDataInfo> unitCardDataInfoList;
    public Sprite icon;

    public int requiredElixir;
}

