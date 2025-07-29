using UnityEngine;

public struct ProjectileMoveComponent : IComponent
{
    public int attackEntityID;
    public int targetEntityID;
    public float speed;
    public float damage;
    public float moveLength;
}