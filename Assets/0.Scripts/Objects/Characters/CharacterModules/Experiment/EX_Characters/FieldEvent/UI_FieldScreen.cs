using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>

/// UIManager가 생성하는 필드 화면의 최상위 스크립트
/// </summary>
public class UI_FieldScreen : UI_ScreenBase
{
    [Header("필드 매니저")]
    [SerializeField]
    private FieldManager fieldManager;

    [Header("손패")]
    [SerializeField]
    private UI_Hand handUI;

    [Header("현재 플레이어")]
    [SerializeField]
    private TextMeshProUGUI playerNameText;

    [Header("행동력")]
    [SerializeField]
    private TextMeshProUGUI actionPointText;

    [Header("턴")]
    [SerializeField]
    private TextMeshProUGUI fieldTurnText;

    [Header("필드 카드 사용 공간")]
    [SerializeField]
    private UI_FieldCardUseDropTarget fieldCardUseArea;

    [SerializeField]
    private TextMeshProUGUI mythTurnText;

    private CharacterBase boundPlayer;

    private ActionPointModule boundActionPoint;

    public event Action<CardInstance, CharacterBase> OnFieldCardSelected;

    private void OnEnable()
    {
        RegisterFieldManager();

        if (fieldManager != null && fieldManager.CurrentPlayer != null)
        {
            BindPlayer(fieldManager.CurrentPlayer);

            RefreshTurnTexts(fieldManager.TotalFieldTurn);
        }
    }

    private void OnDisable()
    {
        UnregisterFieldManager();
        UnbindPlayer();
    }

    private void RegisterFieldManager()
    {
        if (fieldManager == null)
            return;

        fieldManager.OnFieldTurnStarted -= HandleFieldTurnStarted;

        fieldManager.OnFieldTurnStarted += HandleFieldTurnStarted;
    }

    private void UnregisterFieldManager()
    {
        if (fieldManager == null)
            return;

        fieldManager.OnFieldTurnStarted -= HandleFieldTurnStarted;
    }

    private void HandleFieldTurnStarted(CharacterBase player, int completedTurnCount)
    {
        BindPlayer(player);

        RefreshTurnTexts(completedTurnCount);

        if (fieldCardUseArea != null)
        {
            fieldCardUseArea.ResetDisplay();
        }
    }

    private void BindPlayer(CharacterBase player)
    {
        UnbindPlayer();

        boundPlayer = player;

        if (boundPlayer == null)
        {
            ClearScreen();
            return;
        }

        if (playerNameText != null)
        {
            playerNameText.SetText(boundPlayer.DisplayName);
        }

        DeckModule deck = boundPlayer.GetModule<DeckModule>();

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

        boundActionPoint = boundPlayer.GetModule<ActionPointModule>();

        if (boundActionPoint == null)
        {
            RefreshActionPoint(0, 0);
            return;
        }

        boundActionPoint.OnActionPointChanged -= RefreshActionPoint;

        boundActionPoint.OnActionPointChanged += RefreshActionPoint;

        RefreshActionPoint(boundActionPoint.Current, boundActionPoint.Max);
    }

    private void UnbindPlayer()
    {
        if (boundActionPoint != null)
        {
            boundActionPoint.OnActionPointChanged -= RefreshActionPoint;
        }

        boundActionPoint = null;
        boundPlayer = null;
    }

    private void RefreshActionPoint(int current, int maximum)
    {
        if (actionPointText != null)
        {
            actionPointText.SetText($"행동력 {current}/{maximum}");
        }
    }

    private void RefreshTurnTexts(int completedTurnCount)
    {
        // totalFieldTurn은 끝난 턴의 개수이므로
        // 현재 진행 중인 턴은 +1
        int currentTurn = completedTurnCount + 1;

        if (fieldTurnText != null)
        {
            fieldTurnText.SetText($"필드 턴 {currentTurn}");
        }

        if (mythTurnText == null || fieldManager == null)
        {
            return;
        }

        int interval = Mathf.Max(1, fieldManager.MythTurnInterval);

        int remaining = interval - completedTurnCount % interval;

        mythTurnText.SetText($"신화턴까지 {remaining}턴");
    }

    private void ClearScreen()
    {
        if (playerNameText != null)
        {
            playerNameText.SetText(string.Empty);
        }

        RefreshActionPoint(0, 0);

        if (handUI != null)
        {
            handUI.ClearHand();
        }
    }

    /// <summary>
    /// 필드 카드 사용 영역에 놓인 손패 카드를
    /// FieldCardUseController에 전달합니다.
    /// </summary>
    public bool TryUseDroppedCard(CardInstance selectedCard)
    {
        if (selectedCard == null || selectedCard.Data == null)
            return false;

        if (fieldManager == null || !fieldManager.IsFieldActive)
        {
            return false;
        }

        if (fieldManager.TurnState != FieldTurnState.PlayerAction)
        {
            Debug.Log("현재는 필드 카드를 사용할 수 없습니다.");

            return false;
        }

        if (boundPlayer == null || boundPlayer != fieldManager.CurrentPlayer)
        {
            return false;
        }

        DeckModule deck = boundPlayer.GetModule<DeckModule>();

        if (deck == null ||
            !ContainsCard(deck, selectedCard))
        {
            Debug.LogWarning($"{selectedCard.CardName}: 손패에 없는 카드입니다.");

            return false;
        }

        OnFieldCardSelected?.Invoke(selectedCard, boundPlayer);

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

    private void HandleFieldCardResolved(CharacterBase user)
    {
        if (user == null || user != boundPlayer)
        {
            return;
        }

        DeckModule deck = user.GetModule<DeckModule>();

        if (handUI == null)
            return;

        if (deck != null)
        {
            handUI.RefreshFromDeck(deck);
        }
        else
        {
            handUI.ClearHand();
        }
    }

}
