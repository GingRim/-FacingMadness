using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 필드 이벤트 실행, 다음 이벤트 이동,
/// 선택 결과 처리 및 1회용 선택지 기록을 담당한다.
/// </summary>
public class FieldEventRunner : MonoBehaviour
{
    private readonly HashSet<FieldEventData> completedEvents = new();

    private readonly HashSet<string> usedChoices = new();

    private readonly Stack<FieldEventData> eventHistory = new();

    [Header("점화 판정")]
    [SerializeField]
    private CardIgnitionController ignitionController;

    private CardInstance pendingIgnitionCard;


    private FieldEventData currentEvent;
    private FieldEventContext currentContext;
    private FieldEventData currentData;

    private JudgeResult lastJudgeResult;
    private bool hasLastJudgeResult;

    private bool lastChoiceSucceeded;
    private CardData lastUsedJudgeCard;

    public JudgeResult LastJudgeResult => lastJudgeResult;

    public bool HasLastJudgeResult => hasLastJudgeResult;

    public bool LastChoiceSucceeded => lastChoiceSucceeded;

    public CardData LastUsedJudgeCard => lastUsedJudgeCard;

    private bool isChoiceResolved;

    /// <summary>
    /// 현재 실행 중인 이벤트다.
    /// </summary>
    public FieldEventData CurrentEvent => currentEvent;

    /// <summary>
    /// 현재 이벤트의 실행 정보다.
    /// </summary>
    public FieldEventContext CurrentContext => currentContext;

    /// <summary>
    /// 현재 화면에 표시 중인 통합 이벤트 데이터다.
    /// </summary>
    public FieldEventData CurrentData => currentData;

    private FieldEventChoice pendingStatChoice;

    // 메모·기억 선택 페이지를 연 원래 행동 선택지입니다.
    // 정보가 실제로 선택될 때까지 선택지 사용과 이벤트 완료를 보류합니다.
    private FieldEventChoice pendingInformationChoice;

    /// <summary>
    /// 현재 능력치 판정 방법 선택을 기다리는 선택지.
    /// </summary>
    public FieldEventChoice PendingStatChoice => pendingStatChoice;

    /// <summary>
    /// 현재 능력치 판정 방법 선택을 기다리고 있는지 확인한다.
    /// </summary>
    public bool IsWaitingStatCheck => pendingStatChoice != null;

    /// <summary>
    /// 능력치 판정 선택지가 실행되어
    /// 직접 판정 또는 카드 사용을 선택해야 할 때 발생한다.
    /// </summary>
    public event Action<FieldEventChoice> OnStatCheckRequested;

    /// <summary>
    /// 현재 이벤트가 실행 중인지 반환한다.
    /// </summary>
    public bool IsEventActive => currentEvent != null;

    /// <summary>
    /// 실제 행동 선택지가 처리되었는지 반환한다.
    /// </summary>
    public bool IsChoiceResolved => isChoiceResolved;

    /// <summary>
    /// 이전 이벤트 화면으로 돌아갈 수 있는지 반환한다.
    /// </summary>
    public bool CanReturnToPreviousPage => eventHistory.Count > 0;

    /// <summary>
    /// 이벤트가 처음 열렸을 때 발생한다.
    /// </summary>
    public event Action<FieldEventData, FieldEventContext> OnEventOpened;

    /// <summary>
    /// 현재 표시할 통합 이벤트 데이터가 변경되었을 때 발생한다.
    /// </summary>
    public event Action<FieldEventData> OnEventDataChanged;

    /// <summary>
    /// 실제 행동 선택지가 처리되었을 때 발생한다.
    /// </summary>
    public event Action<FieldEventData, FieldEventChoice> OnChoiceSelected;

    /// <summary>
    /// 선택 조건을 만족하지 못했을 때 발생한다.
    /// </summary>
    public event Action<string> OnChoiceFailed;

    /// <summary>
    /// 현재 이벤트가 종료되었을 때 발생한다.
    /// </summary>
    public event Action OnEventClosed;

    /// <summary>
    /// 선택지 결과 이미지가 설정되어 있을 때 발생합니다.
    /// </summary>
    public event Action<Sprite> OnResultImageChanged;

    private void Awake()
    {
        if (ignitionController == null)
        {
            ignitionController = FindFirstObjectByType<CardIgnitionController>(FindObjectsInactive.Include);
        }
    }

    private void OnEnable()
    {
        BindIgnitionController();
    }

    private void OnDisable()
    {
        UnbindIgnitionController();

        ignitionController?.Cancel();
        pendingIgnitionCard = null;
    }

    private void BindIgnitionController()
    {
        if (ignitionController == null)
        {
            ignitionController = FindFirstObjectByType<CardIgnitionController>(
                FindObjectsInactive.Include);
        }

        if (ignitionController == null)
            return;

        ignitionController.OnIgnitionCheckRequested -= HandleIgnitionCheckRequested;
        ignitionController.OnIgnitionCheckRequested += HandleIgnitionCheckRequested;
    }

    private void UnbindIgnitionController()
    {
        if (ignitionController == null)
            return;

        ignitionController.OnIgnitionCheckRequested -= HandleIgnitionCheckRequested;
    }

    /// <summary>
    /// 지정된 통합 이벤트 데이터를 엽니다.
    /// </summary>
    /// <param name="eventData">실행할 이벤트 데이터</param>
    /// <param name="context">현재 이벤트 실행 정보</param>
    /// <returns>이벤트가 정상적으로 열렸으면 true</returns>
    public bool OpenEvent(FieldEventData eventData, FieldEventContext context)
    {
        return OpenEvent(
            eventData,
            context,
            false);
    }

    /// <summary>
    /// 기존 호출부와의 호환을 위해 세 번째 인자를 유지합니다.
    /// 이벤트의 Usage Type은 호출 위치와 관계없이 동일하게 적용합니다.
    /// </summary>
    public bool OpenEvent(
        FieldEventData eventData,
        FieldEventContext context,
        bool ignoreCompletionHistory)
    {
        if (eventData == null || context == null)
            return false;

        if (!eventData.CanOpen(context))
            return false;

        if (!eventData.HasPlayableContent)
        {
            Debug.LogWarning(
                $"{eventData.EventName}: 선택지가 없습니다.");

            return false;
        }

        if (!IsEventAvailable(eventData))
            return false;

        FieldEventData entryData = eventData.EntryData;

        if (!HasAvailableChoice(entryData, context))
            return false;

        _ = ignoreCompletionHistory;

        context.ClearEventResult();

        currentEvent = eventData;

        currentContext = context;

        currentData = entryData;

        isChoiceResolved = false;

        eventHistory.Clear();

        OnEventOpened?.Invoke(currentEvent, currentContext);

        pendingStatChoice = null;
        pendingInformationChoice = null;

        OnEventDataChanged?.Invoke(currentData);

        ResetLastChoiceResult();

        return true;
    }

    /// <summary>
    /// 현재 표시 중인 통합 이벤트 데이터에서 선택지를 가져옵니다.
    /// </summary>
    /// <returns>현재 선택 가능한 원본 선택지 목록</returns>
    public FieldEventChoice[] GetCurrentChoices()
    {
        if (currentData == null)
            return null;

        return currentData.Choices;
    }

    /// <summary>
    /// 해당 선택지가 현재 필드에서 아직 사용 가능한지 확인한다.
    /// </summary>
    /// <param name="choice">확인할 선택지</param>
    /// <returns>표시하거나 실행할 수 있으면 true</returns>
    public bool IsChoiceAvailable(FieldEventChoice choice)
    {
        if (choice == null)
            return false;

        if (!choice.IsOneTime)
            return true;

        if (string.IsNullOrWhiteSpace(choice.ChoiceId))
        {
            Debug.LogWarning(
                "1회용 선택지에 Choice Id가 설정되지 않았습니다.");

            return false;
        }

        return !usedChoices.Contains(choice.ChoiceId);
    }

    /// <summary>
    /// 현재 페이지에서 지정된 선택지를 선택한다.
    /// 페이지 이동 선택지는 다음 페이지를 열고,
    /// 일반 선택지는 즉시 실행하며,
    /// 능력치 선택지는 판정 방법 선택을 기다린다.
    /// </summary>
    /// <param name="choiceIndex">현재 페이지의 선택지 번호.</param>
    public void SelectChoice(int choiceIndex)
    {
        if (currentEvent == null || currentContext == null)
        {
            return;
        }

        if (isChoiceResolved || IsWaitingStatCheck)
        {
            return;
        }

        FieldEventChoice[] choices = GetCurrentChoices();

        if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
        {
            OnChoiceFailed?.Invoke(
                "선택지 번호가 올바르지 않습니다.");

            return;
        }

        FieldEventChoice choice = choices[choiceIndex];

        if (choice == null)
            return;

        if (!IsChoiceAvailable(choice))
        {
            OnChoiceFailed?.Invoke("이미 사용한 선택지입니다.");

            return;
        }

        if (!choice.CanSelect(currentContext))
        {
            OnChoiceFailed?.Invoke(choice.GetFailMessage(currentContext));

            return;
        }

        if (choice.IsNavigation)
        {
            TryOpenNextEvent(choice);
            return;
        }

        if (choice.RequiresStatCheck)
        {
            pendingStatChoice = choice;

            OnStatCheckRequested?.Invoke(choice);

            return;
        }

        ResolveChoiceResult(choice, true);
    }

    /// <summary>
    /// 이동 선택지가 지정한 다음 FieldEventData를 엽니다.
    /// 데이터 이동만으로는 이벤트 결과를 완료하지 않습니다.
    /// </summary>
    /// <param name="choice">페이지 이동 선택지</param>
    /// <returns>다음 이벤트 데이터를 열었으면 true</returns>
    private bool TryOpenNextEvent(FieldEventChoice choice)
    {
        if (choice == null)
            return false;

        FieldEventData nextEvent = choice.NextEvent;

        if (nextEvent == null)
        {
            Debug.LogWarning(
                $"다음 이벤트가 연결되지 않았습니다: " +
                $"{choice.ChoiceText}");

            return false;
        }

        if (!nextEvent.CanOpen(currentContext))
        {
            OnChoiceFailed?.Invoke(
                nextEvent.GetOpeningFailMessage(currentContext));

            return false;
        }

        FieldEventData nextData = nextEvent.EntryData;

        if (!HasAvailableChoice(nextData, currentContext))
        {
            OnChoiceFailed?.Invoke(
                "이 이벤트에는 현재 사용할 수 있는 선택지가 없습니다.");

            return false;
        }

        if (currentData != null)
            eventHistory.Push(currentData);

        if (nextData.ShowOwnedMemos)
        {
            pendingInformationChoice = choice;
        }
        else
        {
            RegisterChoiceUse(choice);
        }

        currentData = nextData;

        OnEventDataChanged?.Invoke(currentData);

        return true;
    }

    /// <summary>
    /// 이전에 열었던 통합 이벤트 데이터로 돌아갑니다.
    /// 행동력을 소모하거나 이벤트를 완료하지 않는다.
    /// </summary>
    /// <returns>이전 페이지로 돌아갔으면 true</returns>
    public bool TryReturnToPreviousPage()
    {
        if (currentEvent == null ||
            isChoiceResolved ||
            eventHistory.Count == 0)
        {
            return false;
        }

        if (currentData != null && currentData.ShowOwnedMemos)
        {
            pendingInformationChoice = null;
        }

        currentData = eventHistory.Pop();

        OnEventDataChanged?.Invoke(currentData);

        return true;
    }

    /// <summary>
    /// 정보 선택 페이지에서 고른 메모를 원래 행동 선택지의
    /// 결과로 확정합니다. 이 시점에 선택지가 완료되어 결과창으로 이어집니다.
    /// </summary>
    public bool ResolveInformationSelection(FieldInformationData information)
    {
        if (currentEvent == null ||
            currentContext == null ||
            currentData == null ||
            !currentData.ShowOwnedMemos ||
            pendingInformationChoice == null ||
            information == null ||
            !information.IsMemo)
        {
            return false;
        }

        FieldInformationInventory inventory =
            FieldInformationInventory.Get(currentContext.Character);

        if (inventory == null || !inventory.Contains(information))
            return false;

        string resultText = BuildInformationResultText(information);

        FieldEventChoice resolvedChoice = pendingInformationChoice;
        pendingInformationChoice = null;

        ResolveInformationChoiceResult(
            resolvedChoice,
            resultText);

        return true;
    }

    private static string BuildInformationResultText(
        FieldInformationData information)
    {
        if (information == null)
            return string.Empty;

        string title = information.Title != null
            ? information.Title.Trim()
            : string.Empty;

        string description = information.Description != null
            ? information.Description.Trim()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(title))
            return description;

        if (string.IsNullOrWhiteSpace(description))
            return title;

        return title + "\n\n" + description;
    }


    /// <summary>
    /// 정보 열람은 에셋에 임시로 연결된 성공 효과를 실행하지 않고,
    /// 선택한 정보의 내용만 결과로 확정합니다.
    /// </summary>
    private void ResolveInformationChoiceResult(
        FieldEventChoice choice,
        string resultText)
    {
        if (choice == null ||
            currentEvent == null ||
            currentContext == null)
        {
            return;
        }

        FieldEventData resolvedEvent = currentEvent;

        pendingStatChoice = null;
        isChoiceResolved = true;
        lastChoiceSucceeded = true;

        currentContext.SetResultText(resultText);

        Sprite resultImage = choice.GetResultImage(true);

        if (resultImage != null)
        {
            OnResultImageChanged?.Invoke(resultImage);
        }

        RegisterChoiceUse(choice);

        OnChoiceSelected?.Invoke(
            resolvedEvent,
            choice);
    }


    /// <summary>
    /// 직접 판정과 카드 자동 성공의 결과를 하나의 경로로 처리합니다.
    /// </summary>
    private void ResolveChoiceResult(FieldEventChoice choice, bool success)
    {
        if (choice == null ||
            currentEvent == null ||
            currentContext == null)
        {
            return;
        }

        FieldEventData resolvedEvent = currentEvent;

        pendingStatChoice = null;
        isChoiceResolved = true;
        lastChoiceSucceeded = success;

        if (success)
        {
            choice.ExecuteSuccess(currentContext);

            if (choice.ShowKnownMemoryTitlesOnSuccess)
            {
                AddKnownMemoryTitlesToResult();
            }
        }
        else
        {
            choice.ExecuteFailure(currentContext);

            if (choice.ShowKnownMemoryTitlesOnSuccess &&
                !currentContext.HasResultTextOverride)
            {
                currentContext.SetResultText(
                    "기억을 떠올리지 못했다.");
            }
        }

        Sprite resultImage =
            choice.GetResultImage(success);

        if (resultImage != null)
        {
            OnResultImageChanged?.Invoke(
                resultImage);
        }

        RegisterChoiceUse(choice);

        OnChoiceSelected?.Invoke(
            resolvedEvent,
            choice);
    }

    private void AddKnownMemoryTitlesToResult()
    {
        if (currentContext == null || currentContext.Character == null)
            return;

        FieldInformationInventory inventory =
            FieldInformationInventory.Get(currentContext.Character);

        if (inventory == null)
        {
            currentContext.AddResultMessage(
                "떠올릴 수 있는 기억이 없다.");
            return;
        }

        int memoryCount = 0;

        foreach (FieldInformationData information
                 in inventory.AcquiredInformation)
        {
            if (information == null || !information.IsMemory)
                continue;

            string title = string.IsNullOrWhiteSpace(information.Title)
                ? "이름 없는 기억"
                : information.Title.Trim();

            currentContext.AddResultMessage(title);
            memoryCount++;
        }

        if (memoryCount == 0)
        {
            currentContext.AddResultMessage(
                "떠올릴 수 있는 기억이 없다.");
        }
    }

    /// <summary>
    /// 1회용 선택지가 실제로 사용된 경우 식별자를 기록한다.
    /// 반복 가능한 선택지는 기록하지 않는다.
    /// </summary>
    /// <param name="choice">처리된 선택지</param>
    private void RegisterChoiceUse(FieldEventChoice choice)
    {
        if (choice == null || !choice.IsOneTime)
            return;

        if (string.IsNullOrWhiteSpace(choice.ChoiceId))
            return;

        usedChoices.Add(choice.ChoiceId);
    }

    /// <summary>
    /// 결과 확인이 끝난 이벤트를 종료한다.
    /// 실제 행동 선택지가 처리되지 않았다면 종료하지 않는다.
    /// </summary>
    public void CompleteCurrentEvent()
    {
        if (currentEvent == null)
            return;

        if (!isChoiceResolved)
            return;

        CloseEvent();
    }

    /// <summary>
    /// 실행 중인 이벤트와 데이터 이동 기록을 초기화합니다.
    /// 1회용 선택지 사용 기록은 유지한다.
    /// </summary>
    public void CloseEvent()
    {
        FieldEventContext closedContext = currentContext;

        if (currentEvent != null && isChoiceResolved)
        {
            RegisterEventCompletion(currentEvent);
        }

        currentEvent = null;

        currentContext = null;

        currentData = null;

        pendingInformationChoice = null;

        isChoiceResolved = false;

        eventHistory.Clear();

        OnEventClosed?.Invoke();

        ClearPendingStatCheck();

        // 필드 상태와 이벤트 UI가 모두 정리된 뒤 전투 같은 후속 동작을 시작한다.
        closedContext?.ExecuteEventClosedActions();

    }

    /// <summary>
    /// 필드가 새로 시작될 때 1회용 이벤트와 선택지 기록을 초기화합니다.
    /// </summary>
    public void ResetCompletedEvents()
    {
        completedEvents.Clear();
        usedChoices.Clear();
    }

    /// <summary>
    /// 현재 이벤트에서 사용할 카드를 등록한다.
    /// </summary>
    /// <param name="card">선택한 카드</param>
    public void SetSelectedCard(CardData card)
    {
        if (currentContext == null)
            return;

        currentContext.SetSelectedCard(card);
    }

    /// <summary>
    /// 현재 이벤트에 등록된 카드와 관련 판정 정보를 해제한다.
    /// </summary>
    public void ClearSelectedCard()
    {
        if (currentContext == null)
            return;

        currentContext.ClearSelectedCard();
    }

    /// <summary>
    /// 대기 중인 능력치 선택지를 카드 없이 직접 판정한다.
    /// 범용 판정을 사용하며 펌블은 실패 결과로 처리한다.
    /// </summary>
    /// <returns>정상적으로 판정을 실행했으면 true.</returns>
    public void TryRollPendingStatCheck()
    {
        if (pendingStatChoice == null || currentContext == null || isChoiceResolved)
        {
            return;
        }

        CharacterBase character = GetCurrentCheckCharacter();

        if (character == null)
        {
            OnChoiceFailed?.Invoke("판정을 진행할 캐릭터가 없습니다.");

            return;
        }

        FieldEventChoice choice = pendingStatChoice;

        JudgeResult judgeResult = JudgeUtility.Roll(character, choice.RequiredStat, choice.Target);

        lastJudgeResult = judgeResult;
        hasLastJudgeResult = true;
        lastUsedJudgeCard = null;

        ResolvePendingIgnition(judgeResult.success);
        ResolveChoiceResult(choice, judgeResult.success);

        Debug.Log(
            $"이벤트 직접 판정: " +
            $"D10 {judgeResult.dice} + " +
            $"능력 보정 {judgeResult.statModifier} + " +
            $"상태 보정 {judgeResult.statusModifier} " +
            $"결과 {judgeResult.total} / " +
            $"목표 {judgeResult.target}");
    }

    /// <summary>
    /// 대응 능력치 카드의 등급 보정을 더해 선택지 판정을 실행한다.
    /// </summary>
    /// <param name="usedCard">소비한 대응 색상 카드.</param>
    /// <returns>카드 보정이 적용된 최종 판정 결과.</returns>
    public FieldCardCheckResult CompletePendingStatCheckByCard(CardInstance usedCard)
    {
        if (pendingStatChoice == null || currentContext == null || isChoiceResolved)
        {
            return FieldCardCheckResult.Failure;
        }

        if (usedCard == null || usedCard.Data == null ||
            !pendingStatChoice.CanUseCard(usedCard))
        {
            OnChoiceFailed?.Invoke("이 판정에 대응하지 않는 카드입니다.");

            return FieldCardCheckResult.Failure;
        }

        FieldEventChoice choice = pendingStatChoice;

        CharacterBase character = GetCurrentCheckCharacter();

        if (character == null)
            return FieldCardCheckResult.Failure;

        JudgeResult judgeResult = JudgeUtility.Roll(
            character,
            choice.RequiredStat,
            choice.Target,
            usedCard.GradeBonus);

        lastJudgeResult = judgeResult;
        hasLastJudgeResult = true;
        lastUsedJudgeCard = usedCard.Data;

        ResolvePendingIgnition(judgeResult.success);

        ResolveChoiceResult(choice, judgeResult.success);

        if (judgeResult.success)
            return FieldCardCheckResult.Success;

        return judgeResult.fumble
            ? FieldCardCheckResult.Fumble
            : FieldCardCheckResult.Failure;
    }

    /// <summary>
    /// 현재 대기 중인 능력치 판정 선택지를 초기화한다.
    /// </summary>
    public void ClearPendingStatCheck()
    {
        pendingStatChoice = null;
        pendingIgnitionCard = null;

        ignitionController?.Cancel();
    }

    /// <summary>
    /// 최근 선택지 판정 정보를 초기화합니다.
    /// </summary>
    private void ResetLastChoiceResult()
    {
        lastJudgeResult = default;
        hasLastJudgeResult = false;

        lastChoiceSucceeded = false;
        lastUsedJudgeCard = null;
    }

    /// <summary>
    /// 점화 전달
    /// </summary>
    /// <param name="checkSucceeded"></param>
    private void ResolvePendingIgnition(bool checkSucceeded)
    {
        if (pendingIgnitionCard == null)
            return;

        if (ignitionController == null)
        {
            pendingIgnitionCard = null;
            return;
        }

        ignitionController.ResolveIgnition(checkSucceeded);

        pendingIgnitionCard = null;
    }

    private CharacterBase GetCurrentCheckCharacter()
    {
        if (currentContext == null)
            return null;

        if (currentContext.Player != null)
            return currentContext.Player;

        return currentContext.Character;
    }

    /// <summary>
    /// 현재 대기 중인 능력치 판정을 점화 판정으로 사용합니다.
    /// </summary>
    public bool BeginPendingIgnitionSelection()
    {
        if (pendingStatChoice == null || currentContext == null || isChoiceResolved)
        {
            return false;
        }

        BindIgnitionController();

        if (ignitionController == null)
        {
            OnChoiceFailed?.Invoke("점화 처리기를 찾지 못했습니다.");

            return false;
        }

        CharacterBase character = GetCurrentCheckCharacter();

        if (character == null)
        {
            OnChoiceFailed?.Invoke("점화를 진행할 캐릭터가 없습니다.");

            return false;
        }

        return ignitionController.BeginSelection(character);
    }

    private void HandleIgnitionCheckRequested(CardInstance card, CharacterBase character)
    {
        if (pendingStatChoice == null || currentContext == null || isChoiceResolved)
        {
            ignitionController?.Cancel();
            return;
        }

        CharacterBase checkCharacter = GetCurrentCheckCharacter();

        if (character != checkCharacter)
        {
            OnChoiceFailed?.Invoke(
                "현재 판정 캐릭터의 카드가 아닙니다.");

            ignitionController?.Cancel();
            return;
        }

        if (card == null || !card.CanIgnite)
        {
            OnChoiceFailed?.Invoke("점화할 수 없는 카드입니다.");

            ignitionController?.Cancel();
            return;
        }

        pendingIgnitionCard = card;

        Debug.Log($"점화 판정 대상 확정: {card.CardName}");

        OnStatCheckRequested?.Invoke(pendingStatChoice);
    }

    /// <summary>
    /// 통합 이벤트 데이터에 현재 사용할 수 있는 선택지가 있는지 확인합니다.
    /// </summary>
    public bool CanOpenEvent(FieldEventData eventData)
    {
        return CanOpenEvent(eventData, null, false);
    }

    public bool CanOpenEvent(
        FieldEventData eventData,
        FieldEventContext context)
    {
        return CanOpenEvent(eventData, context, false);
    }

    public bool CanOpenEvent(
        FieldEventData eventData,
        FieldEventContext context,
        bool ignoreCompletionHistory)
    {
        if (eventData == null ||
            !eventData.HasPlayableContent)
        {
            return false;
        }

        if (!IsEventAvailable(eventData))
            return false;

        if (context != null && !eventData.CanOpen(context))
        {
            return false;
        }

        _ = ignoreCompletionHistory;

        return HasAvailableChoice(
            eventData.EntryData,
            context);
    }

    /// <summary>
    /// 반복 이벤트는 항상 허용하고, 필드당 1회 이벤트는
    /// 현재 필드에서 해당 이벤트 에셋이 아직 완료되지 않았을 때만 허용합니다.
    /// </summary>
    private bool IsEventAvailable(FieldEventData eventData)
    {
        if (eventData == null)
            return false;

        if (!eventData.IsOneTime)
            return true;

        return !completedEvents.Contains(eventData);
    }

    private void RegisterEventCompletion(FieldEventData eventData)
    {
        if (eventData == null ||
            !eventData.IsOneTime)
        {
            return;
        }

        completedEvents.Add(eventData);
    }

    private bool HasAvailableChoice(
        FieldEventData eventData,
        FieldEventContext context)
    {
        if (eventData == null)
            return false;

        // 메모 열람 페이지는 메모가 없어도 열 수 있습니다.
        // UI에서 보유한 메모가 없다는 안내를 표시하고 이전 화면으로 돌아갑니다.
        if (eventData.ShowOwnedMemos)
            return true;

        if (eventData.Choices == null)
            return false;

        foreach (FieldEventChoice choice in eventData.Choices)
        {
            if (!IsChoiceAvailable(choice))
                continue;

            if (context != null && !choice.CanSelect(context))
                continue;

            return true;
        }

        return false;
    }

}
