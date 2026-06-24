// =========================
// DeckModule.cs
// 캐릭터의 카드 영역 관리
// =========================

using System.Collections.Generic;
using UnityEngine;

public class DeckModule : CharacterModule
{
    public sealed override System.Type RegistrationType
        => typeof(DeckModule);


    /// <summary>
    /// 메인 덱
    /// 드로우 대상
    /// </summary>
    [SerializeField]
    private List<CardData> deck = new();


    /// <summary>
    /// 현재 손패
    /// </summary>
    [SerializeField]
    private List<CardData> hand = new();


    /// <summary>
    /// 사용된 카드
    /// 일반적으로 여기로 이동
    /// </summary>
    [SerializeField]
    private List<CardData> graveyard = new();


    /// <summary>
    /// 일시 제거
    /// 전투 종료 시 복귀 가능
    /// </summary>
    [SerializeField]
    private List<CardData> exhaust = new();


    /// <summary>
    /// 영구 제거
    /// 다시 덱에 들어오지 않음
    /// </summary>
    [SerializeField]
    private List<CardData> remove = new();

   // 캐릭터 정보 가저 오기
    private CharacterBase owner;
    /// <summary>
    /// 캐릭터 정보 받아오기
    /// </summary>
    /// <param name="owner"></param>
    public override void OnRegistration(CharacterBase owner)
    {
        base.OnRegistration(owner);
        this.owner = owner;
    }

    // =========================
    // Getter
    // =========================

    public IReadOnlyList<CardData> Deck => deck;
    public IReadOnlyList<CardData> Hand => hand;
    public IReadOnlyList<CardData> Graveyard => graveyard;
    public IReadOnlyList<CardData> Exhaust => exhaust;
    public IReadOnlyList<CardData> Remove => remove;


    /// <summary>
    /// 특정 영역에 카드 추가
    /// </summary>
    public void AddCard(CardData card, CardZoneType zone)
    {
        if (card == null)
            return;

        GetZone(zone).Add(card);
    }


    /// <summary>
    /// 특정 영역에서 카드 제거
    /// </summary>
    public bool RemoveCard(CardData card, CardZoneType zone)
    {
        if (card == null)
            return false;

        return GetZone(zone).Remove(card);
    }


    /// <summary>
    /// 카드 영역 이동
    /// </summary>
    public bool MoveCard(CardData card, CardZoneType from, CardZoneType to)
    {
        if (!RemoveCard(card, from))
            return false;

        AddCard(card, to);

        return true;
    }


    /// <summary>
    /// 덱에서 손패로 카드 드로우
    /// </summary>
    public CardData Draw()
    {
        // 1. 덱이 비었으면 묘지 회수
        if (deck.Count <= 0)
        {
            if (graveyard.Count > 0)
            {
                ShuffleGraveyardIntoDeck();
            }
        }

        // 2. 회수 후에도 덱이 없으면 진짜 드로우 불가
        if (deck.Count <= 0)
        {
            Debug.Log("덱과 묘지가 모두 비어 있습니다. 게임오버 조건 확인 필요");
            return null;
        }

        // 3. 드로우
        CardData card = deck[0];

        deck.RemoveAt(0);
        hand.Add(card);

        Debug.Log($"드로우: {card.cardName}");

        return card;
    }

    /// <summary>
    /// 현재 손패 수
    /// </summary>
    public int HandCount
    {
        get
        {
            return hand.Count;
        }
    }

    private void CheckHandLimit()
    {
        int maxHand = GetMaxHand();

        while (hand.Count > maxHand)
        {
            CardData overflowCard = hand[hand.Count - 1];

            hand.RemoveAt(hand.Count - 1);
            graveyard.Add(overflowCard);

            Debug.Log($"핸드 초과로 묘지 이동: {overflowCard.cardName}");
        }
    }

    // =========================
    // 셔플
    // =========================

    /// <summary>
    /// 묘지를 덱으로 복귀 후 셔플
    /// </summary>
    public void ShuffleGraveyardIntoDeck()
    {
        deck.AddRange(graveyard);

        graveyard.Clear();

        Shuffle(deck);
    }

    /// <summary>
    /// 리스트 셔플
    /// </summary>
    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex =
                Random.Range(i, list.Count);

            (list[i], list[randomIndex]) =
                (list[randomIndex], list[i]);
        }
    }

    /// <summary>
    /// 게임 중 카드를 덱에 추가한 뒤 셔플
    /// </summary>
    public void AddCardToDeckAndShuffle(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("AddCardToDeckAndShuffle 실패: card가 null");
            return;
        }
        Debug.Log($"추가 전 덱 수: {deck.Count}");

        AddCardToDeck(card);
        Shuffle(deck);

        Debug.Log($"덱에 추가됨: {card.cardName} / 추가 후 덱 수: {deck.Count}");
    }

    /// <summary>
    /// 카드 사용 후 이동 처리
    /// </summary>
    public void UseCard(CardData card, bool isExhaust = false, bool isRemove = false)
    {
        if (!hand.Contains(card))
            return;

        hand.Remove(card);

        // 영구 제거
        if (isRemove)
        {
            remove.Add(card);
            return;
        }

        // 일시 소멸
        if (isExhaust)
        {
            exhaust.Add(card);
            return;
        }

        // 일반 사용
        graveyard.Add(card);
    }
    
    /// <summary>
    /// 게임 중 카드 추가
    /// </summary>
    /// <param name="card"></param>
    public void AddCardToDeck(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("AddCardToDeck 실패: card가 null");
            return;
        }

        deck.Add(card);
        Debug.Log($"AddCardToDeck 실행: {card.cardName}");
    }


    /// <summary>
    /// 게임 중 카드 제거
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public bool RemoveCardFromDeck(CardData card)
    {
        if (card == null)
            return false;

        return deck.Remove(card);
    }

    /// <summary>
    /// 핸드 최대치
    /// </summary>
    /// <returns></returns>
    public int GetMaxHand()
    {
        DerivedStatModule derived = Owner.GetModule<DerivedStatModule>();

        if (derived == null)
            return 5;

        return derived.GetMaxHand();
    }

    // =========================
    // 전투 종료 처리
    // =========================

    /// <summary>
    /// 전투 종료 시
    /// 소멸 카드 덱 복귀
    /// </summary>
    public void ReturnExhaustToDeck()
    {
        deck.AddRange(exhaust);

        exhaust.Clear();

        Shuffle(deck);
    }

    // =========================
    // 영역 가져오기
    // =========================

    private List<CardData> GetZone(CardZoneType zone)
    {
        switch (zone)
        {
            case CardZoneType.Deck:
                return deck;

            case CardZoneType.Hand:
                return hand;

            case CardZoneType.Graveyard:
                return graveyard;

            case CardZoneType.Exhaust:
                return exhaust;

            case CardZoneType.Remove:
                return remove;
        }

        return deck;
    }


    /// <summary>
    /// 사용 가능한 카드가 완전히 없는지 확인한다.
    /// 덱, 손패, 묘지가 모두 비었으면 true.
    /// 소멸/제거 영역은 사용 가능한 카드로 보지 않는다.
    /// </summary>
    public bool IsCardEmpty()
    {
        return deck.Count == 0 && hand.Count == 0 && graveyard.Count == 0;
    }

    /// <summary>
    /// 시작 덱 등록
    /// 기존 카드 영역을 초기화하고 덱 데이터를 복사한다.
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

        foreach (CardData card in deckData.cards)
        {
            if (card == null)
                continue;

            deck.Add(card);
        }

        ApplyDeckColorLimit();

        Shuffle(deck);
    }

    /// <summary>
    /// 능력치에 따른 색상별 덱 제한 적용.
    /// 초과한 카드는 덱에서 제거하고 소멸 영역으로 보낸다.
    /// </summary>
    private void ApplyDeckColorLimit()
    {
        StatModules stat = owner.GetModule<StatModules>();

        if (stat == null)
            return;

        ApplyColorLimit(CardColorType.Red, stat.GetStat(StatType.Strength));
        ApplyColorLimit(CardColorType.Yellow, stat.GetStat(StatType.Agility));
        ApplyColorLimit(CardColorType.Green, stat.GetStat(StatType.Health));
        ApplyColorLimit(CardColorType.Blue, stat.GetStat(StatType.Intelligence));
        ApplyColorLimit(CardColorType.Purple, stat.GetStat(StatType.Will));
    }

    private void ApplyColorLimit(CardColorType color, int maxCount)
    {
        int count = 0;

        for (int i = deck.Count - 1; i >= 0; i--)
        {
            CardData card = deck[i];

            if (card == null)
                continue;

            if (card.color != color)
                continue;

            count++;

            if (count <= maxCount)
                continue;

            deck.RemoveAt(i);
            remove.Add(card);

            Debug.Log($"덱 제한 초과: {card.cardName} → 제거");
        }
    }




}
