using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;

public class Unit : MonoBehaviour, IEntityEffector
{
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

    public static void ResetAgent(NavMeshAgent inAgent)
    {
        if (inAgent == null || !inAgent.isActiveAndEnabled || !inAgent.isOnNavMesh)
            return;

        inAgent.isStopped = true;
        inAgent.ResetPath();
        inAgent.velocity = Vector3.zero;
        inAgent.enabled = false;
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

    #region 피격시 처리

    private bool _isPlayingEffect = false;
    private Material _originalMaterial;
    public void PlayHitEffect()
    {
        if (_isPlayingEffect)
            return;

        _isPlayingEffect = true;
        _originalMaterial = _renderer.material;
        _renderer.material = InGameData.HitEffectMaterial;
        StartCoroutine(Co_RestoreColorCoroutine(_originalMaterial, 0.125f));
    }

    private IEnumerator Co_RestoreColorCoroutine(Material inOriginalMateiral, float inDelayTime)
    {
        yield return new WaitForSeconds(inDelayTime);
        _renderer.material = inOriginalMateiral;
        _isPlayingEffect = false;
    }

    #endregion





}
