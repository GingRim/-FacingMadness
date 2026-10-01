using System;
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

    private readonly List<CardInstance> removedCardRecoveryCandidates = new();
    private readonly List<string> leadingResultMessages = new();
    private readonly List<string> resultMessages = new();
    private readonly List<string> postResultPopupMessages = new();
    private readonly List<Action<FieldEventContext>> afterResultActions = new();
    private readonly List<Action> eventClosedActions = new();

    public IReadOnlyList<CardInstance> RemovedCardRecoveryCandidates => removedCardRecoveryCandidates;


    /// <summary>
    /// 현재 이벤트에서 실제로 적용된 효과 결과 목록이다.
    /// </summary>
    public IReadOnlyList<string> ResultMessages => resultMessages;

    public IReadOnlyList<string> LeadingResultMessages =>
        leadingResultMessages;

    /// <summary>
    /// 표시할 실제 효과 결과가 존재하는지 반환한다.
    /// </summary>
    public bool HasResultMessages =>
        leadingResultMessages.Count > 0 ||
        resultMessages.Count > 0;

    public bool HasPostResultPopupMessages =>
        postResultPopupMessages.Count > 0;

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

    public void RequestRemovedCardRecovery(IEnumerable<CardInstance> cards)
    {
        removedCardRecoveryCandidates.Clear();

        if (cards == null)
            return;

        foreach (CardInstance card in cards)
        {
            if (card == null || card.Data == null)
                continue;

            // 무색 카드는 복귀 선택지에 표시하지 않는다.
            if (card.Data.color == CardColorType.Colorless)
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
    /// 선택지의 일반 결과 설명보다 먼저 표시할 효과 문장을 추가한다.
    /// 정보 획득 안내처럼 우선 공개해야 하는 결과에 사용한다.
    /// </summary>
    public void AddLeadingResultMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        leadingResultMessages.Add(message);
    }

    /// <summary>
    /// 현재 이벤트에서 기록한 효과 결과를 모두 제거한다.
    /// </summary>
    public void ClearResultMessages()
    {
        leadingResultMessages.Clear();
        resultMessages.Clear();
    }

    /// <summary>
    /// 이벤트 결과창을 닫은 뒤 팝업으로 알릴 실제 적용 결과를 추가합니다.
    /// 전투 로그와는 별도로 필드 이벤트 효과만 기록합니다.
    /// </summary>
    public void AddPostResultPopupMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        postResultPopupMessages.Add(message.Trim());
    }

    public string BuildPostResultPopupText()
    {
        if (postResultPopupMessages.Count == 0)
            return string.Empty;

        return string.Join("\n", postResultPopupMessages);
    }

    public void ClearPostResultPopupMessages()
    {
        postResultPopupMessages.Clear();
    }

    /// <summary>
    /// 이벤트 시작 전 이전 결과 문장과 효과 기록을 초기화한다.
    /// 카드 판정 정보와 선택된 카드는 제거하지 않는다.
    /// </summary>
    public void ClearEventResult()
    {
        ClearResultText();
        ClearResultMessages();
        ClearPostResultPopupMessages();
        afterResultActions.Clear();
        eventClosedActions.Clear();
    }

    /// <summary>
    /// 현재 선택지에 연결된 모든 효과가 실행된 뒤 처리할 동작을 예약한다.
    /// 피해와 회복을 전부 적용한 뒤 생존 여부를 확인할 때 사용한다.
    /// </summary>
    public void AddAfterResultAction(Action<FieldEventContext> action)
    {
        if (action != null)
            afterResultActions.Add(action);
    }

    /// <summary>
    /// 선택지 결과 효과가 끝난 시점의 예약 동작을 한 번씩 실행한다.
    /// 실행 전에 목록을 비워 재실행과 중복 진행을 막는다.
    /// </summary>
    public void ExecuteAfterResultActions()
    {
        if (afterResultActions.Count == 0)
            return;

        Action<FieldEventContext>[] actions = afterResultActions.ToArray();
        afterResultActions.Clear();

        foreach (Action<FieldEventContext> action in actions)
        {
            action?.Invoke(this);
        }
    }

    /// <summary>
    /// 결과 확인으로 현재 이벤트가 완전히 닫힌 뒤 실행할 동작을 예약한다.
    /// 전투는 이 시점에 시작해야 복귀 후 이전 이벤트 UI가 다시 열리지 않는다.
    /// </summary>
    public void AddEventClosedAction(Action action)
    {
        if (action != null)
            eventClosedActions.Add(action);
    }

    /// <summary>
    /// 예약된 이벤트 종료 동작을 한 번씩 실행한다.
    /// </summary>
    public void ExecuteEventClosedActions()
    {
        if (eventClosedActions.Count == 0)
            return;

        Action[] actions = eventClosedActions.ToArray();
        eventClosedActions.Clear();

        foreach (Action action in actions)
        {
            action?.Invoke();
        }
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

        AppendResultMessages(builder, leadingResultMessages);

        if (!string.IsNullOrWhiteSpace(narrativeText))
        {
            AppendParagraphGap(builder);
            builder.Append(narrativeText.Trim());
        }

        if (resultMessages.Count > 0)
        {
            AppendParagraphGap(builder);
            AppendResultMessages(builder, resultMessages);
        }

        return builder.ToString();
    }

    private static void AppendParagraphGap(StringBuilder builder)
    {
        if (builder.Length <= 0)
            return;

        builder.AppendLine();
        builder.AppendLine();
    }

    private static void AppendResultMessages(
        StringBuilder builder,
        IReadOnlyList<string> messages)
    {
        if (builder == null || messages == null)
            return;

        for (int i = 0; i < messages.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(messages[i]))
                continue;

            builder.Append("• ");
            builder.Append(messages[i].Trim());

            if (i < messages.Count - 1)
                builder.AppendLine();
        }
    }

}
