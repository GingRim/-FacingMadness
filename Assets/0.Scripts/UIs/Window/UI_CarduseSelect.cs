using UnityEngine;



/// <summary>
/// 카드 사용 선택 팝업.
/// 행동 / 보조 행동 중 어떤 코스트로 사용할지 선택한다.
/// </summary>
public class UI_CardUseSelect : MonoBehaviour
{

    private CardResolver cardResolver;
    private CardData selectedCard;
    private CharacterBase user;
    private CharacterBase target;
    private UI_Hand handUI;

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
    /// 타겟을 지정하는 카드인가?
    /// </summary>
    /// <param name="card"></param>
    /// <param name="useCost"></param>
    /// <returns></returns>
    private bool NeedTarget(CardData card, CardUseCost useCost)
    {
        if (card == null)
            return false;

        if (card.magicCardType != MagicCardType.None)
            return true;

        switch (card.color)
        {
            case CardColorType.Red:
                return useCost == CardUseCost.Action;

            case CardColorType.Yellow:
                return useCost == CardUseCost.Action;

            case CardColorType.Blue:
                return useCost == CardUseCost.Action;

            case CardColorType.Black:
                return true;

            case CardColorType.Purple:
                return false;

            case CardColorType.Green:
                return false;

            case CardColorType.Colorless:
                return useCost == CardUseCost.Action;
        }

        return false;
    }

    /// <summary>
    /// 카드 사용 선택 팝업 열기.
    /// </summary>
    public void Open(CardData card, CharacterBase newUser, CharacterBase newTarget)
    {
        selectedCard = card;
        user = newUser;
        target = newTarget;

        gameObject.SetActive(true);
        Debug.Log(
       $"카드 사용 팝업 열림 / 카드 {(selectedCard != null ? selectedCard.cardName : "null")} / " +
       $"사용자 {(user != null ? user.name : "null")} / " +
       $"대상 {(target != null ? target.name : "null")}"
   );
    }

    /// <summary>
    /// 팝업 닫기.
    /// 취소 버튼 또는 나중에 특정 키 입력에 연결 가능.
    /// </summary>
    public void Close()
    {
        selectedCard = null;
        user = null;
        target = null;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 행동 코스트로 카드 사용.
    /// 버튼 OnClick에 연결한다.
    /// </summary>
    public void UseAction()
    {
        Use(CardUseCost.Action);
    }

    /// <summary>
    /// 보조 행동 코스트로 카드 사용.
    /// 버튼 OnClick에 연결한다.
    /// </summary>
    public void UseAuxiliary()
    {
        Use(CardUseCost.Auxiliary);
    }

    /// <summary>
    /// 선택한 코스트로 카드 사용 실행.
    /// 성공하면 카드 이동과 팝업 닫기를 처리한다.
    /// </summary>
    private void Use(CardUseCost useCost)
    {
        if (selectedCard == null || user == null)
            return;

        if (cardResolver == null)
            cardResolver = new CardResolver();

        if (NeedTarget(selectedCard, useCost) && target == null)
        {
            BattleManager.ClaimBattleLog("대상이 필요한 카드입니다. 먼저 대상을 선택하세요.");
            return;
        }

        if (!cardResolver.CanUse(selectedCard, user, useCost))
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

        bool success = cardResolver.UseWithoutCostCheck(selectedCard, user, target, useCost);

        if (!success)
        {
            BattleManager.ClaimBattleLog("카드 효과 처리 실패");
            return;
        }

        deck.UseCard(selectedCard);

        if (handUI == null)
            handUI = FindFirstObjectByType<UI_Hand>();

        if (handUI != null)
            handUI.RefreshFromDeck(deck);

        Close();
    }

    public void SetTarget(CharacterBase newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            Debug.Log($"카드 대상 선택: {target.name}");
        }
    }
}
