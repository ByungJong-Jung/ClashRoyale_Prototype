using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class Building : MonoBehaviour, IEntityEffector
{
    public BuildingUnit BuildingUnit => _buildingUnit;
    [SerializeField] private BuildingUnit _buildingUnit;
    [SerializeField] private ETeamType _teamType;

    public EntityData EntityData => _entityData;
    [SerializeField] private EntityData _entityData;
    [SerializeField] private Renderer _renderer;
    public GameObject HealthBar => _objHealthBar;
    [SerializeField] private GameObject _objHealthBar;
    public Image HpImage => _imgHp;
    [SerializeField] private Image _imgHp;
    public void CreateBulildingEntity(EntityFactory inFactory)
    {
        inFactory.CreateEntityBuildings(_entityData, gameObject, _teamType);
    }

    public void ChangeColor(Color inColor)
    {
        _buildingUnit?.ChangeColor(inColor);
    }

    public void Clear()
    {
        _buildingUnit?.Clear();
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

    private bool _isPlayingEffect = false;
    public void PlayHitEffect()
    {
        if (_isPlayingEffect)
            return;

        _isPlayingEffect = true;
        Material orignalMaterial = _renderer.material;
        _renderer.material = InGameData.HitEffectMaterial;
        StartCoroutine(Co_RestoreColorCoroutine(orignalMaterial, 0.125f));
    }

    private IEnumerator Co_RestoreColorCoroutine(Material inOriginalMateiral, float inDelayTime)
    {
        yield return new WaitForSeconds(inDelayTime);
        _renderer.material = inOriginalMateiral;
        _isPlayingEffect = false;
    }

}
