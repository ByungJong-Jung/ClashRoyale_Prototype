using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Unity.Netcode;
using System;

public class Unit : NetworkBehaviour, IEntityEffector, IEntityHealthBar
{
    #region Network 
    public NetworkVariable<int> TeamIndex = new NetworkVariable<int>();

    public Action SpawnAnimationCompleteEvent = null;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ApplyTeamColor(TeamIndex.Value);

        TeamIndex.OnValueChanged += (oldValue, newValue) =>
        {
            ApplyTeamColor(newValue);
        };

        PlaySpawnAnimation(
            ()=>
            {
                SpawnAnimationCompleteEvent?.Invoke();
            });
    }


    [ClientRpc]
    public void PlayHitEffectClientRpc()
    {
        if (_isPlayingEffect)
            return;

        _isPlayingEffect = true;
        _originalMaterial = _renderer.material;
        _renderer.material = InGameData.HitEffectMaterial;
        StartCoroutine(Co_RestoreColorCoroutine(_originalMaterial, 0.125f));
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
    public void ProcessHearBarRotationClientRpc()
    {
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        _objHealthBar.transform.forward = camForward;
    }

    [ClientRpc]
    public void SetActiveClientRpc(bool inActivity)
    {
        gameObject.SetActive(false);
        gameObject.transform.position = InGameData.INFINITY_POS;
    }
    #endregion


    public EntityAnimator EntityAnimator => _entityAnimator;
    [SerializeField] private EntityAnimator _entityAnimator;
    [SerializeField] private Transform _effectTransform;
    public NavMeshAgent Agent => _agent;
    [SerializeField] private NavMeshAgent _agent;

    public GameObject HealthBar => _objHealthBar;
    [SerializeField] private GameObject _objHealthBar;
    public Image HpImage => _imgHp;
    [SerializeField] private Image _imgHp;

    [SerializeField] private SkinnedMeshRenderer _renderer;
    private const string COLOR_PROPERTY_REFERENCE = "Color_c18aea2e3ad54319abb53f299507b005";

    public void SetEnable(bool inEnabled, EUnitType inUnitType)
    {
        if ((inUnitType & EUnitType.Air) != 0)
            _agent.enabled = false;
        else
            _agent.enabled = inEnabled;
    }

    public void Clear()
    {
        if(_originalMaterial != null)
            _renderer.material = _originalMaterial;

        _objHealthBar.SetActive(false);
        ResetAgent(_agent);
    }

    public void SetActive(bool inActivity)
    {
        SetActiveClientRpc(inActivity);
    }
    public void PlaySpawnAnimation(Action inComplete)
    {
        Vector3 inSpawnPos = transform.position;
        float dropHeight = 10f;
        Vector3 dropStartPos = inSpawnPos + Vector3.up * dropHeight;
        float dropTime = 0.5f;

        transform.position = dropStartPos;

        transform.DOMoveY(inSpawnPos.y, dropTime)
            .SetEase(Ease.OutBounce).OnComplete(()=>inComplete?.Invoke());
    }
    public static void ResetAgent(NavMeshAgent inAgent)
    {
        if (inAgent == null || !inAgent.isActiveAndEnabled || !inAgent.isOnNavMesh)
            return;

        inAgent.isStopped = true;
        inAgent.ResetPath();
        inAgent.velocity = Vector3.zero;
        inAgent.enabled = false;
    }
    private void ApplyTeamColor(int inTeamindex)
    {
        var teamType = NetworkService.Instance.GetRelation(inTeamindex);
        Color color = teamType == ETeamType.Ally ? InGameData.TEAM_BLUE : InGameData.TEAM_RED;
        ChangeColor(color);
    }
    public void ChangeColor(Color inColor)
    {
        _renderer.material.SetColor(COLOR_PROPERTY_REFERENCE, inColor); 
    }


    public float GetAgentSize()
    {
        var agent = _agent;
        if (agent != null)
            return agent.radius;

        return 0f;
    }

    public Vector3 GetEffectTransformPos()
    {
        return _effectTransform.transform.position;
    }

    public Vector3 GetTransformPos()
    {
        return transform.position;
    }

    public void PlayEffects(EffectDataComponent inEffectData) { }


    #region 피격시 처리

    private bool _isPlayingEffect = false;
    private Material _originalMaterial;
    public void PlayHitEffect()
    {
        PlayHitEffectClientRpc();
    }

    private IEnumerator Co_RestoreColorCoroutine(Material inOriginalMateiral, float inDelayTime)
    {
        yield return new WaitForSeconds(inDelayTime);
        _renderer.material = inOriginalMateiral;
        _isPlayingEffect = false;
    }

    #endregion

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
        ProcessHearBarRotationClientRpc();
    }

    #endregion



}
