using UnityEngine;

public class BuildingUnit : MonoBehaviour
{
    public EntityAnimator EntityAnimator => _entityAnaimator;
    [SerializeField] private EntityAnimator _entityAnaimator;

    private const string COLOR_PROPERTY_REFERENCE = "Color_c18aea2e3ad54319abb53f299507b005";
    [SerializeField] private Renderer _renderer;

    [SerializeField] private Transform _effectTransform;

    private Quaternion _initRotation;

    public void ChangeColor(Color inColor)
    {
        _initRotation = transform.rotation;
        _renderer.material.SetColor(COLOR_PROPERTY_REFERENCE, inColor);
    }

    public Vector3 GetEffectTransformPos()
    {
        return _effectTransform.position;
    }
    public void Clear()
    {
        transform.rotation = _initRotation;
    }
}
