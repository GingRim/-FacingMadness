using UnityEngine;



/// <summary>
/// 카드 사용 선택 팝업.
/// 행동 / 보조 행동 중 어떤 코스트로 사용할지 선택한다.
/// </summary>
public class UI_CardUseSelect : MonoBehaviour
{
    private CardResolver cardResolver;

    private CardInstance selectedCardInstance;

    private CharacterBase user;
    private CharacterBase target;

    private UI_Hand handUI;

    private CardData SelectedCardData => selectedCardInstance != null ? selectedCardInstance.Data : null;

    public bool IsOpened => gameObject.activeSelf;

    private void Awake()
    {
        cardResolver = new CardResolver();

        handUI = FindFirstObjectByType<UI_Hand>();

        gameObject.SetActive(false);
    }

    public void SetHandUI(UI_Hand ui)
    {
        handUI = ui;
    }

    /// <summary>
    /// 해당 카드 사용 방식에 대상이 필요한지 확인합니다.
    /// </summary>
    private bool NeedTarget(CardData card, CardUseCost useCost)
    {
        if (card == null)
            return false;

        if (card.magicCardType != MagicCardType.None)
        {
            return true;
        }

        switch (card.color)
        {
            case CardColorType.Red:
                return
                    useCost ==
                    CardUseCost.Action;

            case CardColorType.Yellow:
                return
                    useCost ==
                    CardUseCost.Action;

            case CardColorType.Blue:
                return
                    useCost ==
                    CardUseCost.Action;

            case CardColorType.Black:
                return true;

            case CardColorType.Purple:
            case CardColorType.Green:
                return false;

            case CardColorType.Colorless:
                return
                    useCost ==
                    CardUseCost.Action;

            default:
                return false;
        }
    }

    /// <summary>
    /// 카드 사용 선택 팝업을 엽니다.
    /// </summary>
    public void Open(CardInstance cardInstance, CharacterBase newUser, CharacterBase newTarget)
    {
        if (cardInstance == null || cardInstance.Data == null || newUser == null)
        {
            return;
        }

        selectedCardInstance = cardInstance;

        user = newUser;
        target = newTarget;

        gameObject.SetActive(true);

        Debug.Log(
            $"카드 사용 팝업 열림 / " +
            $"카드 {SelectedCardData.cardName} / " +
            $"사용자 {user.name} / " +
            $"대상 " +
            $"{(target != null ? target.name : "null")}");
    }

    /// <summary>
    /// 팝업을 닫고 선택 정보를 초기화합니다.
    /// </summary>
    public void Close()
    {
        selectedCardInstance = null;

        user = null;
        target = null;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 행동 코스트로 사용합니다.
    /// </summary>
    public void UseAction()
    {
        Use(CardUseCost.Action);
    }

    /// <summary>
    /// 보조 행동 코스트로 사용합니다.
    /// </summary>
    public void UseAuxiliary()
    {
        Use(CardUseCost.Auxiliary);
    }

    /// <summary>
    /// 선택한 코스트로 카드를 사용합니다.
    /// </summary>
    private void Use(CardUseCost useCost)
    {
        if (selectedCardInstance == null || SelectedCardData == null || user == null)
        {
            return;
        }

        CardData cardData = SelectedCardData;

        if (cardResolver == null)
        {
            cardResolver = new CardResolver();
        }

        if (NeedTarget(cardData, useCost) && target == null)
        {
            BattleManager.ClaimBattleLog("대상이 필요한 카드입니다." + "<br>먼저 대상을 선택하세요.");

            return;
        }

        if (!cardResolver.CanUse(cardData, user, useCost))
        {
            BattleManager.ClaimBattleLog("코스트가 부족합니다.");

            Close();
            return;
        }

        DeckModule deck = user.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{user.name}: DeckModule 없음");

            return;
        }

        bool success = cardResolver.UseWithoutCostCheck(cardData, user, target, useCost);

        if (!success)
        {
            BattleManager.ClaimBattleLog("카드 효과 처리 실패");

            return;
        }

        bool moved = deck.UseCard(selectedCardInstance);

        if (!moved)
        {
            Debug.LogWarning(
                $"{cardData.cardName}: " +
                "선택한 카드 인스턴스가 " +
                "손패에 없습니다.");

            return;
        }

        if (handUI == null)
        {
            handUI = FindFirstObjectByType<UI_Hand>();
        }

        if (handUI != null)
        {
            handUI.RefreshFromDeck(deck);
        }

        Close();
    }

    public void SetTarget(CharacterBase newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            Debug.Log($"카드 대상 선택: " + $"{target.name}");
        }
    }
}