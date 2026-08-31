// =========================
// DeckModule.cs
// 캐릭터의 카드 영역 관리
// =========================

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터의 덱, 손패, 묘지, 제외, 소멸 영역을 관리합니다.
/// 같은 CardData라도 각각 독립된 CardInstance로 저장합니다.
/// </summary>
public class DeckModule : CharacterModule
{
    public sealed override System.Type RegistrationType =>
        typeof(DeckModule);

    [Header("카드 영역")]
    [SerializeField] private List<CardInstance> deck = new();
    [SerializeField] private List<CardInstance> hand = new();
    [SerializeField] private List<CardInstance> graveyard = new();
    [SerializeField] private List<CardInstance> exhaust = new();
    [SerializeField] private List<CardInstance> remove = new();

    private CharacterBase owner;

    public IReadOnlyList<CardInstance> DeckInstances => deck;
    public IReadOnlyList<CardInstance> HandInstances => hand;
    public IReadOnlyList<CardInstance> GraveyardInstances => graveyard;
    public IReadOnlyList<CardInstance> ExhaustInstances => exhaust;
    public IReadOnlyList<CardInstance> RemoveInstances => remove;

    public int HandCount => hand.Count;

    /// <summary>
    /// 캐릭터에게 덱 모듈을 등록합니다.
    /// </summary>
    public override void OnRegistration(CharacterBase owner)
    {
        base.OnRegistration(owner);
        this.owner = owner;
    }

    /// <summary>
    /// CardInstance를 지정한 카드 영역에 추가합니다.
    /// </summary>
    public void AddCard(CardInstance card, CardZoneType zone)
    {
        if (card == null || card.Data == null)
            return;

        GetZone(zone).Add(card);
    }

    /// <summary>
    /// CardData로 새로운 카드 인스턴스를 생성하여
    /// 지정한 카드 영역에 추가합니다.
    /// </summary>
    public CardInstance AddCard(CardData cardData, CardZoneType zone)
    {
        if (cardData == null)
            return null;

        CardInstance instance = cardData.CreateInstance();

        AddCard(instance, zone);

        return instance;
    }

    /// <summary>
    /// 지정한 카드 인스턴스를 카드 영역에서 제거합니다.
    /// </summary>
    public bool RemoveCard(CardInstance card, CardZoneType zone)
    {
        if (card == null)
            return false;

        return GetZone(zone).Remove(card);
    }

    /// <summary>
    /// 카드 인스턴스를 한 영역에서 다른 영역으로 이동합니다.
    /// </summary>
    public bool MoveCard(CardInstance card, CardZoneType from, CardZoneType to)
    {
        if (!RemoveCard(card, from))
            return false;

        AddCard(card, to);

        return true;
    }

    /// <summary>
    /// 덱에서 카드 인스턴스 한 장을 드로우합니다.
    /// 덱이 비어 있으면 묘지를 덱으로 복귀시킵니다.
    /// </summary>
    public CardInstance DrawInstance()
    {
        if (deck.Count <= 0 && graveyard.Count > 0)
        {
            ShuffleGraveyardIntoDeck();
        }

        if (deck.Count <= 0)
        {
            Debug.Log("덱과 묘지가 모두 비어 있습니다.");

            return null;
        }

        CardInstance card = deck[0];

        deck.RemoveAt(0);
        hand.Add(card);

        CheckHandLimit();

        return card;
    }

    /// <summary>
    /// 손패 제한을 초과한 카드를 묘지로 이동합니다.
    /// </summary>
    private void CheckHandLimit()
    {
        int maxHand = GetMaxHand();

        while (hand.Count > maxHand)
        {
            CardInstance overflowCard = hand[hand.Count - 1];

            hand.RemoveAt(hand.Count - 1);
            graveyard.Add(overflowCard);

            Debug.Log($"핸드 초과로 묘지 이동: " + $"{overflowCard.Data.cardName}");
        }
    }

    /// <summary>
    /// 묘지의 모든 카드를 덱으로 복귀시킨 뒤 셔플합니다.
    /// </summary>
    public void ShuffleGraveyardIntoDeck()
    {
        deck.AddRange(graveyard);
        graveyard.Clear();

        Shuffle(deck);
    }

    /// <summary>
    /// 카드 인스턴스 목록을 무작위로 섞습니다.
    /// </summary>
    private void Shuffle(List<CardInstance> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    /// <summary>
    /// 새로운 카드를 덱에 추가합니다.
    /// </summary>
    public CardInstance AddCardToDeck(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("AddCardToDeck 실패: CardData가 없습니다.");

            return null;
        }

        CardInstance instance = cardData.CreateInstance();

        deck.Add(instance);

        Debug.Log($"덱에 카드 추가: {cardData.cardName}");

        return instance;
    }

    /// <summary>
    /// 이미 존재하는 카드 인스턴스를 덱에 추가합니다.
    /// </summary>
    public void AddCardToDeck(CardInstance card)
    {
        if (card == null || card.Data == null)
            return;

        deck.Add(card);
    }

    /// <summary>
    /// 새로운 카드를 덱에 추가한 뒤 셔플합니다.
    /// </summary>
    public CardInstance AddCardToDeckAndShuffle(CardData cardData)
    {
        CardInstance instance = AddCardToDeck(cardData);

        if (instance == null)
            return null;

        Shuffle(deck);

        return instance;
    }

    /// <summary>
    /// 손패에서 사용한 카드를 지정한 카드 영역으로 이동합니다.
    /// </summary>
    public bool UseCard(CardInstance card, bool isExhaust = false, bool isRemove = false)
    {
        if (card == null || !hand.Contains(card))
            return false;

        CardZoneType targetZone = CardZoneType.Graveyard;

        if (isRemove)
        {
            targetZone = CardZoneType.Remove;
        }
        else if (isExhaust)
        {
            targetZone = CardZoneType.Exhaust;
        }

        return MoveCard(card, CardZoneType.Hand, targetZone);
    }

    /// <summary>
    /// 필드 판정 결과에 따라 사용한 카드를 이동합니다.
    /// </summary>
    public bool ResolveFieldCard(CardInstance card, FieldCardCheckResult result, bool forceRemove = false)
    {
        if (card == null || !hand.Contains(card))
            return false;

        CardZoneType targetZone;

        if (forceRemove || result == FieldCardCheckResult.Fumble)
        {
            targetZone = CardZoneType.Remove;
        }
        else if (result ==
                 FieldCardCheckResult.Failure)
        {
            targetZone = CardZoneType.Exhaust;
        }
        else
        {
            targetZone = CardZoneType.Graveyard;
        }

        return MoveCard(card, CardZoneType.Hand, targetZone);
    }

    /// <summary>
    /// 이벤트 판정에 사용한 색상 카드를 소멸시키고
    /// 새로운 무색 카드를 덱에 추가합니다.
    /// </summary>
    public bool ReplaceEventCardWithColorless(CardInstance usedCard, CardData colorlessCard)
    {
        if (usedCard == null || colorlessCard == null || !hand.Contains(usedCard))
        {
            return false;
        }

        if (usedCard.Data.color == CardColorType.Colorless)
        {
            return false;
        }

        if (!MoveCard(usedCard, CardZoneType.Hand, CardZoneType.Remove))
        {
            return false;
        }

        AddCardToDeck(colorlessCard);
        Shuffle(deck);

        return true;
    }

    /// <summary>
    /// 제외 영역의 모든 카드를 덱으로 복귀시키고 셔플합니다.
    /// </summary>
    public void ReturnAllExhaustToDeck()
    {
        if (exhaust.Count <= 0)
            return;

        deck.AddRange(exhaust);
        exhaust.Clear();

        Shuffle(deck);
    }

    /// <summary>
    /// 전투 종료 시 제외된 카드를 덱으로 복귀시킵니다.
    /// </summary>
    public void ReturnExhaustToDeck()
    {
        ReturnAllExhaustToDeck();
    }

    /// <summary>
    /// 복귀 가능한 소멸 카드 인스턴스 목록을 반환합니다.
    /// 무색 카드는 제외합니다.
    /// </summary>
    public List<CardInstance> GetRecoverableRemovedCardInstances()
    {
        List<CardInstance> result = new();

        foreach (CardInstance card in remove)
        {
            if (card == null || card.Data == null)
                continue;

            if (card.Data.color == CardColorType.Colorless)
            {
                continue;
            }

            result.Add(card);
        }

        return result;
    }

    /// <summary>
    /// 소멸 카드 인스턴스를 덱으로 복귀시키고 셔플합니다.
    /// </summary>
    public bool ReturnRemovedCardToDeck(CardInstance card)
    {
        if (card == null || card.Data == null)
            return false;

        if (card.Data.color == CardColorType.Colorless)
        {
            return false;
        }

        if (!remove.Remove(card))
            return false;

        deck.Add(card);
        Shuffle(deck);

        return true;
    }

    /// <summary>
    /// 덱에서 지정한 카드 인스턴스를 제거합니다.
    /// </summary>
    public bool RemoveCardFromDeck(CardInstance card)
    {
        if (card == null)
            return false;

        return deck.Remove(card);
    }

    /// <summary>
    /// 현재 사용할 수 있는 카드가 완전히 없는지 확인합니다.
    /// </summary>
    public bool IsCardEmpty()
    {
        return deck.Count == 0 &&
               hand.Count == 0 &&
               graveyard.Count == 0;
    }

    /// <summary>
    /// 시작 덱 데이터를 카드 인스턴스로 변환하여 등록합니다.
    /// </summary>
    public void RegisterDeck(DeckData deckData)
    {
        if (deckData == null)
            return;

        deck.Clear();
        hand.Clear();
        graveyard.Clear();
        exhaust.Clear();
        remove.Clear();

        foreach (CardData cardData in deckData.cards)
        {
            if (cardData == null)
                continue;

            deck.Add(cardData.CreateInstance());
        }

        ApplyDeckColorLimit();
        Shuffle(deck);
    }

    /// <summary>
    /// 능력치에 따라 색상별 덱 제한을 적용합니다.
    /// </summary>
    private void ApplyDeckColorLimit()
    {
        StatModules stat = owner?.GetModule<StatModules>();

        if (stat == null)
            return;

        ApplyColorLimit(CardColorType.Red, stat.GetStat(StatType.Strength));

        ApplyColorLimit(CardColorType.Yellow, stat.GetStat(StatType.Agility));

        ApplyColorLimit(CardColorType.Green, stat.GetStat(StatType.Health));

        ApplyColorLimit(CardColorType.Blue, stat.GetStat(StatType.Intelligence));

        ApplyColorLimit(CardColorType.Purple, stat.GetStat(StatType.Will));
    }

    /// <summary>
    /// 지정한 색상의 카드가 제한을 초과하면
    /// 초과 카드를 소멸 영역으로 이동합니다.
    /// </summary>
    private void ApplyColorLimit(CardColorType color, int maxCount)
    {
        int count = 0;

        for (int i = deck.Count - 1;
             i >= 0;
             i--)
        {
            CardInstance card = deck[i];

            if (card == null || card.Data == null)
                continue;

            if (card.Data.color != color)
                continue;

            count++;

            if (count <= maxCount)
                continue;

            deck.RemoveAt(i);
            remove.Add(card);
        }
    }

    /// <summary>
    /// 캐릭터의 최대 손패 수를 반환합니다.
    /// </summary>
    public int GetMaxHand()
    {
        DerivedStatModule derived = Owner.GetModule<DerivedStatModule>();

        return derived != null
            ? derived.GetMaxHand()
            : 5;
    }

    /// <summary>
    /// 지정한 카드 영역의 내부 목록을 반환합니다.
    /// </summary>
    private List<CardInstance> GetZone(CardZoneType zone)
    {
        switch (zone)
        {
            case CardZoneType.Hand:
                return hand;

            case CardZoneType.Graveyard:
                return graveyard;

            case CardZoneType.Exhaust:
                return exhaust;

            case CardZoneType.Remove:
                return remove;

            case CardZoneType.Deck:
            default:
                return deck;
        }
    }

    /// <summary>
    /// 소유자의 턴 시작 시 손패에 있는 활성화된
    /// 광원·점화 카드의 내구도를 1 감소시킵니다.
    /// 내구도가 0이 된 카드는 소멸 영역으로 이동합니다.
    /// </summary>
    /// <returns>소멸한 카드 수</returns>
    public int ProcessHandTurnDurability()
    {
        int removedCount = 0;

        for (int i = hand.Count - 1; i >= 0; i--)
        {
            CardInstance card = hand[i];

            if (card == null || card.Data == null)
            {
                continue;
            }

            int previousDurability = card.CurrentDurability;

            bool consumed = card.ConsumeTurnDurability(true);

            if (!consumed)
                continue;

            Debug.Log(
                $"{card.CardName} 내구도 감소: " +
                $"{previousDurability} → " +
                $"{card.CurrentDurability}");

            if (!card.IsDepleted)
                continue;

            hand.RemoveAt(i);

            card.SetKeywordActive(false);

            remove.Add(card);
            removedCount++;

            Debug.Log($"{card.CardName}: " + "내구도가 0이 되어 소멸");
        }

        return removedCount;
    }

}
