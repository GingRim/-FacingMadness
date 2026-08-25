using System;
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

    [Header("필드 카드 입력")]
    [SerializeField]
    private UI_FieldScreen fieldScreen;

    private bool isProcessingCard;

    public event Action<CharacterBase> OnFieldCardResolved;

    private CardData pendingUsedCard;
    private CharacterBase pendingUser;
    private DeckModule pendingDeck;
    private FieldEventContext pendingContext;

    private bool pendingFromEventSelection;

    private void OnEnable()
    {
        // 이벤트 선택지에서 요구한 카드 선택
        if (cardSelector != null)
        {
            cardSelector.OnCardSelected -= HandleCardSelected;

            cardSelector.OnCardSelected += HandleCardSelected;
        }

        // 일반 필드 손패 카드 사용
        if (fieldScreen != null)
        {
            fieldScreen.OnFieldCardSelected -= HandleFieldCardSelected;

            fieldScreen.OnFieldCardSelected += HandleFieldCardSelected;
        }
    }

    private void OnDisable()
    {
        if (cardSelector != null)
        {
            cardSelector.OnCardSelected -= HandleCardSelected;
        }

        if (fieldScreen != null)
        {
            fieldScreen.OnFieldCardSelected -= HandleFieldCardSelected;
        }

        isProcessingCard = false;
    }

    private void HandleCardSelected(FieldEventChoice choice, CardData card)
    {
        if (isProcessingCard)
            return;

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

        TryProcessFieldCard(card, user, context, choice.CardRequirement, true);
    }

    private FieldCardCheckData RollFieldCardCheck(CharacterBase user, CardData card, FieldCardRequirement requirement)
    {
        StatModules stat = user.GetModule<StatModules>();

        if (stat == null)
        {
            return CreateFailedCheck(card);
        }

        StatType statType = ResolveCheckStat(user, card, requirement);

        if (statType == StatType.None)
        {
            return CreateFailedCheck(card);
        }

        int statValue = stat.GetStat(statType);

        // 이벤트 밖에서 직접 사용하는 무색 카드인지 확인
        bool isDirectColorless = card.color == CardColorType.Colorless && requirement == null;

        int dice;
        int abilityModifier = 0;
        int statusModifier = 0;

        StatusEffectModule status = user.GetModule<StatusEffectModule>();

        if (isDirectColorless)
        {
            // 무색 카드 직접 사용:
            // 지정 능력치 판정, 모든 보정 미적용
            dice = Dice.RollD10();
        }
        else
        {
            abilityModifier = stat.GetModifier(statType);

            if (status != null)
            {
                dice = status.RollJudgeDice();

                statusModifier = status.GetJudgeBonus();
            }
            else
            {
                dice = Dice.RollD10();
            }
        }

        int judgmentValue = Mathf.Max(0, dice + abilityModifier + statusModifier);

        FieldCardCheckResult result;

        // 펌블은 주사위 단독이 아니라
        // 모든 보정이 적용된 최종 판정값 기준
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

        // 직접 사용한 무색 카드는
        // 축복·저주 등의 판정 상태를 소비하지 않음
        if (!isDirectColorless && status != null)
        {
            status.ConsumeJudgeStatus();
        }

        Debug.Log($"필드 판정: {card.cardName} / " + $"능력치:{statType}({statValue}) / " + $"주사위:{dice} + " +
            $"능력 보정:{abilityModifier} + " + $"상태 보정:{statusModifier} " + $"= {judgmentValue} / 결과:{result}"
        );

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

    private void HandleFieldCardSelected(CardData card, CharacterBase user)
    {
        if (isProcessingCard)
            return;

        if (card == null || user == null)
            return;

        if (fieldManager == null || !fieldManager.IsFieldActive)
        {
            return;
        }

        if (fieldManager.TurnState != FieldTurnState.PlayerAction)
        {
            return;
        }

        if (fieldManager.CurrentPlayer != user)
            return;

        FieldNode currentNode = fieldManager.CurrentNode;

        if (currentNode == null)
            return;

        FieldEventContext context = new FieldEventContext(user, currentNode, fieldManager);

        TryProcessFieldCard(card, user, context, null, false);
    }

    private bool TryProcessFieldCard(CardData card, CharacterBase user, FieldEventContext context, FieldCardRequirement requirement, bool fromEventSelection)
    {
        DeckModule deck = user.GetModule<DeckModule>();

        ActionPointModule actionPoint = user.GetModule<ActionPointModule>();

        if (deck == null || actionPoint == null)
        {
            Debug.LogWarning("DeckModule 또는 ActionPointModule이 없습니다.");

            return false;
        }

        if (!ContainsCard(deck, card))
        {
            Debug.LogWarning($"{card.cardName}: 현재 손패에 없는 카드입니다.");

            return false;
        }

        if (!actionPoint.CanUse(1))
        {
            Debug.Log("행동력이 부족합니다.");
            return false;
        }

        isProcessingCard = true;

        FieldCardCheckData checkData = RollFieldCardCheck(user, card, requirement);

        context.SetCardCheck(checkData);

        CardResolver resolver = new CardResolver();

        bool effectApplied = resolver.UseField(card, user, context);

        if (!effectApplied)
        {
            Debug.LogWarning($"필드 카드 효과 실행 실패: {card.cardName}");

            isProcessingCard = false;
            return false;
        }

        bool forceRemove = card.color == CardColorType.Colorless;

        bool moved = deck.ResolveFieldCard(card, checkData.Result, forceRemove);

        if (!moved)
        {
            isProcessingCard = false;
            return false;
        }

        if (context.HasRemovedCardRecoveryRequest)
        {
            return BeginRemovedCardSelection(card, user, deck, context, fromEventSelection);
        }

        CompleteCardUse(card, user, fromEventSelection);

        return true;
    }

    private bool ContainsCard(DeckModule deck, CardData card)
    {
        foreach (CardData handCard in deck.Hand)
        {
            if (handCard == card)
                return true;
        }

        return false;
    }

    private bool BeginRemovedCardSelection(CardData usedCard, CharacterBase user, DeckModule deck, FieldEventContext context, bool fromEventSelection)
    {
        if (removedCardSelectUI == null)
        {
            Debug.LogWarning("FieldCardUseController: " + "제거 카드 선택 UI가 연결되지 않았습니다.");

            context.ClearRemovedCardRecoveryRequest();

            CompleteCardUse(usedCard, user, fromEventSelection);

            return true;
        }

        pendingUsedCard = usedCard;
        pendingUser = user;
        pendingDeck = deck;
        pendingContext = context;
        pendingFromEventSelection = fromEventSelection;

        bool opened = removedCardSelectUI.Open(context.RemovedCardRecoveryCandidates, HandleRemovedCardSelected);

        if (opened)
            return true;

        ClearPendingRecovery();
        context.ClearRemovedCardRecoveryRequest();

        CompleteCardUse(usedCard, user, fromEventSelection);

        return true;
    }

    private void HandleRemovedCardSelected(CardData selectedCard)
    {
        CardData usedCard = pendingUsedCard;

        CharacterBase user = pendingUser;

        DeckModule deck = pendingDeck;

        FieldEventContext context = pendingContext;

        bool fromEventSelection = pendingFromEventSelection;

        ClearPendingRecovery();

        if (deck != null && selectedCard != null)
        {
            bool returned = deck.ReturnRemovedCardToDeck(selectedCard);

            if (returned)
            {
                Debug.Log($"제거 카드 복귀: {selectedCard.cardName}");
            }
            else
            {
                Debug.LogWarning($"제거 카드 복귀 실패: {selectedCard.cardName}");
            }
        }

        if (context != null)
        {
            context.ClearRemovedCardRecoveryRequest();
        }

        CompleteCardUse(usedCard, user, fromEventSelection);
    }

    private void ClearPendingRecovery()
    {
        pendingUsedCard = null;
        pendingUser = null;
        pendingDeck = null;
        pendingContext = null;
        pendingFromEventSelection = false;
    }

    private void CompleteCardUse(CardData usedCard, CharacterBase user, bool fromEventSelection)
    {
        isProcessingCard = false;

        if (user != null)
        {
            DeckModule deck = user.GetModule<DeckModule>();

            if (handUI != null)
            {
                if (deck != null)
                {
                    handUI.RefreshFromDeck(deck);
                }
                else
                {
                    handUI.ClearHand();
                }
            }

            OnFieldCardResolved?.Invoke(user);
        }

        if (fromEventSelection)
        {
            if (usedCard != null && cardSelector != null)
            {
                cardSelector.CompleteSelection(usedCard);
            }

            return;
        }

        if (fieldManager != null)
        {
            fieldManager.CompleteCardAction();
        }
    }
    private StatType ResolveCheckStat(CharacterBase user, CardData card, FieldCardRequirement requirement)
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
                if (requirement != null)
                    return requirement.CheckStat;

                StatModules stat = user?.GetModule<StatModules>();

                return stat != null
                    ? stat.DesignatedStatType
                    : StatType.None;

            default:
                return requirement != null
                    ? requirement.CheckStat
                    : StatType.None;
        }
    }

    private FieldCardCheckData CreateFailedCheck(CardData card)
    {
        return new FieldCardCheckData(card, StatType.None, 0, 0, 0, 0, 0, FieldCardCheckResult.Failure);
    }
}
