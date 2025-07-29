using UnityEngine;
using UnityEngine.UI;
public class UIPreviewUnitCard : MonoBehaviour
{
    [SerializeField] private Image _imgIcon;

    public void SetCardData(UnitCardData inCardData)
    {
        _imgIcon.sprite = inCardData.icon;
    }
}
