using UnityEngine;
using Unity.Netcode;

public struct HealthComponent : IComponent, INetworkSerializable
{
    public float hp;
    public float maxHp;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hp);
        serializer.SerializeValue(ref maxHp);
    }
}
