using UnityEngine;

public class Effect : MonoBehaviour
{
    [SerializeField] ParticleSystem _particle;

    public float GetEffectDuration
    {
        get
        {
            return _particle?.main.duration ?? 0f;
        }
    }
}
