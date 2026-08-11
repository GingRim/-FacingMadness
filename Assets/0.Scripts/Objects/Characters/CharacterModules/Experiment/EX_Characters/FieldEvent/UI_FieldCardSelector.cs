using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 핸드 카드 클릭을 필드 선택으로 전환하는 중간 연결 스크립트
/// </summary>

public class UI_FieldCardSelector : MonoBehaviour
{
    
    [Header("연결")]
    [SerializeField]
    private UI_FieldEvent fieldEventUI;

    [Header("안내 UI")]
    [SerializeField]
    private GameObject guideObject;

    [SerializeField]
    private TextMeshProUGUI guideText;

    private FieldEventChoice pendingChoice;

    public bool IsSelectingCard =>
        pendingChoice != null;

    /// <summary>
    /// 카드 조건을 통과한 카드가 선택됐을 때 발생한다.
    /// 실제 판정과 카드 이동은 다음 단계에서 처리한다.
    /// </summary>
    public event Action<FieldEventChoice, CardData> OnCardSelected;

    private void Awake()
    {
        SetGuideActive(false);
    }

    private void OnEnable()
    {
        RegisterEvent();
    }

    private void OnDisable()
    {
        UnregisterEvent();
        CancelSelection();
    }

    private void RegisterEvent()
    {
        if (fieldEventUI == null)
            return;

        fieldEventUI.OnCardSelectionRequested -= BeginCardSelection;

        fieldEventUI.OnCardSelectionRequested += BeginCardSelection;
    }

    private void UnregisterEvent()
    {
        if (fieldEventUI == null)
            return;

        fieldEventUI.OnCardSelectionRequested -= BeginCardSelection;
    }

    private void BeginCardSelection(FieldEventChoice choice)
    {
        if (choice == null)
            return;

        pendingChoice = choice;

        if (guideText != null)
        {
            guideText.SetText("사용할 카드를 선택하세요.");
        }

        SetGuideActive(true);
    }

    /// <summary>
    /// 핸드의 카드 클릭 스크립트에서 먼저 호출한다.
    /// true면 현재 클릭을 필드 카드 선택이 처리한 것이다.
    /// </summary>
    public bool TrySelectCard(CardData card)
    {
        if (!IsSelectingCard)
            return false;

        // 선택 모드이므로 잘못된 카드도
        // 일반 카드 사용으로 전달하지 않는다.
        if (card == null)
            return true;

        if (!pendingChoice.CanUseCard(card))
        {
            if (guideText != null)
            {
                guideText.SetText("이 선택지에 사용할 수 없는 카드입니다.");
            }

            return true;
        }

        OnCardSelected?.Invoke(pendingChoice, card);

        return true;
    }

    /// <summary>
    /// 카드 판정과 카드 이동 처리가 끝난 뒤 호출한다.
    /// </summary>
    public bool CompleteSelection(CardData card)
    {
        if (!IsSelectingCard || fieldEventUI == null)
        {
            return false;
        }

        if (!pendingChoice.CanUseCard(card))
            return false;

        bool completed = fieldEventUI.SubmitSelectedCard(card);

        if (!completed)
            return false;

        pendingChoice = null;
        SetGuideActive(false);

        return true;
    }

    public void CancelSelection()
    {
        pendingChoice = null;

        if (fieldEventUI != null)
        {
            fieldEventUI.CancelCardSelection();
        }

        SetGuideActive(false);
    }

    private void SetGuideActive(bool active)
    {
        if (guideObject != null)
        {
            guideObject.SetActive(active);
        }
    }
}
