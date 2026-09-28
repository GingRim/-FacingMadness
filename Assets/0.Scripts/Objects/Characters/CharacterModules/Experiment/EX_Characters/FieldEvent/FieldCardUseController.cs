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

    private bool isProcessingCard;

    public event Action<CharacterBase> OnFieldCardResolved;

    private void Awake()
    {
        ResolveRuntimeReferences();
    }

    private void OnEnable()
    {
        ResolveRuntimeReferences();
        BindInputEvents();
    }

    private void Start()
    {
        // FieldManager는 GameManager 초기화 과정에서 추가되므로
        // 모든 Awake가 끝난 뒤 한 번 더 연결을 확인합니다.
        ResolveRuntimeReferences();
        BindInputEvents();
    }

    private void BindInputEvents()
    {
        // 이벤트 선택지에서 요구한 카드 선택
        if (cardSelector != null)
        {
            cardSelector.OnCardSelected -= HandleCardSelected;

            cardSelector.OnCardSelected += HandleCardSelected;
        }

    }

    private void ResolveRuntimeReferences()
    {
        if (fieldManager == null && GameManager.Instance != null)
        {
            fieldManager = GameManager.Instance.Field;
        }

        if (eventRunner == null)
        {
            eventRunner = GetComponent<FieldEventRunner>();

            if (eventRunner == null)
            {
                eventRunner = FindFirstObjectByType<FieldEventRunner>(
                    FindObjectsInactive.Include);
            }
        }

        if (cardSelector == null)
        {
            cardSelector = FindFirstObjectByType<UI_FieldCardSelector>(
                FindObjectsInactive.Include);
        }

        Canvas fieldCanvas = cardSelector != null
            ? cardSelector.GetComponentInParent<Canvas>(true)
            : null;

        if (handUI == null && fieldCanvas != null)
        {
            handUI = fieldCanvas.GetComponentInChildren<UI_Hand>(true);
        }

    }

    private void OnDisable()
    {
        if (cardSelector != null)
        {
            cardSelector.OnCardSelected -= HandleCardSelected;
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

        FieldCardCheckResult result = eventRunner.CompletePendingStatCheckByCard(card);

        bool removeCard = false;

        if (result != FieldCardCheckResult.Success)
        {
            if (card.CurrentGrade <= 1)
            {
                removeCard = true;
            }
            else
            {
                card.TryDecreaseGrade();
            }
        }

        deck.ResolveFieldCard(card, result, removeCard);

        if (handUI != null)
        {
            handUI.RefreshFromDeck(deck);
        }

        isProcessingCard = false;

        OnFieldCardResolved?.Invoke(user);
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

    /// <summary>
    /// 이전 드롭 UI 호환용입니다. 이벤트 판정용 카드 선택은 별도로 처리합니다.
    /// 직접 드롭은 행동력, 내구도, 카드 영역을 변경하지 않습니다.
    /// </summary>
    public bool TryUseDroppedCard(CardInstance card)
    {
        return false;
    }
}
