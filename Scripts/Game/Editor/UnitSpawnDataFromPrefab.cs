using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class UnitCardDataFromPrefab
{
    [MenuItem("GameObject/Canvas And Need Scripts", false, 11)]
    public static void SetupCanvasUnderSelectedObject()
    {
        // 1. 선택 오브젝트 검사
        var selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("하이라키에서 오브젝트를 선택하세요!");
            return;
        }

        // 2. 프리팹 로드
        var canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Canvas.prefab");
        if (canvasPrefab == null)
        {
            Debug.LogError($"Canvas 프리팹을 찾을 수 없음: {"Assets/Resources/Canvas.prefab"}");
            return;
        }

        // 3. 프리팹 인스턴스 생성 및 부모로 설정
        GameObject canvasInstance = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab, selected.transform);
        canvasInstance.transform.localPosition = Vector3.zero;
        canvasInstance.transform.localRotation = Quaternion.identity;

        // 4. 프리팹 언팩(프리팹 연결 해제)
        PrefabUtility.UnpackPrefabInstance(canvasInstance, PrefabUnpackMode.Completely, InteractionMode.UserAction);

        // 5. 선택 오브젝트에 컴포넌트 추가
        Undo.AddComponent<EntityAnimator>(selected);
        Undo.AddComponent<Unit>(selected);

        Debug.Log($"[{selected.name}]에 Canvas 추가, EntityEffector/Unit 컴포넌트 부착 완료");
    }


    [MenuItem("Assets/UnitCardData from Prefab Single", priority = 0)]
    public static void GenerateFroPrefabSigngleUnit()
    {
        var prefab = Selection.activeObject as GameObject;
        if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            Debug.LogError("Project 창에서 프리팹을 선택해야 합니다.");
            return;
        }

        // Prefab 인스턴스 생성 (비활성 상태로)
        GameObject instance = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
        if (instance == null)
        {
            Debug.LogError("프리팹 인스턴스를 불러올 수 없습니다.");
            return;
        }

        // 저장 폴더. 
        string folderPath = "Assets/Resources/ScriptableObject";
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "Generated");

        // UnitData 생성 또는 로드
        string entityDataName = $"EntityData_{prefab.name}";
        string entityDataPath = $"{folderPath}/{entityDataName}.asset";

        EntityData entityData = AssetDatabase.LoadAssetAtPath<EntityData>(entityDataPath);
        if (entityData == null)
        {
            entityData = ScriptableObject.CreateInstance<EntityData>();
            entityData.entityName = prefab.name;
            entityData.resourcePath = GetResourcesPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            entityData.unityType = EUnitType.Normal;
            AssetDatabase.CreateAsset(entityData, entityDataPath);
        }
        else
        {
            entityData.name = prefab.name;
            entityData.resourcePath = GetResourcesPath(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        string assetPath = $"{folderPath}/UnitCardData_{prefab.name}.asset";

        // ScriptableObject 생성
        UnitCardData cardData = ScriptableObject.CreateInstance<UnitCardData>();
        cardData.id = $"UnitCardData_{prefab.name}";
        cardData.resourcePath = GetResourcesPath(assetPath);
        cardData.unitPrefabPath = GetResourcesPath(AssetDatabase.GetAssetPath(Selection.activeObject));
        cardData.unitCardDataInfoList = new List<UnitCardDataInfo>();
        cardData.unitCardDataInfoList.Add(new UnitCardDataInfo() { entityData = entityData, spawnOffset = Vector3.zero });



        if (File.Exists(assetPath))
        {
            AssetDatabase.DeleteAsset(assetPath);
        }

        AssetDatabase.CreateAsset(cardData, assetPath);
        AssetDatabase.SaveAssets();

        // 프리팹 언로드
        PrefabUtility.UnloadPrefabContents(instance);

        Debug.Log($"UnitCardData 생성 완료: {assetPath}");
        Selection.activeObject = cardData;
    }


    [MenuItem("Assets/UnitCardData from Prefab Group", priority = 1)]
    public static void GenerateFromPrefabGroupUnit()
    {
        var prefab = Selection.activeObject as GameObject;
        if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            Debug.LogError("Project 창에서 프리팹을 선택해야 합니다.");
            return;
        }

        // Prefab 인스턴스 생성 (비활성 상태로)
        GameObject instance = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
        if (instance == null)
        {
            Debug.LogError("프리팹 인스턴스를 불러올 수 없습니다.");
            return;
        }

        // 저장 폴더
        string folderPath = "Assets/Resources/ScriptableObject";
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "Generated");

        string assetPath = $"{folderPath}/UnitCardData_{prefab.name}.asset";
        // ScriptableObject 생성
        UnitCardData cardData = ScriptableObject.CreateInstance<UnitCardData>();
        cardData.id = $"UnitCardData_{prefab.name}";
        cardData.resourcePath = GetResourcesPath(assetPath);
        cardData.unitPrefabPath = GetResourcesPath(AssetDatabase.GetAssetPath(Selection.activeObject));
        cardData.unitCardDataInfoList = new List<UnitCardDataInfo>();

        foreach (Transform child in instance.transform)
        {
            // UnitData 생성 또는 로드
            string entityDataName = $"EntityData_{Regex.Replace(child.gameObject.name, @"\s*\(\d+\)", "") }";
            string entityDataPath = $"{folderPath}/{entityDataName}.asset";

            EntityData entityData = AssetDatabase.LoadAssetAtPath<EntityData>(entityDataPath);
            if (entityData == null)
            {
                Debug.LogError("그룹은 유닛 데이터가 기존에 있어야 합니다.");
                return;
            }
          
            var info = new UnitCardDataInfo
            {
                entityData = entityData, // 수동 연결
                spawnOffset = child.localPosition
            };
            cardData.unitCardDataInfoList.Add(info);
        }


        if (File.Exists(assetPath))
        {
            AssetDatabase.DeleteAsset(assetPath);
        }

        AssetDatabase.CreateAsset(cardData, assetPath);
        AssetDatabase.SaveAssets();

        // 프리팹 언로드
        PrefabUtility.UnloadPrefabContents(instance);

        Debug.Log($"UnitCardData 생성 완료: {assetPath}");
        Selection.activeObject = cardData;
    }

    [MenuItem("Assets/Prjectile Entity Data from Prefab Single", priority = 0)]
    public static void GenerateFroPrefabSigngleProjectile()
    {
        var prefab = Selection.activeObject as GameObject;
        if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            Debug.LogError("Project 창에서 프리팹을 선택해야 합니다.");
            return;
        }

        // Prefab 인스턴스 생성 (비활성 상태로)
        GameObject instance = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
        if (instance == null)
        {
            Debug.LogError("프리팹 인스턴스를 불러올 수 없습니다.");
            return;
        }

        // 저장 폴더. 
        string folderPath = "Assets/Resources/ScriptableObject";
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "Generated");

        // UnitData 생성 또는 로드
        string entityDataName = $"EntityData_{prefab.name}_projectile";
        string entityDataPath = $"{folderPath}/{entityDataName}.asset";

        EntityData entityData = AssetDatabase.LoadAssetAtPath<EntityData>(entityDataPath);
        if (entityData == null)
        {
            entityData = ScriptableObject.CreateInstance<EntityData>();
            entityData.entityType = EEntitiyType.Projectile;
            entityData.entityName = prefab.name;
            entityData.resourcePath = GetResourcesPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            AssetDatabase.CreateAsset(entityData, entityDataPath);
        }
        else
        {
            entityData.name = prefab.name;
            entityData.resourcePath = GetResourcesPath(AssetDatabase.GetAssetPath(Selection.activeObject));
        }


        AssetDatabase.SaveAssets();

        // 프리팹 언로드
        PrefabUtility.UnloadPrefabContents(instance);

        Debug.Log($"EntityData projectile 생성 완료: {entityDataPath}");
        Selection.activeObject = entityData;
    }


    private static string GetResourcesPath(string assetPath)
    {
        const string resourcesPrefix = "Assets/Resources/";

        if (!assetPath.StartsWith(resourcesPrefix))
            return null;

        string relativePath = assetPath.Substring(resourcesPrefix.Length);
        return Path.ChangeExtension(relativePath, null); // 확장자 제거
    }
}
