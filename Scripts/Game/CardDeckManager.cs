using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class CardDeckManager : Singleton<CardDeckManager>
{
    private const string UnitCardDataFolder = "ScriptableObject";
    public List<EntityData> AllEntityDataList => _allEntityDataList;
    private List<EntityData> _allEntityDataList = new List<EntityData>();
    private List<UnitCardData> _allUnitCardDataList = new List<UnitCardData>();
    private DeckQueue _cardDeck;
    public DeckQueue CardDeck => _cardDeck;

    public EntityData GetEntityData(string inEntityName)
    {
        for(int i = 0;i< AllEntityDataList.Count;i++)
        {
            if (AllEntityDataList[i].entityName.Equals(inEntityName))
                return AllEntityDataList[i];
        }

        return null;
    }

    public IEnumerator Co_Initialize()
    {
        yield return null;

        // 테스트
        //{
        //   var testCard = Resources.Load<UnitCardData>($"{UnitCardDataFolder}/UnitCardData_Mage");
        //   var testCard2 = Resources.Load<UnitCardData>($"{UnitCardDataFolder}/UnitCardData_Gunner");
        //   var testCard3 = Resources.Load<UnitCardData>($"{UnitCardDataFolder}/UnitCardData_Gunner");
        //   var testCard4 = Resources.Load<UnitCardData>($"{UnitCardDataFolder}/UnitCardData_Gunner");

        //    for (int i = 0; i < 4; i++)
        //    {
        //        _allUnitCardDataList.Add(testCard);
        //        _allUnitCardDataList.Add(testCard2);
        //        _allUnitCardDataList.Add(testCard3);
        //        _allUnitCardDataList.Add(testCard4);
        //    }
        //}

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

        // Test용 덱 구성
        {
            UnitCardData[] test = _allUnitCardDataList.ToArray();
            test.ShuffleArray();

            // 8개 할당.
            _cardDeck = new DeckQueue();
            List<UnitCardData> cardList = _cardDeck.Init(test);

            //foreach (var unitCard in cardList)
            //{
            //    var infoList = unitCard.unitCardDataInfoList;
            //
            //    foreach (var info in infoList)
            //    {
            //        ObjectPoolManager.Instance.GetPoolingObjects(info.entityData.resourcePath);
            //        yield return null;
            //    }
            //}
        }
    }

}

public class DeckQueue
{
    private List<UnitCardData> _allCards;   // 8장 전체 카드
    private List<UnitCardData> _handCards;  // 4장 핸드
    private Queue<UnitCardData> _waitQueue; // 4장 대기

    public IReadOnlyList<UnitCardData> HandCards => _handCards;

    public List<UnitCardData> Init(UnitCardData[] inCards)
    {
        _allCards = new List<UnitCardData>(inCards);
        _handCards = new List<UnitCardData>();
        _waitQueue = new Queue<UnitCardData>();

        // 처음에는 0~3번째는 핸드, 4~7번째는 대기 큐로
        for (int i = 0; i < 4; i++)
            _handCards.Add(_allCards[i]);
        for (int i = 4; i < 8; i++)
            _waitQueue.Enqueue(_allCards[i]);

        return _allCards;
    }

    // n번째 핸드 카드를 사용 (0~3)
    public void UseCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= _handCards.Count)
            return;

        var usedCard = _handCards[handIndex];

        // 대기 큐에서 하나 뽑아서 핸드에 보충
        if (_waitQueue.Count > 0)
            _handCards[handIndex] = _waitQueue.Dequeue();
        else
            _handCards.RemoveAt(handIndex); // 카드가 부족한 경우

        // 사용한 카드는 맨 뒤로
        _waitQueue.Enqueue(usedCard);
    }

    /// <summary>
    /// 핸드에서 미리보기 용 카드 반환 (0~3)
    /// </summary>
    public UnitCardData GetPreviewCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= _handCards.Count)
            return null;

        return _handCards[handIndex];
    }

    /// <summary>
    /// 대기큐에서 다음 들어올 카드 미리보기 (가장 앞에 있는 대기카드 반환)
    /// </summary>
    public UnitCardData GetNextWaitingCard()
    {
        if (_waitQueue.Count > 0)
            return _waitQueue.Peek();
        return null;
    }
}

