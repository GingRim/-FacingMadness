using UnityEngine;

/// <summary>
/// 전투 드롭 대상에 놓인 카드의 대상, 코스트와 효과를 처리합니다.
/// CardClick은 전투 규칙을 알지 않고 CardDropTarget을 통해 이곳으로 전달합니다.
/// </summary>
public class BattleCardUseController : MonoBehaviour
{
    [Header("전투 카드 UI")]
    [SerializeField]
    private UI_CardUseSelect useSelectUI;

    [SerializeField]
    private UI_Hand handUI;

    private CardResolver resolver;

    /// <summary>
    /// 동적으로 생성된 전투 화면의 UI 참조를 연결합니다.
    /// </summary>
    public void Configure(UI_CardUseSelect newUseSelectUI, UI_Hand newHandUI)
    {
        useSelectUI = newUseSelectUI;
        handUI = newHandUI;
    }

    /// <summary>
    /// 지정한 전투 대상에게 실제 카드 한 장을 사용합니다.
    /// </summary>
    public bool TryUseCard(CardInstance card, CharacterBase target)
    {
        if (card == null || card.Data == null)
            return false;

        CharacterBase user = FindControlledCharacter();

        if (user == null)
        {
            Debug.LogWarning("전투 카드를 사용할 플레이어를 찾지 못했습니다.");

            return false;
        }

        CardDropDecision decision = GetDropDecision(card.Data, user, target);

        switch (decision.Result)
        {
            case CardDropResult.OpenPopup:
                return OpenPopup(card, user, target);

            case CardDropResult.UseDirect:
                return TryUseCardDirect(card, user, target, decision.UseCost);

            default:
                return false;
        }
    }

    private bool TryUseCardDirect(CardInstance card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (resolver == null)
        {
            resolver = new CardResolver();
        }

        CardData cardData = card.Data;

        if (!resolver.CanUse(cardData, user, useCost))
        {
            BattleManager.ClaimBattleLog("코스트가<br>부족합니다.");

            return false;
        }

        DeckModule deck = user.GetModule<DeckModule>();

        if (deck == null)
            return false;

        bool effectApplied = resolver.UseWithoutCostCheck(cardData, user, target, useCost);

        if (!effectApplied)
            return false;

        bool isExhaust = ShouldExhaustOnUse(cardData);

        bool moved = deck.UseCard(card, isExhaust);

        if (!moved)
        {
            Debug.LogWarning(
                $"{cardData.cardName}: " +
                "선택한 카드 인스턴스를 " +
                "손패에서 찾지 못했습니다.");

            return false;
        }

        RefreshHand(deck);

        return true;
    }

    private bool OpenPopup(CardInstance card, CharacterBase user, CharacterBase target)
    {
        if (useSelectUI == null)
        {
            useSelectUI = FindFirstObjectByType<UI_CardUseSelect>(FindObjectsInactive.Include);
        }

        if (useSelectUI == null)
        {
            Debug.LogWarning("전투 카드 사용 선택 UI가 없습니다.");

            return false;
        }

        if (handUI != null)
        {
            useSelectUI.SetHandUI(handUI);
        }

        useSelectUI.Open(card, user, target);

        return true;
    }

    private void RefreshHand(DeckModule deck)
    {
        if (handUI == null)
        {
            UI_BattleScreen battleScreen = GetComponentInParent<UI_BattleScreen>();

            if (battleScreen != null)
            {
                handUI = battleScreen.GetComponentInChildren<UI_Hand>(true);
            }
        }

        if (handUI != null)
        {
            handUI.RefreshFromDeck(deck);
        }
    }

    private CharacterBase FindControlledCharacter()
    {
        CharacterBase[] characters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);

        foreach (CharacterBase character in characters)
        {
            if (character != null &&
                character.Controller != null)
            {
                return character;
            }
        }

        return null;
    }

    private bool ShouldExhaustOnUse(CardData card)
    {
        if (card == null)
            return false;

        if (card.color == CardColorType.Purple && card.magicCardType == MagicCardType.None)
        {
            return true;
        }

        return
            card.magicCardType != MagicCardType.None;
    }

    private TeamType GetTargetTeamType(CharacterBase user, CharacterBase target)
    {
        if (user == null || target == null)
            return TeamType.None;

        if (user == target)
            return TeamType.Self;

        bool userIsPlayer = user.Controller != null;

        bool targetIsPlayer = target.Controller != null;

        return
            userIsPlayer == targetIsPlayer ? TeamType.Ally : TeamType.Enemy;
    }

    private CardDropDecision GetDropDecision(CardData card, CharacterBase user, CharacterBase target)
    {
        if (card == null || user == null)
        {
            return CardDropDecision.Invalid();
        }

        if (card.magicCardType != MagicCardType.None)
        {
            return GetMagicDropDecision(card, user, target);
        }

        switch (card.color)
        {
            case CardColorType.Red:
                return GetRedDropDecision(user, target);

            case CardColorType.Yellow:
                return GetYellowDropDecision(user, target);

            case CardColorType.Green:
                return GetGreenDropDecision(user, target);

            case CardColorType.Blue:
                return GetBlueDropDecision(user, target);

            case CardColorType.Purple:
                return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);

            case CardColorType.Colorless:
                return GetColorlessDropDecision(user, target);

            default:
                Debug.Log($"{card.cardName}: " + "전투 드롭 규칙이 없는 카드 색상입니다.");

                return CardDropDecision.Invalid();
        }
    }

    private CardDropDecision GetMagicDropDecision(CardData card, CharacterBase user, CharacterBase target)
    {
        TeamType targetType = GetTargetTeamType(user, target);

        switch (card.magicCardType)
        {
            case MagicCardType.Attack:
                if (targetType == TeamType.Enemy)
                {
                    return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);
                }

                BattleManager.ClaimBattleLog("공격 마법 사용 불가<br>" + "적 대상만 가능");

                return CardDropDecision.Invalid();

            case MagicCardType.Defense:
                if (targetType == TeamType.Self || targetType == TeamType.Ally)
                {
                    return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);
                }

                BattleManager.ClaimBattleLog("방어 마법 사용 불가<br>" + "자신 또는 아군 대상만 가능");

                return CardDropDecision.Invalid();

            case MagicCardType.Buff:
                return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);

            case MagicCardType.Forbidden:
                if (targetType == TeamType.Enemy)
                {
                    return CardDropDecision.Direct(CardUseCost.ActionAndAuxiliary);
                }

                BattleManager.ClaimBattleLog("금지된 마법 사용 불가<br>" + "적 대상만 가능");

                return CardDropDecision.Invalid();

            default:
                return CardDropDecision.Invalid();
        }
    }

    private CardDropDecision GetRedDropDecision(CharacterBase user, CharacterBase target)
    {
        if (GetTargetTeamType(user, target) == TeamType.Enemy)
        {
            return CardDropDecision.Popup();
        }

        BattleManager.ClaimBattleLog("적색 카드 사용 불가<br>" + "적 대상만 가능");

        return CardDropDecision.Invalid();
    }

    private CardDropDecision GetYellowDropDecision(CharacterBase user, CharacterBase target)
    {
        switch (GetTargetTeamType(user, target))
        {
            case TeamType.Enemy:
                return CardDropDecision.Direct(CardUseCost.Action);

            case TeamType.Self:
            case TeamType.Ally:
                return CardDropDecision.Direct(CardUseCost.Auxiliary);

            default:
                return CardDropDecision.Invalid();
        }
    }

    private CardDropDecision GetGreenDropDecision(CharacterBase user, CharacterBase target)
    {
        switch (GetTargetTeamType(user, target))
        {
            case TeamType.Self:
                return CardDropDecision.Popup();

            case TeamType.Ally:
                return CardDropDecision.Direct(CardUseCost.Auxiliary);

            default:
                BattleManager.ClaimBattleLog("녹색 카드 사용 불가<br>" + "팀 대상만 가능");

                return CardDropDecision.Invalid();
        }
    }

    private CardDropDecision GetBlueDropDecision(CharacterBase user, CharacterBase target)
    {
        switch (GetTargetTeamType(user, target))
        {
            case TeamType.Enemy:
                return CardDropDecision.Direct(CardUseCost.Action);

            case TeamType.Self:
            case TeamType.Ally:
                return CardDropDecision.Direct(CardUseCost.Auxiliary);

            default:
                return CardDropDecision.Invalid();
        }
    }

    private CardDropDecision GetColorlessDropDecision(CharacterBase user, CharacterBase target)
    {
        switch (GetTargetTeamType(user, target))
        {
            case TeamType.Enemy:
                return CardDropDecision.Direct(CardUseCost.Action);

            case TeamType.Self:
            case TeamType.Ally:
                return CardDropDecision.Direct(CardUseCost.Auxiliary);

            default:
                return CardDropDecision.Invalid();
        }
    }

    private readonly struct CardDropDecision
    {
        public CardDropResult Result { get; }
        public CardUseCost UseCost { get; }

        private CardDropDecision(CardDropResult result, CardUseCost useCost)
        {
            Result = result;
            UseCost = useCost;
        }

        public static CardDropDecision Invalid()
        {
            return new CardDropDecision(CardDropResult.Invalid, CardUseCost.Action);
        }

        public static CardDropDecision Popup()
        {
            return new CardDropDecision(CardDropResult.OpenPopup, CardUseCost.Action);
        }

        public static CardDropDecision Direct(CardUseCost useCost)
        {
            return new CardDropDecision(CardDropResult.UseDirect, useCost);
        }
    }
}
