using UnityEngine;
using Unity.Netcode;


public class Projectile : NetworkBehaviour, IEntityEffector
{
    #region Network
    public NetworkVariable<int> TeamIndex = new NetworkVariable<int>();
    public NetworkVariable<int> EntityID = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ApplyTeamColor(TeamIndex.Value);

        TeamIndex.OnValueChanged += (oldValue, newValue) =>
        {
            ApplyTeamColor(newValue);
        };
    }

    [ClientRpc]
    public void SetActiveClientRpc(bool inActivity)
    {
        gameObject.SetActive(false);
        gameObject.transform.position = InGameData.INFINITY_POS;
    }

    [ClientRpc]
    public void PlayEffectClientRpc(EffectDataComponent inEffectDataComp)
    {
        string effectPath = ResourceEffectPath.GetEffectResourcePath(inEffectDataComp.effectNameKey);

        float particleDuration = 0f;
        GameObject effectObject = null;
        ObjectPoolManager.Instance.GetPoolingObjects(effectPath).Dequeue(
            (effect) =>
            {
                effectObject = effect;
                effectObject.transform.position = inEffectDataComp.position;
                particleDuration = effectObject.GetComponent<Effect>()?.GetEffectDuration ?? 0f;
            });

        ObjectPoolManager.Instance.ReleasePoolingObjects(particleDuration, effectObject, effectPath,
            inComplete: () =>
            {
                inEffectDataComp.completeCallback?.Invoke();
            });
    }
    #endregion


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

    private void ApplyTeamColor(int inTeamindex)
    {
        var teamType = NetworkService.Instance.GetRelation(inTeamindex);
        Color color = teamType == ETeamType.Ally ? InGameData.TEAM_BLUE : InGameData.TEAM_RED;
        ChangeColor(color);
    }

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
    public void PlayEffects(EffectDataComponent inEffectData) 
    {
        PlayEffectClientRpc(inEffectData);
    }
    public void PlayHitEffect() { }

    public void Clear() { }

    public void SetActive(bool inActivity)
    {
        SetActiveClientRpc(inActivity);
    }

}
