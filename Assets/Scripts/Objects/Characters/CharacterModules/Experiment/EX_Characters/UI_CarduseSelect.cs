using UnityEngine;


/// <summary>
/// 카드 사용 선택 팝업.
/// 행동 / 보조 행동 중 어떤 코스트로 사용할지 선택한다.
/// </summary>
public class UI_CardUseSelect : MonoBehaviour
{
    private CardData selectedCard;
    private CharacterBase user;
    private CharacterBase target;
    private UI_Hand handUI;


    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetHandUI(UI_Hand ui)
    {
        handUI = ui;
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

        CardResolver resolver = new CardResolver();

        bool success =
            resolver.Use(selectedCard, user, target, useCost);

        if (!success)
        {
            Debug.Log("카드 사용 실패");
            return;
        }

        DeckModule deck =
            user.GetModule<DeckModule>();

        if (deck != null)
        {
            // 손패 → 묘지
            deck.UseCard(selectedCard);
            handUI?.RefreshFromDeck(deck);
        }

        Close();
    }
}
