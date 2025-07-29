using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIMain : Singleton<UIMain> , IManager
{
    [SerializeField] private UILoading _uiLoading;
    [SerializeField] private UIElixir _uIElixir;

    public IEnumerator Co_initialize()
    {
        yield return null;
        _uIElixir.Init();
    }
    
    public void GameStart()
    {
        _uIElixir.GameStart();
        StopLoadingAnimation();
        UIUnitCardManager.Instance.GameStart();
    }


    public void StartLoadingAnimation()
    {
        _uiLoading.StartAnimation();
    }

    public void StopLoadingAnimation()
    {
        _uiLoading.StopAnimation();
    }

}
