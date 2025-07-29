using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using UnityEngine.AI;
using System.Collections.Generic;

public class UIUnitCard : UIUnitCardBase, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] public UnitCardData CardData => _cardData;
    private enum ECardState
    {
        Idle,
        Selected,
        Dragging
    }

    private ECardState _state = ECardState.Idle;

    [SerializeField] private RectTransform _rtUiBottom;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Camera _worldCamera;

    private RectTransform _rtUnitCard;
    private Vector3 _originalScale;
    private Vector2 _originalAnchoredPos;

    private bool _hasStartedPlacing = false;
    public void Init()
    {
        _rtUnitCard = GetComponent<RectTransform>();
        _originalAnchoredPos = _rtUnitCard.anchoredPosition;
        _originalScale = _rtUnitCard.localScale;

        if (_worldCamera == null)
            _worldCamera = Camera.main;
    }

    public void GameStart()
    {
        ResetCard();
        _unitCardElixir.GameStart();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        PlayMoveUpAnimation();
        _state = ECardState.Selected;
        UIUnitCardManager.Instance.SelectCard(this);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        switch (_state)
        {
            case ECardState.Selected:
                {
                    _state = ECardState.Dragging;
                }

                break;

            case ECardState.Dragging:
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            _rtUnitCard.parent as RectTransform,
                            eventData.position,
                            eventData.pressEventCamera,
                            out Vector2 localPos))
                    {
                        _rtUnitCard.anchoredPosition = localPos;
                    }

                    bool canSpawn = TryGetSpawnPosition(eventData.position, CardData.unitCardDataInfoList, out Vector3 spawnPos);                    
                    UnitPlacer.Instance.UpdatePreviewUnitColor(canSpawn);

                    if (canSpawn && !_hasStartedPlacing)
                    {
                        _hasStartedPlacing = true;

                        if(!UnitPlacer.Instance.IsPlacing)
                            UnitPlacer.Instance.StartPlacement(spawnPos, CardData.unitPrefabPath);
                    }
                    else if (!canSpawn && _hasStartedPlacing)
                    {
                        _hasStartedPlacing = false;
                    }

                    bool isMousePositionInBottomUi = IsPointerOutsideRect(eventData, _rtUiBottom);

                    if (isMousePositionInBottomUi)
                    {
                        PlayScaleAnimation(Vector3.zero);
                    }
                    else if (!isMousePositionInBottomUi)
                    {
                        PlayScaleAnimation(_originalScale);
                    }
                }

                break;
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
      
        switch(_state)
        {
            case ECardState.Selected:
                {

                }

                break;

            case ECardState.Dragging:
                {
                    if (_hasStartedPlacing)
                    {
                        bool canSpawn = TryGetSpawnPosition(eventData.position, CardData.unitCardDataInfoList, out Vector3 spawnPos);
                        bool hasEnoughElixer = ElixirManager.Instance.TryUseElixir(CardData.requiredElixir);
                        if (canSpawn && hasEnoughElixer)
                        {
                            ElixirManager.Instance.UseElixir(CardData.requiredElixir);
                            UnitPlacer.Instance.ConfirmPlacement(CardData.unitCardDataInfoList, spawnPos);
                            UIUnitCardManager.Instance.UpdateCardDeck();
                            ResetCard();
                        }
                        else
                        {
                            UnitPlacer.Instance.CancelPlacement();
                            CancleCard(
                                () =>
                                {
                                    PlayMoveUpAnimation();
                                    _state = ECardState.Selected;
                                });
                        }
                    }
                    else
                    {
                        UnitPlacer.Instance.CancelPlacement();
                        CancleCard(
                            ()=>
                            {
                                PlayMoveUpAnimation();
                                _state = ECardState.Selected;
                            });
                    }
                }

                break;
        }
    }


    public bool IsInSelectableState()
    {
        return _state.Equals(ECardState.Selected);
    }

    public void ResetCard()
    {
        // 다시 보이기 전에 왼쪽 바깥으로 이동
        _rtUnitCard.anchoredPosition = new Vector2(-Screen.width, _originalAnchoredPos.y);
        _rtUnitCard.localScale = _originalScale * 1.1f;
        gameObject.SetActive(true);

        // 날아오는 애니메이션 실행
        PlayEnterAnimation();
    }

    public void ResetSelectCard()
    {
        PlayScaleAnimation(_originalScale * 1.1f);
    }

    public void CancleCard(Action inComplete = null)
    {
        _state = ECardState.Idle;
        _hasStartedPlacing = false;

        PlayReturnAnimation(inComplete);
    }
    private bool IsPointerOutsideRect(PointerEventData eventData, RectTransform inTargetRect)
    {
        if (inTargetRect == null)
            return true;

        var canvas = inTargetRect.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        return !RectTransformUtility.RectangleContainsScreenPoint(inTargetRect, eventData.position, uiCamera);
    }

    public bool TryGetSpawnPosition(Vector2 screenPos, List<UnitCardDataInfo> inUnitSpawnDataInfo, out Vector3 resultPos)
    {
        resultPos = Vector3.zero;

        if (inUnitSpawnDataInfo == null || inUnitSpawnDataInfo.Count == 0)
            return false;

        Ray ray = _worldCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, InGameData.RAYCAST_VALUE, _groundLayer))
        {
            resultPos = hit.point;

            foreach (var spawnData in inUnitSpawnDataInfo)
            {
                Vector3 checkPos = resultPos + spawnData.spawnOffset;
                if (!MapData.Instance.CanSpawnAtWorldPosition(checkPos))
                {
                    resultPos = Vector3.zero;
                    return false;
                }
            }

            return true;
        }

        return false;
    }


    public void PlayScaleAnimation(Vector3 inTargetScale)
    {
        float shrinkDuration = 0.15f;
        _rtUnitCard.DOScale(inTargetScale, shrinkDuration).SetEase(Ease.OutQuad);
    }


    private void PlayMoveUpAnimation(Action inComplete = null)
    {
        Vector2 moveUpPos = new Vector2(0, 30f);
        float animDuration = 0.15f;

        DOTween.Sequence()
            .Join(_rtUnitCard.DOAnchorPos(_originalAnchoredPos + moveUpPos, animDuration).SetEase(Ease.OutQuad))
            .Join(_rtUnitCard.DOScale(_originalScale * 1.1f, animDuration).SetEase(Ease.OutQuad))
            .OnComplete(() => inComplete?.Invoke());
    }

    private void PlayEnterAnimation(Action inComplete = null)
    {
        float animDuration = 0.35f;

        DOTween.Sequence()
            .Join(_rtUnitCard.DOAnchorPos(_originalAnchoredPos, animDuration).SetEase(Ease.OutCubic))
            .Join(_rtUnitCard.DOScale(_originalScale, animDuration).SetEase(Ease.OutBack))
            .OnComplete(() => inComplete?.Invoke());
    }

    private void PlayReturnAnimation(Action inComplete = null)
    {
        float returnDuration = 0.3f;
        float shrinkDuration = 0.15f;

        DOTween.Sequence()
            .Join(_rtUnitCard.DOAnchorPos(_originalAnchoredPos, returnDuration).SetEase(Ease.OutBack))
            .Join(_rtUnitCard.DOScale(_originalScale, shrinkDuration).SetEase(Ease.OutBack))
            .OnComplete(() => inComplete?.Invoke());
    }

 

}
