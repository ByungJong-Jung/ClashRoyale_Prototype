using UnityEngine;
using Unity.Netcode;

public struct ProjectileMoveComponent : IComponent, INetworkSerializable
{
    public int attackEntityID;
    public int targetEntityID;
    public float speed;
    public float damage;
    public float moveLength;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref attackEntityID);
        serializer.SerializeValue(ref targetEntityID);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref moveLength);
    }
}
