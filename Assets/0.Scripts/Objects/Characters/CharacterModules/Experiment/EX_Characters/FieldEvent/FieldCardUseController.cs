using System;
using UnityEngine;

public class FieldCardUseController : MonoBehaviour
{
    [Header("필드")]
    [SerializeField]
    private FieldManager fieldManager;

    [Header("이벤트 판정 대체 카드")]
    [SerializeField]
    private CardData colorlessReplacementCard;

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

    [Header("필드 카드 사용 공간")]
    [SerializeField]
    private UI_FieldCardUseDropTarget fieldCardUseArea;

    private bool isProcessingCard;

    public event Action<CharacterBase> OnFieldCardResolved;

    private CardInstance pendingUsedCard;
    private CharacterBase pendingUser;
    private DeckModule pendingDeck;
    private FieldEventContext pendingContext;


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

    private void HandleCardSelected(FieldEventChoice choice, CardInstance card)
    {
        if (isProcessingCard)
            return;

        if (choice == null || card == null || card.Data == null)
            return;

        if (fieldManager == null || eventRunner == null || cardSelector == null)
        {
            Debug.LogWarning("FieldCardUseController: 필드 연결이 부족합니다.");

            return;
        }

        if (!eventRunner.IsWaitingStatCheck || eventRunner.PendingStatChoice != choice)
        {
            Debug.LogWarning("현재 처리할 능력치 판정이 없습니다.");

            return;
        }

        if (!choice.CanUseCard(card.Data))
        {
            Debug.Log("이 능력치 판정에 대응하지 않는 카드입니다.");

            return;
        }

        if (colorlessReplacementCard == null)
        {
            Debug.LogWarning("무색 대체 카드가 연결되지 않았습니다.");

            return;
        }

        CharacterBase user = fieldManager.CurrentPlayer;

        if (user == null)
            return;

        DeckModule deck = user.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{user.name}: DeckModule이 없습니다.");

            return;
        }

        if (!ContainsCard(deck, card))
        {
            Debug.LogWarning($"{card.CardName}: 현재 손패에 없는 카드입니다.");

            return;
        }

        isProcessingCard = true;

        bool replaced = deck.ReplaceEventCardWithColorless(card, colorlessReplacementCard);

        if (!replaced)
        {
            isProcessingCard = false;
            return;
        }

        // 선택한 실제 카드의 소멸과 무색 카드 추가가 끝난 뒤
        // 이벤트 판정을 확정 성공으로 처리합니다.
        eventRunner.CompletePendingStatCheckByCard(card);

        if (handUI != null)
        {
            handUI.RefreshFromDeck(deck);
        }

        isProcessingCard = false;

        OnFieldCardResolved?.Invoke(user);
    }

    /// <summary>
    /// 필드에서 직접 사용한 카드의 판정을 실행합니다.
    /// 색상 카드는 공용 판정기를 사용하고,
    /// 무색 카드는 능력치와 상태 보정 없이 판정합니다.
    /// </summary>
    private FieldCardCheckData RollFieldCardCheck(CharacterBase user, CardData card)
    {
        if (user == null || card == null)
        {
            return CreateFailedCheck(card);
        }

        StatModules stat = user.GetModule<StatModules>();

        if (stat == null)
        {
            return CreateFailedCheck(card);
        }

        StatType statType = ResolveFieldCheckStat(user, card);

        if (statType == StatType.None)
        {
            return CreateFailedCheck(card);
        }

        int statValue = stat.GetStat(statType);

        JudgeResult judgeResult;

        if (card.color == CardColorType.Colorless)
        {
            // 무색 카드 직접 사용:
            // 능력치 및 상태 보정 없이 주사위만 사용
            int dice = Dice.RollD10();

            judgeResult = new JudgeResult(dice, 0, 0, card.FieldCheckTarget);
        }
        else
        {
            judgeResult = JudgeUtility.Roll(user, statType, card.FieldCheckTarget);
        }

        FieldCardCheckResult checkResult;

        if (!judgeResult.valid)
        {
            checkResult = FieldCardCheckResult.Failure;
        }
        else if (judgeResult.fumble)
        {
            checkResult = FieldCardCheckResult.Fumble;
        }
        else if (judgeResult.success)
        {
            checkResult = FieldCardCheckResult.Success;
        }
        else
        {
            checkResult = FieldCardCheckResult.Failure;
        }

        Debug.Log(
            $"필드 카드 판정: {card.cardName} / " +
            $"능력치:{statType}({statValue}) / " +
            $"주사위:{judgeResult.dice} + " +
            $"능력 보정:{judgeResult.statModifier} + " +
            $"상태 보정:{judgeResult.statusModifier} " +
            $"= {judgeResult.total} / " +
            $"목표:{judgeResult.target} / " +
            $"결과:{checkResult}"
        );

        return new FieldCardCheckData(
            card,
            statType,
            judgeResult.dice,
            statValue,
            judgeResult.statModifier,
            judgeResult.statusModifier,
            judgeResult.total,
            judgeResult.target,
            checkResult);
    }

    /// <summary>
    /// 카드 색상에 대응하는 필드 판정 능력치를 반환합니다.
    /// 무색 카드는 캐릭터의 지정 능력치를 사용합니다.
    /// </summary>
    private StatType ResolveFieldCheckStat(CharacterBase user, CardData card)
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
                StatModules stat = user?.GetModule<StatModules>();

                return stat != null ? stat.DesignatedStatType : StatType.None;

            default:
                return StatType.None;
        }
    }

    private void HandleFieldCardSelected(CardInstance card, CharacterBase user)
    {
        if (isProcessingCard)
            return;

        if (card == null || card.Data == null || user == null)
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

        TryProcessFieldCard(card, user, context);
    }

    private bool TryProcessFieldCard(CardInstance card, CharacterBase user, FieldEventContext context)
    {
        if (card == null || card.Data == null)
            return false;

        CardData cardData = card.Data;

        DeckModule deck = user.GetModule<DeckModule>();

        ActionPointModule actionPoint = user.GetModule<ActionPointModule>();

        if (deck == null || actionPoint == null)
        {
            Debug.LogWarning("DeckModule 또는 ActionPointModule이 없습니다.");

            return false;
        }

        if (!ContainsCard(deck, card))
        {
            Debug.LogWarning($"{card.CardName}: 현재 손패에 없는 카드입니다.");

            return false;
        }

        if (!actionPoint.CanUse(1))
        {
            Debug.Log("행동력이 부족합니다.");
            return false;
        }

        isProcessingCard = true;

        FieldCardCheckData checkData = RollFieldCardCheck(user, cardData);

        context.SetCardCheck(checkData);

        if (fieldCardUseArea != null)
        {
            fieldCardUseArea.ShowResult(checkData);
        }

        CardResolver resolver = new CardResolver();

        bool effectApplied = resolver.UseField(cardData, user, context);

        if (!effectApplied)
        {
            Debug.LogWarning($"필드 카드 효과 실행 실패: {card.CardName}");

            isProcessingCard = false;
            return false;
        }

        bool forceRemove = cardData.color == CardColorType.Colorless;

        bool moved = deck.ResolveFieldCard(card, checkData.Result, forceRemove);

        if (!moved)
        {
            isProcessingCard = false;
            return false;
        }

        if (context.HasRemovedCardRecoveryRequest)
        {
            return BeginRemovedCardSelection(card, user, deck, context);
        }

        CompleteCardUse(card, user);

        return true;
    }

    private bool ContainsCard(DeckModule deck, CardInstance card)
    {
        foreach (CardInstance handCard in deck.HandInstances)
        {
            if (handCard == card)
                return true;
        }

        return false;
    }

    private bool BeginRemovedCardSelection(CardInstance usedCard, CharacterBase user, DeckModule deck, FieldEventContext context)
    {
        if (removedCardSelectUI == null)
        {
            Debug.LogWarning("FieldCardUseController: " + "제거 카드 선택 UI가 연결되지 않았습니다.");

            context.ClearRemovedCardRecoveryRequest();

            CompleteCardUse(usedCard, user);

            return true;
        }

        pendingUsedCard = usedCard;
        pendingUser = user;
        pendingDeck = deck;
        pendingContext = context;


        bool opened = removedCardSelectUI.Open(context.RemovedCardRecoveryCandidates, HandleRemovedCardSelected);

        if (opened)
            return true;

        ClearPendingRecovery();
        context.ClearRemovedCardRecoveryRequest();

        CompleteCardUse(usedCard, user);

        return true;
    }

    private void HandleRemovedCardSelected(CardInstance selectedCard)
    {
        CardInstance usedCard = pendingUsedCard;

        CharacterBase user = pendingUser;

        DeckModule deck = pendingDeck;

        FieldEventContext context = pendingContext;

        ClearPendingRecovery();

        if (deck != null && selectedCard != null)
        {
            bool returned = deck.ReturnRemovedCardToDeck(selectedCard);

            if (returned)
            {
                Debug.Log($"제거 카드 복귀: {selectedCard.CardName}");
            }
            else
            {
                Debug.LogWarning($"제거 카드 복귀 실패: {selectedCard.CardName}");
            }
        }

        if (context != null)
        {
            context.ClearRemovedCardRecoveryRequest();
        }

        CompleteCardUse(usedCard, user);
    }

    private void ClearPendingRecovery()
    {
        pendingUsedCard = null;
        pendingUser = null;
        pendingDeck = null;
        pendingContext = null;
    }

    private void CompleteCardUse(CardInstance usedCard, CharacterBase user)
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
        int target = card != null ? card.FieldCheckTarget : 0;

        return new FieldCardCheckData(card, StatType.None, 0, 0, 0, 0, 0, target, FieldCardCheckResult.Failure);
    }

}
