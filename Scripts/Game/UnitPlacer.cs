using UnityEngine;
using System.Collections;
using VContainer;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
public class UnitPlacer : Singleton<UnitPlacer>, IManager
{
    public MapData MapData => _mapData;
    [SerializeField] private MapData _mapData;
    [SerializeField] LayerMask _groundMask;
    [SerializeField] RectTransform _cancelOnHoverUI;

    private GameObject _previewUnit;
    private string _unitPath;

    public bool IsPlacing => _isPlacing;
    private bool _isPlacing = false;

    private IObjectResolver _resolver;

    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        _resolver = resolver;
    }


    public IEnumerator Co_Initialize() 
    {
        yield return null;
        _priviewMaterial = new Material(Shader.Find("Unlit/UnlitAlphaWithFade"));
        //_mapData.Initialize();
    }

    public void GameStart()
    {
        var factory = _resolver.Resolve<EntityFactory>();
        _mapData.CreateBuildingEntity(factory);
        _mapData.SetUpMapDecorations();

        //SetupCameraView();

        StartCoroutine(Co_Upate());
    }

    private void SetupCameraView()
    {
        // 내 팀 가져오기
        ulong myClientId = NetworkManager.Singleton.LocalClientId;
        ETeamType myTeam = NetworkGameManager.Instance.GetPlayerTeam(myClientId);

        if (myTeam == ETeamType.Enemy)
        {
            // 적 진영이면 시점 반전
            var cam = Camera.main;
            cam.transform.rotation = Quaternion.Euler(75f, 180f, 0f);
            cam.transform.position = new Vector3(0f, 105f, 32.5f); // Z축 반대로
        }
        else
        {
            // 아군이면 원래 시점
            var cam = Camera.main;
            cam.transform.rotation = Quaternion.Euler(75f, 0f, 0f);
            cam.transform.position = new Vector3(0f, 105f, -32.5f);
        }
    }

    private IEnumerator Co_Upate()
    {
        while(true)
        {
            yield return null;

            if (_isPlacing && _previewUnit != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, InGameData.RAYCAST_VALUE, _groundMask))
                {
                    _previewUnit.transform.position = hit.point.SetY(0f);
                }
            }
        }
        
    }
    public void StartPlacement(Vector3 inPos, string inUnitPath)
    {
        if (!_isPlacing && _unitPath != null && _previewUnit != null)
        {
            ObjectPoolManager.Instance.PoolDic[$"Single/{_unitPath}"].Enqueue(_previewUnit);
            _previewUnit = null;
            _unitPath = null;
        }

        _unitPath = $"Single/{inUnitPath}";
        _previewUnit = ObjectPoolManager.Instance.GetPoolingObjects(_unitPath).Dequeue();
        _previewUnit.transform.position = inPos;

        Quaternion rotation;
        if (NetworkManager.Singleton.IsHost)
            rotation = Quaternion.Euler(Vector3.zero);
        else
            rotation = Quaternion.Euler(Vector3.up * 180f);

        _previewUnit.transform.rotation = rotation;
        MakeTransparent(_previewUnit);
        
        _isPlacing = true;
    }
    public void CancelPlacement()
    {
        _isPlacing = false;

        if (_previewUnit != null)
        {
            RestoreMaterials(_previewUnit);

            ObjectPoolManager.Instance.PoolDic[_unitPath].Enqueue(_previewUnit);
            _previewUnit = null;
            _unitPath = null;
        }
    }

    public void CancelPlacementv2(float inDelayTime)
    {
        _isPlacing = false;

        var unit = _previewUnit;
        var path = _unitPath;

        ObjectPoolManager.Instance.ReleasePoolingObjects(inDelayTime, unit, path,
            () =>
            {
                RestoreMaterials(unit);
            });

        _previewUnit = null;
        _unitPath = null;
    }

    public void ConfirmPlacement(List<UnitCardDataInfo> inSpawnInfoList, Vector3 inSpawnPos, System.Action inComplete = null)
    {
        _isPlacing = false;
        StartCoroutine(Co_SpawnUnitDelay(inSpawnInfoList, inSpawnPos, inDelayTime: 1f, inComplete));
    }

    private IEnumerator Co_SpawnUnitDelay(List<UnitCardDataInfo> inSpawnInfoList, Vector3 inSpawnPos,float inDelayTime, System.Action inComplete = null)
    {
        CancelPlacementv2(inDelayTime);
        yield return new WaitForSeconds(inDelayTime);

        for (int i = 0; i < inSpawnInfoList.Count; i++)
        {
            yield return Co_SpawnUnit(inSpawnInfoList[i].entityData, inSpawnInfoList[i].spawnOffset + inSpawnPos, inSpawnInfoList.Count > 1);
        }

        inComplete?.Invoke();
    }

    private IEnumerator Co_SpawnUnit(EntityData inEntityData, Vector3 inSpawnPos, bool inIsGroup)
    {
        yield return null;
        RequestSpawnUnit(inEntityData, inSpawnPos);
    }

    public void RequestSpawnUnit(EntityData inEntityData, Vector3 inSpawnPos)
    {
        NetworkService.Instance.RequestSpawnUnit(inEntityData.entityName, inEntityData.resourcePath, inSpawnPos, NetworkService.Instance.MyTeamIndex);
    }

    #region [ Change Material ]

    private static Material _priviewMaterial;
    private static Material PriviewMaterial
    {
        get
        {
            if (_priviewMaterial == null)
            {
                _priviewMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            }

            return _priviewMaterial;
        }
    }

    private bool _nowCanSpawn = false;
    private readonly Dictionary<GameObject, Material[]> _originalMaterials = new Dictionary<GameObject, Material[]>();

    public void MakeTransparent(GameObject unit)
    {
        if (_originalMaterials.ContainsKey(unit))
            return;

        var renderers = unit.GetComponentsInChildren<Renderer>();
        var originalMats = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];

            if (renderer.sharedMaterial == null) continue;

            originalMats[i] = renderer.sharedMaterial;

            var newMat = PriviewMaterial;

            var color = Color.white;
            color.a = 0.5f;
            newMat.color = color;

            renderer.material = newMat;
        }

        _originalMaterials[unit] = originalMats;
        _nowCanSpawn = true;
    }

    public void RestoreMaterials(GameObject unit)
    {
        if (unit == null)
            return;

        if (!_originalMaterials.TryGetValue(unit, out var originalMats))
            return;

        var renderers = unit.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length && i < originalMats.Length; i++)
        {
            if (originalMats[i] != null)
                renderers[i].material = originalMats[i];
        }

        _originalMaterials.Remove(unit);
    }

    public void UpdatePreviewUnitColor(bool inCanSpawn)
    {
        if (_nowCanSpawn != inCanSpawn)
        {
            if (_previewUnit != null && _originalMaterials.ContainsKey(_previewUnit))
            {
                if (inCanSpawn)
                {
                    var renderers = _previewUnit.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        var color = Color.white;
                        color.a = 0.5f;
                        renderers[i].material.color = color;
                    }
                    _nowCanSpawn = true;
                }
                else
                {
                    var renderers = _previewUnit.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        var color = Color.red;
                        color.a = 0.5f;
                        renderers[i].material.color = color;
                    }
                    _nowCanSpawn = false;
                }
            }
        }
    }

    #endregion
}