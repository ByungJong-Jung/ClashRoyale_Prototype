using UnityEngine;
using UnityEngine.UI;

public class UIUnitCardBase : MonoBehaviour
{
    [SerializeField] protected UnitCardData _cardData;
    [SerializeField] protected Image _imgIcon;

    [SerializeField] protected UIUnitCardElixir _unitCardElixir;

    public int Index { get; private set; }

    public void SetCardData(UnitCardData inCardData,int inIndex)
    {
        Index = inIndex;
        _cardData = inCardData;
        _imgIcon.sprite = inCardData.icon;
        _unitCardElixir.SetElixirCount(inCardData.requiredElixir);
    }
}
