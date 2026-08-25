using System;
using System.Collections.Generic;
using UnityEngine;

public class MythTurnController : MonoBehaviour
{
    [Header("필드 매니저")]
    [SerializeField]
    private FieldManager fieldManager;

    [Header("공통 신화 이벤트")]
    [SerializeField]
    private List<MythEventData> mythEvents = new();

    [Header("신화 효과 실행기")]
    [SerializeField]
    private MythEffectResolver effectResolver;

    private readonly List<MythEventData> availableEvents = new();

    private MythEventData currentEvent;
    private MythTurnContext currentContext;

    public MythEventData CurrentEvent => currentEvent;
    public bool IsRunning => currentEvent != null;

    public event Action<MythEventData, MythTurnContext> OnMythEventStarted;

    public event Action<MythEventData, MythTurnContext> OnMythEventCompleted;

    private void OnEnable()
    {
        if (fieldManager == null)
            return;

        fieldManager.OnMythTurnRequested -= HandleMythTurnRequested;

        fieldManager.OnMythTurnRequested += HandleMythTurnRequested;
    }

    private void OnDisable()
    {
        if (fieldManager != null)
        {
            fieldManager.OnMythTurnRequested -= HandleMythTurnRequested;
        }

        currentEvent = null;
        currentContext = null;
    }


    private void CollectAvailableEvents()
    {
        availableEvents.Clear();

        foreach (MythEventData eventData in mythEvents)
        {
            if (eventData == null)
                continue;

            availableEvents.Add(eventData);
        }
    }

    private void HandleMythTurnRequested(int turnNumber)
    {
        if (fieldManager == null)
            return;

        if (IsRunning)
            return;

        CollectAvailableEvents();

        if (availableEvents.Count == 0)
        {
            Debug.LogWarning("MythTurnController: 실행 가능한 신화 이벤트가 없습니다.");

            fieldManager.CompleteMythTurn();
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, availableEvents.Count);

        currentEvent = availableEvents[randomIndex];

        MythTurnContext context = new MythTurnContext(fieldManager, turnNumber);

        Debug.Log($"신화 이벤트 발생: {currentEvent.EventName}");

        // 여기서는 효과를 바로 실행하지 않고
        // UI가 확인 버튼을 누르기를 기다림
        OnMythEventStarted?.Invoke(currentEvent, context);

        // 선택된 신화 이벤트의 실제 효과 실행
        if (effectResolver == null)
        {
            Debug.LogWarning("MythTurnController: MythEffectResolver가 연결되지 않았습니다.");
        }
        else
        {
            effectResolver.Execute(currentEvent.EventType, context);
        }

        MythEventData completedEvent = currentEvent;

        currentEvent = null;

        OnMythEventCompleted?.Invoke(completedEvent, context);

        fieldManager.CompleteMythTurn();
    }

    /// <summary>
    /// 신화 이벤트 UI의 확인 버튼에서 호출
    /// </summary>
    public void CompleteCurrentMythEvent()
    {
        if (currentEvent == null || currentContext == null)
        {
            return;
        }

        MythEventData completedEvent = currentEvent;

        MythTurnContext completedContext = currentContext;

        if (effectResolver != null)
        {
            effectResolver.Execute(completedEvent.EventType, completedContext);
        }
        else
        {
            Debug.LogWarning("MythTurnController: " + "MythEffectResolver가 없습니다.");
        }

        currentEvent = null;
        currentContext = null;

        OnMythEventCompleted?.Invoke(completedEvent, completedContext);

        // 신화 효과로 플레이어가 사망하면
        // FieldManager가 이미 GameOver 상태가 됨
        if (fieldManager == null || !fieldManager.IsFieldActive)
        {
            return;
        }

        if (fieldManager.TurnState != FieldTurnState.MythTurn)
        {
            return;
        }

        fieldManager.CompleteMythTurn();
    }


}
