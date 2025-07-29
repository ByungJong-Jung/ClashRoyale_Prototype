using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class UIElixir : MonoBehaviour
{
    [SerializeField] private Text _txtElixirCount;
    [SerializeField] private Image _imgElixirGauge;

    public void Init()
    {
        ElixirManager.Instance.OnElixirChanged += UpdateTextWithAnimation;
        _txtElixirCount.text = $"{(int)ElixirManager.START_ELIXIR_COUNT}";
    }

    public void GameStart()
    {
        StartCoroutine(Co_PlayElixirGaugeAnimation());
    }


    private IEnumerator Co_PlayElixirGaugeAnimation()
    {
        while(true)
        {
            yield return null;

            _imgElixirGauge.fillAmount = ElixirManager.Instance.CurrentFillAmount;
        }
    }

    private void UpdateTextWithAnimation(int intValue)
    {
        if (_txtElixirCount == null) return;

        _txtElixirCount.text = intValue.ToString();

        // Dotween ¸ð¼Ç(¿¹: Æ¨±è È¿°ú)
        DOTween.Sequence()
            .Append(_txtElixirCount.rectTransform.DOScale(1.25f, 0.1f))
            .Append(_txtElixirCount.rectTransform.DOScale(1.0f, 0.15f));
    }
}
