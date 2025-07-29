using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class CardDeckManager : Singleton<CardDeckManager>
{
    private const string UnitCardDataFolder = "ScriptableObject";
    private List<EntityData> _allEntityDataList = new List<EntityData>();
    private List<UnitCardData> _allUnitCardDataList = new List<UnitCardData>();
    private DeckQueue _cardDeck;
    public DeckQueue CardDeck => _cardDeck;

    public IEnumerator Co_Initialize()
    {
        yield return null;
        _allEntityDataList = Resources.LoadAll<EntityData>(UnitCardDataFolder).ToList();
        _allUnitCardDataList = Resources.LoadAll<UnitCardData>(UnitCardDataFolder).ToList();

        List<Entity_Data> entity_Datas = DBManager.Instance.Entity_Data_List;

        for(int i = 0;i< _allEntityDataList.Count;i++)
        {
            Entity_Data entity_Data = DBManager.Instance.GetEntity_Data(_allEntityDataList[i].entityName);

            if(entity_Data != null)
            {
                _allEntityDataList[i].hp = entity_Data.hp;
                _allEntityDataList[i].attackRange = entity_Data.attack_range;
                _allEntityDataList[i].attackDamage = entity_Data.attack_damage;
                _allEntityDataList[i].targetDetectionRange = entity_Data.target_detection_range;
                _allEntityDataList[i].moveSpeed = entity_Data.speed;

                if(_allEntityDataList[i].attackType.Equals(EAttackType.Ranged) && _allEntityDataList[i].projectileEntityData != null)
                {
                    _allEntityDataList[i].projectileEntityData.attackDamage = entity_Data.attack_damage;
                }
            }
        }

        for (int i = 0; i < _allUnitCardDataList.Count; i++)
        {
            Unit_Card_Data unit_Card_Data = DBManager.Instance.GetUnit_Card_Data(_allUnitCardDataList[i].id.Replace("UnitCardData_", ""));

            if (unit_Card_Data != null)
            {
                _allUnitCardDataList[i].requiredElixir = unit_Card_Data.elixir;
            }
        }

        // 테스트 용도.
        {
            UnitCardData[] test = _allUnitCardDataList.ToArray();
            test.ShuffleArray();

            _cardDeck = new DeckQueue();
            List<UnitCardData> cardList = _cardDeck.Init(test);

            foreach (var unitCard in cardList)
            {
                var infoList = unitCard.unitCardDataInfoList;

                foreach (var info in infoList)
                {
                    ObjectPoolManager.Instance.GetPoolingObjects(info.entityData.resourcePath);
                    yield return null;
                }
            }
        }
    }

}

public class DeckQueue
{
    private List<UnitCardData> _allCards;   
    private List<UnitCardData> _handCards;  
    private Queue<UnitCardData> _waitQueue; 

    public IReadOnlyList<UnitCardData> HandCards => _handCards;

    public List<UnitCardData> Init(UnitCardData[] inCards)
    {
        _allCards = new List<UnitCardData>(inCards);
        _handCards = new List<UnitCardData>();
        _waitQueue = new Queue<UnitCardData>();

        for (int i = 0; i < 4; i++)
            _handCards.Add(_allCards[i]);
        for (int i = 4; i < 8; i++)
            _waitQueue.Enqueue(_allCards[i]);

        return _allCards;
    }

    public void UseCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= _handCards.Count)
            return;

        var usedCard = _handCards[handIndex];

        if (_waitQueue.Count > 0)
            _handCards[handIndex] = _waitQueue.Dequeue();
        else
            _handCards.RemoveAt(handIndex); 

        _waitQueue.Enqueue(usedCard);
    }


    public UnitCardData GetPreviewCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= _handCards.Count)
            return null;

        return _handCards[handIndex];
    }


    public UnitCardData GetNextWaitingCard()
    {
        if (_waitQueue.Count > 0)
            return _waitQueue.Peek();
        return null;
    }
}

