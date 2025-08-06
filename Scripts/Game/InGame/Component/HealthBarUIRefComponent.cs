
using UnityEngine;
using UnityEngine.UI;
public struct HealthBarUIRefComponent : IComponent
{
    public IEntityHealthBar entityHealthBar;
    public Transform healBarTransform;
    public Image fillImage;
}
