using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public class UIUnitCardManager : Singleton<UIUnitCardManager>
{
    // TODO 테스트 용도이기 떄문에 나중에 어떻게 초기화 할지 생각해야 함. 
    public IEnumerator Co_Initialzie()
    {
        yield return null;

        for (int i = 0; i < _unitCard.Length; i++)
        {
            _unitCard[i].SetCardData(CardDeckManager.Instance.CardDeck.GetPreviewCard(i), i);
            _unitCard[i].Init();
        }

        _previewUnitCard.SetCardData(CardDeckManager.Instance.CardDeck.GetNextWaitingCard());
    }

    [SerializeField] private UIUnitCard[] _unitCard;
    [SerializeField] private UIPreviewUnitCard _previewUnitCard;


    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Camera _worldCamera;

    private UIUnitCard _selectedCard;
    private Coroutine _checkingCoroutine;

    public void GameStart()
    {
        for (int i = 0; i < _unitCard.Length; i++)
            _unitCard[i].GameStart();
    }
    
    public void UpdateCardDeck()
    {
        for (int i = 0;i<_unitCard.Length;i++)
        {
            if(i == _selectedCard.Index)
            {
                CardDeckManager.Instance.CardDeck.UseCard(i);
                _selectedCard.SetCardData(CardDeckManager.Instance.CardDeck.GetPreviewCard(i), i);
                break;
            }
        }

        _previewUnitCard.SetCardData(CardDeckManager.Instance.CardDeck.GetNextWaitingCard());
    }

    public void SelectCard(UIUnitCard card)
    {
        if (_selectedCard != null)
        {
            if (_selectedCard.Equals(card))
                return;

            _selectedCard.CancleCard();
            _selectedCard = null;
        }

        _selectedCard = card;

        if (_checkingCoroutine != null)
            StopCoroutine(_checkingCoroutine);

        _checkingCoroutine = StartCoroutine(Co_CheckingkUnitSpawn());
    }

    public void DeselectCard()
    {
        if (_checkingCoroutine != null)
            StopCoroutine(_checkingCoroutine);

        _selectedCard = null;
    }

    private IEnumerator Co_CheckingkUnitSpawn()
    {
        yield return null;

        while (_selectedCard != null)
        {
            if (_selectedCard.IsInSelectableState())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (IsMouseOverGround(Input.mousePosition, out Vector3 spawnPos))
                    {
                        _selectedCard.PlayScaleAnimation(Vector3.zero);
                        UnitPlacer.Instance.StartPlacement(spawnPos, _selectedCard.CardData.unitPrefabPath);
                    }
                }
                else if(Input.GetMouseButton(0))
                {
                    if (UnitPlacer.Instance.IsPlacing)
                    {
                        bool canSpawnUnit = TryGetSpawnPosition(Input.mousePosition, _selectedCard.CardData.unitCardDataInfoList, out Vector3 spawnPos);
                        UnitPlacer.Instance.UpdatePreviewUnitColor(canSpawnUnit);
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    bool canSpawn = TryGetSpawnPosition(Input.mousePosition, _selectedCard.CardData.unitCardDataInfoList, out Vector3 spawnPos);
                    bool hasEnoughElixer = ElixirManager.Instance.TryUseElixir(_selectedCard.CardData.requiredElixir);

                    if (canSpawn && hasEnoughElixer)
                    {
                        ElixirManager.Instance.UseElixir(_selectedCard.CardData.requiredElixir);
                        UnitPlacer.Instance.ConfirmPlacement(_selectedCard.CardData.unitCardDataInfoList, spawnPos);
                        UpdateCardDeck();
                        _selectedCard.ResetCard();
                        DeselectCard();
                    }
                    else
                    {
                        if (UnitPlacer.Instance.IsPlacing)
                        {
                            _selectedCard.ResetSelectCard();
                            UnitPlacer.Instance.CancelPlacement();
                        }
                    }
                }

            }

            yield return null;
        }
    }

    private bool IsMouseOverGround(Vector2 screenPos, out Vector3 hitPos)
    {
        Ray ray = _worldCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, InGameData.RAYCAST_VALUE, _groundLayer))
        {
            hitPos = hit.point;
            return true;
        }
        hitPos = default;
        return false;
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

}
