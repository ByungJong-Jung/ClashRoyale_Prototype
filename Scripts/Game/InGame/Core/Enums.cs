public enum EEntitiyType
{
    Unit,
    Building,
    Projectile
}

[System.Flags]
public enum EUnitType
{
    Normal = 1 << 0, 
    OnlyBuildingAttack = 1 << 1, 
    Air = 1 << 2, 
    Invisible = 1 << 3, 
    Hero = 1 << 4, 
}

public enum EAttackType
{
    Melee,
    Ranged
}

public enum ETeamType
{
    Ally = 0,    // 아군/플레이어
    Enemy = 1,     // 적군
    Neutral = 2,   // 중립
}
