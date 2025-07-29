using UnityEngine;

public interface IEntityEffector
{
    public Vector3 GetEffectTransformPos();
    public void PlayHitEffect();
    public void Clear();

    public Vector3 GetTransformPos();
}
