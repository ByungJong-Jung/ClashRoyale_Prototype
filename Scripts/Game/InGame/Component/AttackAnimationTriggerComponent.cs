using System;
public struct AttackAnimationTriggerComponent : IComponent 
{
    public Action<EntityManager, int, EntityEffectorRefComponent> triggerEvent;
}
