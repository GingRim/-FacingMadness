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

    private FieldEventData currentEvent;

    private FieldEventContext currentContext;

    private FieldEventPageData currentPage;

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

        OnEventOpened?.Invoke(
            currentEvent,
            currentContext);

        if (currentPage != null)
        {
            OnPageChanged?.Invoke(currentPage);
        }

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
    /// 현재 페이지에서 지정된 번호의 선택지를 실행한다.
    /// 페이지 이동 선택지와 실제 효과 선택지를 구분해 처리한다.
    /// </summary>
    /// <param name="choiceIndex">원본 선택지 배열의 번호</param>
    /// <returns>선택지가 정상적으로 처리되었으면 true</returns>
    public bool SelectChoice(int choiceIndex)
    {
        if (currentEvent == null ||
            currentContext == null ||
            isChoiceResolved)
        {
            return false;
        }

        FieldEventChoice[] choices = GetCurrentChoices();

        if (choices == null ||
            choiceIndex < 0 ||
            choiceIndex >= choices.Length)
        {
            return false;
        }

        FieldEventChoice choice = choices[choiceIndex];

        if (choice == null)
            return false;

        if (!IsChoiceAvailable(choice))
        {
            OnChoiceFailed?.Invoke(
                "이미 사용한 선택지입니다.");

            return false;
        }

        if (!choice.CanSelect(currentContext))
        {
            string failMessage =
                choice.GetFailMessage(currentContext);

            OnChoiceFailed?.Invoke(failMessage);

            return false;
        }

        if (choice.IsNavigation)
        {
            return TryOpenNextPage(choice);
        }

        return ResolveChoice(choice);
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
    /// 실제 행동 선택지의 효과를 적용하고 결과 표시 상태로 전환한다.
    /// </summary>
    /// <param name="choice">실행할 행동 선택지</param>
    /// <returns>선택 결과가 정상적으로 처리되었으면 true</returns>
    private bool ResolveChoice(FieldEventChoice choice)
    {
        if (choice == null || currentContext == null)
            return false;

        choice.Execute(currentContext);

        RegisterChoiceUse(choice);

        isChoiceResolved = true;

        if (!currentEvent.Repeatable &&
            !string.IsNullOrWhiteSpace(currentEvent.EventId))
        {
            completedEvents.Add(currentEvent.EventId);
        }

        OnChoiceSelected?.Invoke(
            currentEvent,
            choice);

        return true;
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
}