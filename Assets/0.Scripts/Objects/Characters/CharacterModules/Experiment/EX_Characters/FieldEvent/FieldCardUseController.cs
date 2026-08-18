using UnityEngine;

public class FieldCardUseController : MonoBehaviour
{
    [Header("필드")]
    [SerializeField]
    private FieldManager fieldManager;

    [SerializeField]
    private FieldEventRunner eventRunner;

    [Header("UI")]
    [SerializeField]
    private UI_FieldCardSelector cardSelector;

    [SerializeField]
    private UI_Hand handUI;

    [Header("무색 카드 복귀 선택")]
    [SerializeField]
    private UI_FieldRemovedCardSelect removedCardSelectUI;

    private CardData pendingUsedCard;
    private DeckModule pendingDeck;
    private FieldEventContext pendingContext;

    private void OnEnable()
    {
        if (cardSelector == null)
            return;

        cardSelector.OnCardSelected -= HandleCardSelected;

        cardSelector.OnCardSelected += HandleCardSelected;
    }

    private void OnDisable()
    {
        if (cardSelector == null)
            return;

        cardSelector.OnCardSelected -= HandleCardSelected;
    }

    private void HandleCardSelected(FieldEventChoice choice, CardData card)
    {
        if (choice == null || card == null)
            return;

        if (fieldManager == null || eventRunner == null || cardSelector == null)
        {
            Debug.LogWarning("FieldCardUseController: 필드 연결이 부족합니다.");

            return;
        }

        CharacterBase user = fieldManager.CurrentPlayer;

        FieldEventContext context = eventRunner.CurrentContext;

        if (user == null || context == null)
        {
            Debug.LogWarning("필드 카드 사용자 또는 이벤트 Context가 없습니다.");

            return;
        }

        DeckModule deck = user.GetModule<DeckModule>();

        ActionPointModule actionPoint = user.GetModule<ActionPointModule>();

        if (deck == null || actionPoint == null)
        {
            Debug.LogWarning("DeckModule 또는 ActionPointModule이 없습니다.");

            return;
        }

        // CardResolver.UseField에서 실제 차감하기 전에
        // 판정 주사위를 굴리지 않도록 먼저 확인한다.
        if (!actionPoint.CanUse(1))
        {
            Debug.Log("행동력이 부족합니다.");
            return;
        }

        FieldCardCheckData checkData = RollFieldCardCheck(user, card, choice.CardRequirement);

        // 카드 효과와 이벤트 결과가 판정값을 읽을 수 있도록 먼저 저장
        context.SetCardCheck(checkData);

        CardResolver resolver = new CardResolver();

        // 판정 결과와 관계없이 카드의 필드 효과는 실행한다.
        bool effectApplied = resolver.UseField(card, user, context);

        if (!effectApplied)
        {
            Debug.LogWarning($"필드 카드 효과 실행 실패: {card.cardName}");

            return;
        }

        bool forceRemove = card.color == CardColorType.Colorless;

        bool moved = deck.ResolveFieldCard(card, checkData.Result, forceRemove);

        if (!moved)
            return;

        if (handUI != null)
        {
            handUI.RefreshFromDeck(deck);
        }

        // 무색 카드 효과로 제거 카드 복귀 선택이 요청됨
        if (context.HasRemovedCardRecoveryRequest)
        {
            if (removedCardSelectUI == null)
            {
                Debug.LogWarning("FieldCardUseController: 제거 카드 선택 UI가 연결되지 않았습니다.");

                context.ClearRemovedCardRecoveryRequest();

                cardSelector.CompleteSelection(card);
                return;
            }

            pendingUsedCard = card;
            pendingDeck = deck;
            pendingContext = context;

            bool opened = removedCardSelectUI.Open(context.RemovedCardRecoveryCandidates, HandleRemovedCardSelected);

            if (opened)
            {
                // 복귀 카드를 선택할 때까지 이벤트 진행 대기
                return;
            }

            // 표시할 카드가 없거나 UI 열기에 실패한 경우
            ClearPendingRecovery();
            context.ClearRemovedCardRecoveryRequest();
        }

        cardSelector.CompleteSelection(card);
    }

    private FieldCardCheckData RollFieldCardCheck(CharacterBase user, CardData card, FieldCardRequirement requirement)
    {
        StatModules stat = user.GetModule<StatModules>();

        if (stat == null)
        {
            return new FieldCardCheckData(card, StatType.None, 0, 0, 0, 0, 0, FieldCardCheckResult.Failure);
        }

        StatType statType = ResolveCheckStat(card, requirement);

        if (statType == StatType.None)
        {
            return new FieldCardCheckData(card, statType, 0, 0, 0, 0, 0, FieldCardCheckResult.Failure);
        }

        int statValue = stat.GetStat(statType);

        int abilityModifier = stat.GetModifier(statType);

        StatusEffectModule status = user.GetModule<StatusEffectModule>();

        int dice;
        int statusModifier = 0;

        if (status != null)
        {
            dice = status.RollJudgeDice();
            statusModifier = status.GetJudgeBonus();
        }
        else
        {
            dice = Dice.RollD10();
        }

        int judgmentValue = Mathf.Max(0, dice + abilityModifier + statusModifier);

        FieldCardCheckResult result;

        if (judgmentValue <= 1)
        {
            result = FieldCardCheckResult.Fumble;
        }
        else if (dice <= statValue)
        {
            result = FieldCardCheckResult.Success;
        }
        else
        {
            result = FieldCardCheckResult.Failure;
        }

        if (status != null)
        {
            status.ConsumeJudgeStatus();
        }

        Debug.Log($"필드 판정: {card.cardName} / " + $"주사위:{dice} + 능력 보정:{abilityModifier} " +
            $"+ 상태 보정:{statusModifier} = {judgmentValue} / " + $"결과:{result}");

        return new FieldCardCheckData(card, statType, dice, statValue, abilityModifier, statusModifier, judgmentValue, result);
    }

    private StatType ResolveCheckStat(CardData card, FieldCardRequirement requirement)
    {
        if (card == null)
            return StatType.None;

        switch (card.color)
        {
            case CardColorType.Red:
                return StatType.Strength;

            case CardColorType.Yellow:
                return StatType.Agility;

            case CardColorType.Green:
                return StatType.Health;

            case CardColorType.Blue:
                return StatType.Intelligence;

            case CardColorType.Purple:
                return StatType.Will;

            case CardColorType.Colorless:
                return requirement != null
                    ? requirement.CheckStat
                    : StatType.None;

            default:
                return requirement != null
                    ? requirement.CheckStat
                    : StatType.None;
        }
    }
    private void HandleRemovedCardSelected(CardData selectedCard)
    {
        CardData usedCard = pendingUsedCard;
        DeckModule deck = pendingDeck;
        FieldEventContext context = pendingContext;

        ClearPendingRecovery();

        if (deck != null && selectedCard != null)
        {
            bool returned = deck.ReturnRemovedCardToDeck(selectedCard);

            if (!returned)
            {
                Debug.LogWarning($"제거 카드 복귀 실패: {selectedCard.cardName}");
            }
            else
            {
                Debug.Log($"제거 카드 복귀: {selectedCard.cardName}");
            }
        }

        if (context != null)
        {
            context.ClearRemovedCardRecoveryRequest();
        }

        // 복귀 카드 선택까지 끝났으므로 이벤트 선택 처리 재개
        if (usedCard != null && cardSelector != null)
        {
            cardSelector.CompleteSelection(usedCard);
        }
    }

    private void ClearPendingRecovery()
    {
        pendingUsedCard = null;
        pendingDeck = null;
        pendingContext = null;
    }

}
