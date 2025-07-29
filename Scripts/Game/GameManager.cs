using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    void Start()
    {
        GameStart();
    }

    public void GameStart()
    {
        StartCoroutine(Co_GameStart());
    }

    private IEnumerator Co_Initialize()
    {
        List<Func<IEnumerator>> ingameInitializes = new List<Func<IEnumerator>>
        {
            DBManager.Instance.Co_Initialize,
            CardDeckManager.Instance.Co_Initialize,
            EnemySpawner.Instance.Co_Initialzie,
            PopupManager.Instance.Co_Initialize,
            ObjectPoolManager.Instance.Co_Initailize,
            UIUnitCardManager.Instance.Co_Initialzie,
            UnitPlacer.Instance.Co_Initialize,
            ElixirManager.Instance.Co_Initialzie,
            UIMain.Instance.Co_initialize,
            InGameManager.Instance.Co_Initialize
        };

        int totalSteps = ingameInitializes.Count + 1;

        for (int i = 0; i < ingameInitializes.Count; i++)
        {
            yield return ingameInitializes[i]();
        }

        Debug.Log("초기화 완료!");
    }

    private IEnumerator Co_GameStart()
    {
        UIMain.Instance.StartLoadingAnimation();

        yield return Co_Initialize();

        
        var managers = new List<IManager>()
        {
            ElixirManager.Instance,
            EnemySpawner.Instance,
            UnitPlacer.Instance,
            InGameManager.Instance,
            UIMain.Instance
        };

        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].GameStart();
        }
    }
}
