using Unity.Android.Gradle;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 이벤트 실행 정보
/// </summary>
public class FieldEventContext
{
    public CharacterBase Player { get; }
    public FieldNode Node { get; }
    public FieldManager FieldManager { get; }

    public string ResultTextOverride { get; private set; }

    private readonly List<CardData> removedCardRecoveryCandidates = new();

    public IReadOnlyList<CardData> RemovedCardRecoveryCandidates => removedCardRecoveryCandidates;

    public bool HasRemovedCardRecoveryRequest => removedCardRecoveryCandidates.Count > 0;


    public bool HasResultTextOverride => !string.IsNullOrEmpty(ResultTextOverride);

    public CharacterBase Character => FieldManager != null ? FieldManager.CurrentPlayer : null;

    public void SetResultText(string resultText)
    {
        ResultTextOverride = resultText;
    }

    public void ClearResultText()
    {
        ResultTextOverride = string.Empty;
    }

    public FieldEventContext(CharacterBase player, FieldNode node, FieldManager fieldManager)
    {
        Player = player;
        Node = node;
        FieldManager = fieldManager;
    }

    public CardData SelectedCard { get; private set; }

    public void SetSelectedCard(CardData card)
    {
        SelectedCard = card;
    }

    public void ClearSelectedCard()
    {
        SelectedCard = null;
        ClearCardCheck();
        ClearRemovedCardRecoveryRequest();
    }


    public FieldCardCheckData CardCheck { get; private set; }

    public bool HasCardCheck { get; private set; }

    public void SetCardCheck(FieldCardCheckData checkData)
    {
        CardCheck = checkData;
        HasCardCheck = true;
    }

    public void ClearCardCheck()
    {
        CardCheck = default;
        HasCardCheck = false;
    }

    public Inventory Inventory
    {
        get
        {
            if (Character == null)
                return null;

            return Character.GetComponentInChildren<Inventory>(true);
        }
    }

    public void RequestRemovedCardRecovery(IEnumerable<CardData> cards)
    {
        removedCardRecoveryCandidates.Clear();

        if (cards == null)
            return;

        foreach (CardData card in cards)
        {
            if (card == null)
                continue;

            // 무색 카드는 복귀 선택지에 표시하지 않는다.
            if (card.color == CardColorType.Colorless)
                continue;

            removedCardRecoveryCandidates.Add(card);
        }
    }

    public void ClearRemovedCardRecoveryRequest()
    {
        removedCardRecoveryCandidates.Clear();
    }

    public FieldEventContext(FieldManager fieldManager, FieldNode node)
    {
        FieldManager = fieldManager;
        Node = node;
    }

}
