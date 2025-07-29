using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIUnitCardElixir : MonoBehaviour
{
    [SerializeField] private Text _txtElixirCount;
    [SerializeField] private Image _imgElixirFill;

    private float _requiredElixer;
    public void SetElixirCount(int inElixirCount)
    {
        _requiredElixer = inElixirCount;
        _txtElixirCount.text = $"{inElixirCount}";
    }

    public void GameStart()
    {
        StartCoroutine(Co_PlayElixirGaugeAnimation());
    }
    private IEnumerator Co_PlayElixirGaugeAnimation()
    {
        while (true)
        {
            yield return null;

            float current = ElixirManager.Instance.CurrentElixir;
            float need = _requiredElixer;

            if (current >= need)
                continue;

            // 1. 엘릭서가 충분하면 fillAmount=0 (사용 가능)
            // 2. 부족할수록 1에 가까워짐 (사용 불가)
            float t = Mathf.Clamp01((need - current) / need);
            _imgElixirFill.fillAmount = t;
        }
    }
}
