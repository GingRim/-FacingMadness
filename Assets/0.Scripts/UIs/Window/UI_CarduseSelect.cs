using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



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

    [Header("선택 화면")]
    [Tooltip("기존 행동/보조 행동 버튼의 상위 오브젝트입니다.")]
    [SerializeField] private GameObject costChoiceRoot;

    [Tooltip("공격 방식 버튼이 생성될 영역입니다.")]
    [SerializeField] private Transform attackStyleButtonRoot;

    [Tooltip("비활성화한 공격 방식 버튼 원본을 연결합니다.")]
    [SerializeField] private Button attackStyleButtonTemplate;

    private readonly List<Button> generatedAttackStyleButtons = new();
    private bool showingAttackStyles;

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

        RefreshChoiceMode();

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
        ClearAttackStyleButtons();
        showingAttackStyles = false;

        if (costChoiceRoot != null)
            costChoiceRoot.SetActive(true);

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
        if (showingAttackStyles)
            return;

        Use(CardUseCost.Action);
    }

    /// <summary>
    /// 보조 행동 코스트로 사용합니다.
    /// </summary>
    public void UseAuxiliary()
    {
        if (showingAttackStyles)
            return;

        Use(CardUseCost.Auxiliary);
    }

    private void RefreshChoiceMode()
    {
        ClearAttackStyleButtons();

        showingAttackStyles =
            SelectedCardData != null &&
            SelectedCardData.HasAttackStyles &&
            target != null && target != user;

        if (costChoiceRoot != null)
            costChoiceRoot.SetActive(!showingAttackStyles);

        if (!showingAttackStyles)
            return;

        if (attackStyleButtonRoot == null || attackStyleButtonTemplate == null)
        {
            Debug.LogWarning(
                "공격 방식 선택 UI의 버튼 영역 또는 버튼 원본이 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < SelectedCardData.AttackStyles.Count; i++)
        {
            CardAttackStyleData style = SelectedCardData.GetAttackStyle(i);

            if (style == null)
                continue;

            int selectedIndex = i;
            Button button = Instantiate(
                attackStyleButtonTemplate,
                attackStyleButtonRoot);

            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => UseAttackStyle(selectedIndex));

            UI_CardAttackStyleButton presentation =
                button.GetComponent<UI_CardAttackStyleButton>();

            if (presentation != null)
            {
                presentation.SetStyle(style);
            }
            else
            {
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

                if (label != null)
                {
                    string description = string.IsNullOrWhiteSpace(style.Description)
                        ? string.Empty
                        : $"\n{style.Description}";

                    label.text = style.DisplayName + description;
                }
            }

            generatedAttackStyleButtons.Add(button);
        }
    }

    private void ClearAttackStyleButtons()
    {
        foreach (Button button in generatedAttackStyleButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        generatedAttackStyleButtons.Clear();
    }

    public void UseAttackStyle(int attackStyleIndex)
    {
        if (!showingAttackStyles || selectedCardInstance == null ||
            SelectedCardData == null || user == null || target == null)
        {
            return;
        }

        if (cardResolver == null)
            cardResolver = new CardResolver();

        DeckModule deck = user.GetModule<DeckModule>();

        if (deck == null)
            return;

        bool success = cardResolver.UseAttackStyle(
            selectedCardInstance,
            user,
            target,
            attackStyleIndex,
            out int durabilityCost);

        if (!success)
        {
            BattleManager.ClaimBattleLog("공격 처리 실패");
            return;
        }

        CompleteCardUse(deck, durabilityCost);
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

        if (!cardResolver.CanUse(selectedCardInstance, user, useCost))
        {
            BattleManager.ClaimBattleLog("행동력이 부족합니다.");

            Close();
            return;
        }

        DeckModule deck = user.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{user.name}: DeckModule 없음");

            return;
        }

        bool success = cardResolver.UseWithoutCostCheck(
            selectedCardInstance,
            user,
            target,
            useCost);

        if (!success)
        {
            BattleManager.ClaimBattleLog("카드 효과 처리 실패");

            return;
        }

        CompleteCardUse(deck, 1);
    }

    private void CompleteCardUse(DeckModule deck, int durabilityCost)
    {
        if (deck == null || selectedCardInstance == null || SelectedCardData == null)
            return;

        CardData cardData = SelectedCardData;

        bool isRemove = false;

        if (selectedCardInstance.Color == CardColorType.Colorless)
        {
            if (!selectedCardInstance.ConsumeDurability(
                    Mathf.Max(1, durabilityCost)))
                return;

            isRemove = selectedCardInstance.IsDepleted;
        }

        bool moved = deck.UseCard(
            selectedCardInstance,
            false,
            isRemove);

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

        if (IsOpened)
            RefreshChoiceMode();

        if (target != null)
        {
            Debug.Log($"카드 대상 선택: " + $"{target.name}");
        }
    }
}
