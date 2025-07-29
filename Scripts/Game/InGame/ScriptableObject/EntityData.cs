using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    public EEntitiyType entityType;
    public EUnitType unityType;
    public EAttackType attackType;
    public EntityData projectileEntityData;

    public string entityName;
    public string resourcePath;

    public float hp;
    public float attackRange;
    public float attackDamage;
    public float targetDetectionRange;
    public float moveSpeed;
}
