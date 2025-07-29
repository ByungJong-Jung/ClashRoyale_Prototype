
using UnityEngine;
using System;
public struct EffectDataComponent : IComponent
{
    public string effectNameKey;
    public Vector3 position;
    public Action completeCallback;
}
