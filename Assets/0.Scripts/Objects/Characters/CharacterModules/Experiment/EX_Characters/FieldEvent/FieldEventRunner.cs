using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 필드 이벤트 실행, 선택지 페이지 이동,
/// 선택 결과 처리 및 1회용 선택지 기록을 담당한다.
/// </summary>
public class FieldEventRunner : MonoBehaviour
{
    private readonly HashSet<string> completedEvents = new();

    private readonly HashSet<string> usedChoices = new();

    private readonly Stack<FieldEventPageData> pageHistory = new();

    [Header("점화 판정")]
    [SerializeField]
    private CardIgnitionController ignitionController;

    private CardInstance pendingIgnitionCard;


    private FieldEventData currentEvent;
    private FieldEventContext currentContext;
    private FieldEventPageData currentPage;

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
    /// 현재 화면에 표시 중인 선택지 페이지다.
    /// </summary>
    public FieldEventPageData CurrentPage => currentPage;

    private FieldEventChoice pendingStatChoice;

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
    /// 이전 페이지로 돌아갈 수 있는지 반환한다.
    /// </summary>
    public bool CanReturnToPreviousPage => pageHistory.Count > 0;

    /// <summary>
    /// 이벤트가 처음 열렸을 때 발생한다.
    /// </summary>
    public event Action<FieldEventData, FieldEventContext> OnEventOpened;

    /// <summary>
    /// 현재 선택지 페이지가 변경되었을 때 발생한다.
    /// </summary>
    public event Action<FieldEventPageData> OnPageChanged;

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
    /// 지정된 이벤트를 열고 시작 페이지를 준비한다.
    /// 시작 페이지가 없다면 기존 이벤트 선택지 배열을 사용한다.
    /// </summary>
    /// <param name="eventData">실행할 이벤트 데이터</param>
    /// <param name="context">현재 이벤트 실행 정보</param>
    /// <returns>이벤트가 정상적으로 열렸으면 true</returns>
    public bool OpenEvent(FieldEventData eventData, FieldEventContext context)
    {
        if (eventData == null || context == null)
            return false;

        if (eventData.RootPage == null)
        {
            Debug.LogWarning($"{eventData.EventName}: 시작 페이지가 연결되지 않았습니다.");

            return false;
        }

        if (!eventData.Repeatable && !string.IsNullOrWhiteSpace(eventData.EventId) && completedEvents.Contains(eventData.EventId))
        {
            return false;
        }

        context.ClearEventResult();

        currentEvent = eventData;

        currentContext = context;

        currentPage = eventData.RootPage;

        isChoiceResolved = false;

        pageHistory.Clear();

        OnEventOpened?.Invoke(currentEvent, currentContext);

        pendingStatChoice = null;

        if (currentPage != null)
        {
            OnPageChanged?.Invoke(currentPage);
        }

        ResetLastChoiceResult();

        return true;
    }

    /// <summary>
    /// 현재 페이지 또는 기존 이벤트 데이터에서 선택지를 가져온다.
    /// </summary>
    /// <returns>현재 선택 가능한 원본 선택지 목록</returns>
    public FieldEventChoice[] GetCurrentChoices()
    {
        if (currentPage == null)
            return null;

        return currentPage.Choices;
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
        if (currentEvent == null || currentContext == null || currentPage == null)
        {
            return;
        }

        if (isChoiceResolved || IsWaitingStatCheck)
        {
            return;
        }

        FieldEventChoice[] choices = currentPage.Choices;

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
            TryOpenNextPage(choice);
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
    /// 페이지 이동 선택지가 지정한 하위 페이지를 연다.
    /// 페이지 이동만으로는 이벤트를 완료하지 않는다.
    /// </summary>
    /// <param name="choice">페이지 이동 선택지</param>
    /// <returns>하위 페이지를 열었으면 true</returns>
    private bool TryOpenNextPage(FieldEventChoice choice)
    {
        if (choice == null)
            return false;

        FieldEventPageData nextPage = choice.NextPage;

        if (nextPage == null)
        {
            Debug.LogWarning(
                $"하위 페이지가 연결되지 않았습니다: " +
                $"{choice.ChoiceText}");

            return false;
        }

        if (currentPage != null)
        {
            pageHistory.Push(currentPage);
        }

        RegisterChoiceUse(choice);

        currentPage = nextPage;

        OnPageChanged?.Invoke(currentPage);

        return true;
    }

    /// <summary>
    /// 이전에 열었던 선택지 페이지로 돌아간다.
    /// 행동력을 소모하거나 이벤트를 완료하지 않는다.
    /// </summary>
    /// <returns>이전 페이지로 돌아갔으면 true</returns>
    public bool TryReturnToPreviousPage()
    {
        if (currentEvent == null ||
            isChoiceResolved ||
            pageHistory.Count == 0)
        {
            return false;
        }

        currentPage = pageHistory.Pop();

        OnPageChanged?.Invoke(currentPage);

        return true;
    }


    /// <summary>
    /// 직접 판정과 카드 자동 성공의 결과를 하나의 경로로 처리합니다.
    /// </summary>
    private void ResolveChoiceResult(FieldEventChoice choice, bool success)
    {
        if (choice == null || currentEvent == null || currentContext == null)
            return;

        FieldEventData resolvedEvent = currentEvent;

        pendingStatChoice = null;
        isChoiceResolved = true;
        lastChoiceSucceeded = success;

        if (success)
        {
            choice.ExecuteSuccess(currentContext);
        }
        else
        {
            choice.ExecuteFailure(currentContext);
        }

        Sprite resultImage = choice.GetResultImage(success);

        if (resultImage != null)
        {
            OnResultImageChanged?.Invoke(resultImage);
        }

        RegisterChoiceUse(choice);
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
    /// 실행 중인 이벤트와 페이지 이동 기록을 초기화한다.
    /// 1회용 선택지 사용 기록은 유지한다.
    /// </summary>
    public void CloseEvent()
    {
        currentEvent = null;

        currentContext = null;

        currentPage = null;

        isChoiceResolved = false;

        pageHistory.Clear();

        OnEventClosed?.Invoke();

        ClearPendingStatCheck();

    }

    /// <summary>
    /// 필드가 새로 시작될 때 이벤트 완료 기록과
    /// 1회용 선택지 사용 기록을 초기화한다.
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
            $"= {judgeResult.total} / " +
            $"목표 {judgeResult.target}");
    }

    /// <summary>
    /// 대응 색상 카드의 소비가 끝난 능력치 선택지를
    /// 판정 없이 확정 성공으로 처리한다.
    /// </summary>
    /// <param name="usedCard">소비한 대응 색상 카드.</param>
    /// <returns>확정 성공 처리가 완료되면 true.</returns>
    public void CompletePendingStatCheckByCard(CardInstance usedCard)
    {
        if (pendingStatChoice == null || currentContext == null || isChoiceResolved)
        {
            return;
        }

        if (usedCard == null || usedCard.Data == null ||
            !pendingStatChoice.CanUseCard(usedCard.Data))
        {
            OnChoiceFailed?.Invoke("이 판정에 대응하지 않는 카드입니다.");

            return;
        }

        FieldEventChoice choice = pendingStatChoice;

        Debug.Log($"이벤트 카드 자동 성공: " + $"{usedCard.CardName} / " + $"{choice.RequiredStat} 판정");

        lastJudgeResult = default;
        hasLastJudgeResult = false;
        lastUsedJudgeCard = usedCard.Data;

        ResolvePendingIgnition(true);

        ResolveChoiceResult(choice, true);
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

}
