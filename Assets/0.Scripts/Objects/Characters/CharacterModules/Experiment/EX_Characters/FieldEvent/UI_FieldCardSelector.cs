using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 능력치 판정 대기 상태를 표시하고
/// 직접 판정 또는 대응 카드 사용을 연결합니다.
/// </summary>

public class UI_FieldCardSelector : MonoBehaviour
{
    [Header("이벤트 실행기")]
    [SerializeField]
    private FieldEventRunner eventRunner;

    [Header("판정 UI")]
    [SerializeField]
    private GameObject checkPanel;

    [SerializeField]
    private TextMeshProUGUI guideText;

    [SerializeField]
    private Button directRollButton;

    private FieldEventChoice pendingChoice;

    public bool IsSelectingCard => pendingChoice != null && eventRunner != null && eventRunner.IsWaitingStatCheck;

    /// <summary>
    /// 유효한 대응 카드가 드롭되었을 때 발생합니다.
    /// 실제 카드 소멸과 무색 카드 추가는 FieldCardUseController가 담당합니다.
    /// </summary>
    public event Action<FieldEventChoice, CardData> OnCardSelected;

    /// <summary>
    /// 초기 UI와 직접 판정 버튼을 설정합니다.
    /// </summary>
    private void Awake()
    {
        if (directRollButton != null)
        {
            directRollButton.onClick.RemoveListener(HandleDirectRoll);
            directRollButton.onClick.AddListener(HandleDirectRoll);
        }

        SetCheckPanelActive(false);
    }

    /// <summary>
    /// FieldEventRunner 이벤트를 등록합니다.
    /// </summary>
    private void OnEnable()
    {
        if (eventRunner == null)
            return;

        eventRunner.OnStatCheckRequested -= BeginStatCheck;
        eventRunner.OnStatCheckRequested += BeginStatCheck;

        eventRunner.OnChoiceSelected -= HandleChoiceResolved;
        eventRunner.OnChoiceSelected += HandleChoiceResolved;

        eventRunner.OnEventClosed -= HandleEventClosed;
        eventRunner.OnEventClosed += HandleEventClosed;
    }

    /// <summary>
    /// FieldEventRunner 이벤트를 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        if (eventRunner != null)
        {
            eventRunner.OnStatCheckRequested -= BeginStatCheck;
            eventRunner.OnChoiceSelected -= HandleChoiceResolved;
            eventRunner.OnEventClosed -= HandleEventClosed;
        }

        CancelSelection();
    }

    /// <summary>
    /// 능력치 판정 선택지가 선택되었을 때 판정 UI를 엽니다.
    /// </summary>
    private void BeginStatCheck(FieldEventChoice choice)
    {
        if (choice == null)
            return;

        pendingChoice = choice;

        if (guideText != null)
        {
            guideText.SetText(choice.Target.ToString());
        }

        SetCheckPanelActive(true);
    }

    /// <summary>
    /// 직접 판정 버튼 입력을 처리합니다.
    /// </summary>
    private void HandleDirectRoll()
    {
        if (eventRunner == null || !eventRunner.IsWaitingStatCheck)
            return;

        eventRunner.TryRollPendingStatCheck();
    }

    /// <summary>
    /// 드롭한 카드가 현재 판정에 대응하는지 확인하고 전달합니다.
    /// 선택 대기 중이라면 잘못된 카드도 일반 카드 사용으로 넘기지 않습니다.
    /// </summary>
    public bool TrySelectCard(CardData card)
    {
        if (!IsSelectingCard)
            return false;

        if (card == null)
            return true;

        if (!pendingChoice.CanUseCard(card))
        {
            if (guideText != null)
            {
                CardColorType requiredColor = GetRequiredColor(pendingChoice.RequiredStat);

                guideText.SetText($"{requiredColor} 카드가 필요합니다.");
            }

            return true;
        }

        OnCardSelected?.Invoke(pendingChoice, card);

        return true;
    }

    /// <summary>
    /// 선택지 판정이 완료되면 판정 UI를 닫습니다.
    /// </summary>
    private void HandleChoiceResolved(FieldEventData eventData, FieldEventChoice choice)
    {
        CancelSelection();
    }

    /// <summary>
    /// 이벤트가 종료되면 남아 있는 카드 선택 상태를 초기화합니다.
    /// </summary>
    private void HandleEventClosed()
    {
        CancelSelection();
    }

    /// <summary>
    /// 현재 판정 대기 상태를 취소합니다.
    /// </summary>
    public void CancelSelection()
    {
        pendingChoice = null;
        SetCheckPanelActive(false);
    }

    /// <summary>
    /// 판정 UI 활성 상태를 변경합니다.
    /// </summary>
    private void SetCheckPanelActive(bool active)
    {
        if (checkPanel != null)
        {
            checkPanel.SetActive(active);
        }
    }

    /// <summary>
    /// 능력치에 대응하는 카드 색상을 반환합니다.
    /// </summary>
    private CardColorType GetRequiredColor(StatType statType)
    {
        switch (statType)
        {
            case StatType.Strength:
                return CardColorType.Red;

            case StatType.Agility:
                return CardColorType.Yellow;

            case StatType.Health:
                return CardColorType.Green;

            case StatType.Intelligence:
                return CardColorType.Blue;

            case StatType.Will:
                return CardColorType.Purple;

            default:
                return CardColorType.None;
        }
    }
}