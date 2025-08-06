using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class SpawnButton : UIUnitCard
{
    public event Action OnPressed;
    public event Action OnReleased;
    public event Action OnEnter;
    public event Action OnExit;

    private bool isPointerDown = false;
    private bool isPointerInside = false;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        isPointerDown = true;
        isPointerInside = true;
        OnPressed?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (isPointerDown && isPointerInside)
        {
            OnReleased?.Invoke();
        }
        isPointerDown = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        OnEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        OnExit?.Invoke();
    }

    public bool IsPressed() => isPointerDown;
    public bool IsPointerInside() => isPointerInside;
}
