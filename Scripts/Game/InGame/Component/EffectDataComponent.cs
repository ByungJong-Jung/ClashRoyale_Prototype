using UnityEngine;
using Unity.Netcode;
using System;

public struct EffectDataComponent : IComponent, INetworkSerializable
{
    public string effectNameKey;
    public Vector3 position;

    [NonSerialized]
    public Action completeCallback;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref effectNameKey);
        serializer.SerializeValue(ref position);
    }
}
