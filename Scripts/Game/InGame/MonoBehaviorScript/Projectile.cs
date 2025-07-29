using UnityEngine;



public class Projectile : MonoBehaviour , IEntityEffector
{
    public enum EProjectileType
    {
        Obejct,
        Particle
    }


    [SerializeField] private EProjectileType _projectileType;
    [SerializeField] private Transform _effectTransform;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private ParticleSystem[] _particles;
    private const string COLOR_PROPERTY_REFERENCE = "Color_c18aea2e3ad54319abb53f299507b005";

    public void ChangeColor(Color inColor)
    {
        switch(_projectileType)
        {
            case EProjectileType.Obejct:
                {
                    _renderer.material.SetColor(COLOR_PROPERTY_REFERENCE, inColor);

                }
                break;

            case EProjectileType.Particle:
                {
                    for (int i = 0; i < _particles.Length; i++)
                        SetColorOverLifetime(_particles[i], inColor);
                }
                break;
        }

    }

    private void SetColorOverLifetime(ParticleSystem inParticle, Color color)
    {
        var colorOverLifetime = inParticle.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(color.a, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    public Vector3 GetEffectTransformPos()
    {
        return _effectTransform.transform.position;
    }
    public Vector3 GetTransformPos()
    {
        return transform.position;
    }
    public void PlayHitEffect() { }

    public void Clear() { }
}
