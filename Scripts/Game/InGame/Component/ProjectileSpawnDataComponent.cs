using UnityEngine;

public struct ProjectileSpawnDataComponent : IComponent
{
    public int attackEntityID;
    public int targetEntityID;
    public EntityData projectileEntityData;
    public Vector3 position;
    public float projectileMoveLength;
}