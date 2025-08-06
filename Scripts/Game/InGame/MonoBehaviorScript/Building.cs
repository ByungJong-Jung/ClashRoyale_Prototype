using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;
public class Building : NetworkBehaviour, IEntityEffector, IEntityHealthBar
{
    #region Network 
    public NetworkVariable<int> TeamIndex = new NetworkVariable<int>();

    public void ApplyTeamColor()
    {
        var teamType = NetworkService.Instance.GetRelation(TeamIndex.Value);
        Debug.Log($"name : {gameObject.name} , teamType : {teamType}");
        _teamType = teamType;
        ChangeColor(teamType);
    }


    [ClientRpc]
    public void PlayHitEffectClientRpc()
    {
        if (_isPlayingEffect)
            return;

        _isPlayingEffect = true;
        Material orignalMaterial = _renderers[0].material;
        _renderers[0].material = InGameData.HitEffectMaterial;
        StartCoroutine(Co_RestoreColorCoroutine(orignalMaterial, 0.125f));
    }

    [ClientRpc]
    public void SetActiveHealthBarClientRpc(bool inActivity)
    {
        _objHealthBar.SetActive(inActivity);
    }

    [ClientRpc]
    public void ProcessHealthBarClientRpc(HealthComponent inHealthComponent)
    {
        if (inHealthComponent.maxHp > 0f)
            _imgHp.fillAmount = inHealthComponent.hp / inHealthComponent.maxHp;
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

    public BuildingUnit BuildingUnit => _buildingUnit;
    [SerializeField] private BuildingUnit _buildingUnit;
    public ETeamType TeamType => _teamType;
    [SerializeField] private ETeamType _teamType;

    public EntityData EntityData => _entityData;
    [SerializeField] private EntityData _entityData;
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private Material _teamBlue;
    [SerializeField] private Material _teamRed;

    public GameObject HealthBar => _objHealthBar;
    [SerializeField] private GameObject _objHealthBar;
    public Image HpImage => _imgHp;
    [SerializeField] private Image _imgHp;
    public void CreateBulildingEntity(EntityFactory inFactory)
    {
        Debug.Log($"CreateBulildingEntity : {_teamType}");
        inFactory.CreateEntityBuildings(_entityData, gameObject, _teamType);
    }

    public void ChangeColor(ETeamType inTeamType)
    {
        Color color = inTeamType == ETeamType.Ally ? InGameData.TEAM_BLUE : InGameData.TEAM_RED;

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].sharedMaterial = inTeamType == ETeamType.Ally ? _teamBlue : _teamRed;

        _buildingUnit?.ChangeColor(color);
    }

    public void Clear()
    {
        _buildingUnit?.Clear();
    }
    public void SetActive(bool inActivity)
    {
        SetActiveClientRpc(inActivity);
    }

    public float CalculateSize()
    {
        var box = gameObject.GetComponent<BoxCollider>();
        if (box != null)
            return box.bounds.extents.magnitude;

        return 0f;
    }


    public float GetRadius()
    {
        GameObject target = this.gameObject;
        var box = target.GetComponent<BoxCollider>();
        if (box == null)
            return 0f;

        float halfDepth = box.size.z * 0.5f * target.transform.lossyScale.z;
        return halfDepth;
    }

    public Vector3 GetEffectTransformPos()
    {
        return _buildingUnit?.GetEffectTransformPos() ?? transform.position;
    }

    public Vector3 GetTransformPos()
    {
        return transform.position;
    }

    public void PlayEffects(EffectDataComponent inEffectData)
    {
        PlayEffectClientRpc(inEffectData);
    }


    private bool _isPlayingEffect = false;
    public void PlayHitEffect()
    {
        PlayHitEffectClientRpc();
    }

    private IEnumerator Co_RestoreColorCoroutine(Material inOriginalMateiral, float inDelayTime)
    {
        yield return new WaitForSeconds(inDelayTime);
        _renderers[0].material = inOriginalMateiral;
        _isPlayingEffect = false;
    }

    #region 체력바

    public void SetActiveHealthBar(bool inActivity)
    {
        SetActiveHealthBarClientRpc(inActivity);
    }
    public void ProcessHealthBar(HealthComponent inHealthComponent)
    {
        ProcessHealthBarClientRpc(inHealthComponent);
    }
    public void ProcessHearBarRotation()
    {

    }
    #endregion
}
