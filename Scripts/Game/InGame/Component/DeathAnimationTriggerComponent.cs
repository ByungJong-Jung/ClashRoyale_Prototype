using System;
public struct DeathAnimationTriggerComponent : IComponent
{
    public Action<EntityManager, int, EntityEffectorRefComponent> triggerEvent;

}
