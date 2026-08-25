using UnityEngine;
using System.Collections.Generic;
using System.Text;

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
    private readonly List<string> resultMessages = new();

    public IReadOnlyList<CardData> RemovedCardRecoveryCandidates => removedCardRecoveryCandidates;


    /// <summary>
    /// 현재 이벤트에서 실제로 적용된 효과 결과 목록이다.
    /// </summary>
    public IReadOnlyList<string> ResultMessages => resultMessages;

    /// <summary>
    /// 표시할 실제 효과 결과가 존재하는지 반환한다.
    /// </summary>
    public bool HasResultMessages => resultMessages.Count > 0;

    public bool HasRemovedCardRecoveryRequest => removedCardRecoveryCandidates.Count > 0;


    public bool HasResultTextOverride => !string.IsNullOrEmpty(ResultTextOverride);

    /// <summary>
    /// 이벤트 효과를 적용받는 캐릭터를 반환한다.
    /// 직접 지정된 플레이어가 없으면 현재 필드 플레이어를 사용한다.
    /// </summary>
    public CharacterBase Character => Player != null ? Player : FieldManager != null ? FieldManager.CurrentPlayer : null;

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

    /// <summary>
    /// 이벤트 효과가 실제로 적용한 결과 문장을 추가한다.
    /// </summary>
    /// <param name="message">표시할 결과 문장</param>
    public void AddResultMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        resultMessages.Add(message);
    }

    /// <summary>
    /// 현재 이벤트에서 기록한 효과 결과를 모두 제거한다.
    /// </summary>
    public void ClearResultMessages()
    {
        resultMessages.Clear();
    }

    /// <summary>
    /// 이벤트 시작 전 이전 결과 문장과 효과 기록을 초기화한다.
    /// 카드 판정 정보와 선택된 카드는 제거하지 않는다.
    /// </summary>
    public void ClearEventResult()
    {
        ClearResultText();
        ClearResultMessages();
    }

    /// <summary>
    /// 선택지의 서술 결과와 실제 효과 결과를 합쳐
    /// UI에 표시할 최종 문장을 생성한다.
    /// </summary>
    /// <param name="defaultResultText">
    /// 선택지에 직접 작성된 기본 결과 문장
    /// </param>
    /// <returns>UI에 표시할 최종 결과 문장</returns>
    public string BuildResultText(string defaultResultText)
    {
        string narrativeText =
            HasResultTextOverride
                ? ResultTextOverride
                : defaultResultText;

        StringBuilder builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(narrativeText))
        {
            builder.Append(narrativeText.Trim());
        }

        if (resultMessages.Count > 0)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
            }

            for (int i = 0; i < resultMessages.Count; i++)
            {
                builder.Append("• ");
                builder.Append(resultMessages[i]);

                if (i < resultMessages.Count - 1)
                {
                    builder.AppendLine();
                }
            }
        }

        return builder.ToString();
    }

}
