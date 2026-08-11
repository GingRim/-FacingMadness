using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 실행기
/// </summary>
public class FieldEventRunner : MonoBehaviour
{
    private readonly HashSet<string> completedEvents = new();

    private FieldEventData currentEvent;
    private FieldEventContext currentContext;

    public FieldEventData CurrentEvent => currentEvent;
    public FieldEventContext CurrentContext => currentContext;
    public bool IsEventActive => currentEvent != null;

    private bool isChoiceResolved;

    public bool IsChoiceResolved => isChoiceResolved;

    public event Action<FieldEventData, FieldEventContext> OnEventOpened;

    public event Action<FieldEventData, FieldEventChoice> OnChoiceSelected;

    public event Action<string> OnChoiceFailed;
    public event Action OnEventClosed;

    public bool OpenEvent(FieldEventData eventData, FieldEventContext context)
    {
        if (eventData == null || context == null)
            return false;

        if (!eventData.Repeatable && completedEvents.Contains(eventData.EventId))
        {
            return false;
        }

        currentEvent = eventData;
        currentContext = context;
        isChoiceResolved = false;


        OnEventOpened?.Invoke(currentEvent, currentContext);

        return true;
    }

    public bool SelectChoice(int choiceIndex)
    {
        if (currentEvent == null || currentContext == null || isChoiceResolved)
        {
            return false;
        }

        FieldEventChoice[] choices = currentEvent.Choices;

        if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
        {
            return false;
        }

        FieldEventChoice choice = choices[choiceIndex];

        if (choice == null)
            return false;

        if (!choice.CanSelect(currentContext))
        {
            string failMessage = choice.GetFailMessage(currentContext);

            OnChoiceFailed?.Invoke(failMessage);
            return false;
        }

        choice.Execute(currentContext);

        if (!currentEvent.Repeatable && !string.IsNullOrEmpty(currentEvent.EventId))
        {
            completedEvents.Add(currentEvent.EventId);
        }

        OnChoiceSelected?.Invoke(currentEvent, choice);

        return true;
    }

    public void CompleteCurrentEvent()
    {
        if (currentEvent == null)
            return;

        if (!isChoiceResolved)
            return;

        CloseEvent();
    }

    public void CloseEvent()
    {
        currentEvent = null;
        currentContext = null;
        isChoiceResolved = false;

        OnEventClosed?.Invoke();
    }


    public void ResetCompletedEvents()
    {
        completedEvents.Clear();
    }

    public void SetSelectedCard(CardData card)
    {
        if (currentContext == null)
            return;

        currentContext.SetSelectedCard(card);
    }

    public void ClearSelectedCard()
    {
        if (currentContext == null)
            return;

        currentContext.ClearSelectedCard();
    }

}