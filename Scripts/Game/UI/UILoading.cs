using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class UILoading : MonoBehaviour
{
    [SerializeField] private GameObject _objLoadingScreen;
    [SerializeField] private Text uiText;
    private Sequence loadingSeq;

    public void StartAnimation()
    {
        _objLoadingScreen.SetActive(true);

        if (loadingSeq != null && loadingSeq.IsActive())
            loadingSeq.Kill();

        loadingSeq = DOTween.Sequence();

        loadingSeq.AppendCallback(() => SetText("Loading."))
            .AppendInterval(0.3f)
            .AppendCallback(() => SetText("Loading.."))
            .AppendInterval(0.3f)
            .AppendCallback(() => SetText("Loading..."))
            .AppendInterval(0.3f)
            .SetLoops(-1, LoopType.Restart);
    }
    public void StopAnimation()
    {
        if (loadingSeq != null && loadingSeq.IsActive())
            loadingSeq.Kill();

        _objLoadingScreen.SetActive(false);
    }

    private void SetText(string inText)
    {
        if (uiText != null)
            uiText.text = inText;
    }
}
