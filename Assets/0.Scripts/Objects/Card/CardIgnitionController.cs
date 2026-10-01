using System;
using UnityEngine;

/// <summary>
/// 손패에서 비점화 카드를 선택하고
/// 점화 판정 결과를 CardInstance에 적용합니다.
/// </summary>
public class CardIgnitionController : MonoBehaviour
{
    [SerializeField]
    private UI_Hand handUI;

    private CardInstance pendingIgnitionCard;
    private CharacterBase owner;
    private DeckModule ownerDeck;

    public bool IsSelecting { get; private set; }

    public bool IsWaitingForResult => pendingIgnitionCard != null;

    public CardInstance PendingIgnitionCard => pendingIgnitionCard;

    /// <summary>
    /// 점화 대상이 선택되어 판정이 필요할 때 발생합니다.
    /// </summary>
    public event Action<CardInstance, CharacterBase> OnIgnitionCheckRequested;

    /// <summary>
    /// 점화 판정 처리가 끝났을 때 발생합니다.
    /// bool은 실제 점화 성공 여부입니다.
    /// </summary>
    public event Action<CardInstance, bool> OnIgnitionResolved;

    private void OnDisable()
    {
        Cancel();
    }

    /// <summary>
    /// 지정 캐릭터의 손패에서 비점화 카드 선택을 시작합니다.
    /// </summary>
    public bool BeginSelection(CharacterBase newOwner)
    {
        if (IsSelecting || IsWaitingForResult || newOwner == null)
        {
            return false;
        }

        DeckModule deck = newOwner.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{newOwner.name}: DeckModule이 없습니다.");

            return false;
        }

        if (!HasIgnitableCard(deck))
        {
            BattleManager.ClaimBattleLog("점화할 수 있는 비점화 카드가 없습니다.");

            return false;
        }

        if (handUI == null)
        {
            handUI = FindFirstObjectByType<UI_Hand>(FindObjectsInactive.Include);
        }

        if (handUI == null)
        {
            Debug.LogWarning("CardIgnitionController: UI_Hand를 찾지 못했습니다.");

            return false;
        }

        owner = newOwner;
        ownerDeck = deck;

        handUI.OnCardInstanceSelected -= HandleCardSelected;
        handUI.OnCardInstanceSelected += HandleCardSelected;

        IsSelecting = true;

        BattleManager.ClaimBattleLog("점화할 비점화 카드를 선택하세요.");

        return true;
    }

    /// <summary>
    /// UI_Hand에서 카드가 선택됐을 때 호출됩니다.
    /// </summary>
    private void HandleCardSelected(CardInstance selectedCard)
    {
        if (!IsSelecting)
            return;

        if (selectedCard == null ||
            selectedCard.Data == null)
        {
            return;
        }

        if (!IsCardInOwnerHand(selectedCard))
        {
            BattleManager.ClaimBattleLog("현재 손패에 없는 카드입니다.");

            return;
        }

        if (!selectedCard.CanIgnite)
        {
            BattleManager.ClaimBattleLog("비점화 카드만 점화할 수 있습니다.");

            return;
        }

        pendingIgnitionCard = selectedCard;

        StopSelection();

        Debug.Log($"점화 대상 선택: {selectedCard.CardName}");

        OnIgnitionCheckRequested?.Invoke(pendingIgnitionCard, owner);
    }

    /// <summary>
    /// 외부 판정 시스템에서 점화 판정 결과를 전달합니다.
    /// </summary>
    public bool ResolveIgnition(bool checkSucceeded)
    {
        if (pendingIgnitionCard == null)
            return false;

        CardInstance resolvedCard = pendingIgnitionCard;

        if (!IsCardInOwnerHand(resolvedCard))
        {
            Debug.LogWarning($"{resolvedCard.CardName}: " + "점화 판정 중 손패에서 이탈했습니다.");

            ClearPendingData();

            OnIgnitionResolved?.Invoke(resolvedCard, false);

            return false;
        }

        bool ignitionSucceeded = resolvedCard.ResolveIgnition(checkSucceeded);

        if (ignitionSucceeded)
        {
            BattleManager.ClaimBattleLog($"{resolvedCard.CardName}<br>점화 성공");
        }
        else
        {
            BattleManager.ClaimBattleLog($"{resolvedCard.CardName}<br>점화 실패");
        }

        ClearPendingData();

        OnIgnitionResolved?.Invoke(resolvedCard, ignitionSucceeded);

        return ignitionSucceeded;
    }

    /// <summary>
    /// 카드 선택 또는 판정 대기를 취소합니다.
    /// </summary>
    public void Cancel()
    {
        StopSelection();
        ClearPendingData();
    }

    private void StopSelection()
    {
        IsSelecting = false;

        if (handUI != null)
        {
            handUI.OnCardInstanceSelected -= HandleCardSelected;
        }
    }

    private void ClearPendingData()
    {
        pendingIgnitionCard = null;
        ownerDeck = null;
        owner = null;
    }

    private bool HasIgnitableCard(DeckModule deck)
    {
        if (deck == null)
            return false;

        foreach (CardInstance card in deck.HandInstances)
        {
            if (card != null && card.CanIgnite)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsCardInOwnerHand(CardInstance targetCard)
    {
        if (ownerDeck == null || targetCard == null)
        {
            return false;
        }

        foreach (CardInstance handCard in ownerDeck.HandInstances)
        {
            if (handCard == targetCard)
            {
                return true;
            }
        }

        return false;
    }
}