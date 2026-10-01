using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_FieldEventCandidateSelect : MonoBehaviour
{
    [Header("화면")]
    [SerializeField]
    private GameObject panel;

    [Header("이벤트 후보")]
    [SerializeField]
    private Transform core;

    [SerializeField]
    private UI_FieldEventCandidateButton buttonTemplate;

    private readonly List<UI_FieldEventCandidateButton> buttonPool = new();

    private readonly List<FieldEventData> displayedEvents = new();

    private bool isOpen;

    public bool IsOpen => isOpen;

    public event Action<FieldEventData> OnEventSelected;

    private void Awake()
    {
        if (buttonTemplate != null)
        {
            buttonTemplate.gameObject.SetActive(false);
        }

        Close();
    }

    public bool Open(IReadOnlyList<FieldEventData> candidates)
    {
        CollectEvents(candidates);

        if (displayedEvents.Count == 0)
        {
            Debug.LogWarning("UI_FieldEventCandidateSelect: 표시할 이벤트 후보가 없습니다.");

            Close();
            return false;
        }

        EnsurePoolSize(displayedEvents.Count);

        if (buttonPool.Count < displayedEvents.Count)
        {
            Debug.LogWarning("UI_FieldEventCandidateSelect: 이벤트 후보 버튼 생성에 실패했습니다.");

            Close();
            return false;
        }

        ClearButtons();

        for (int i = 0; i < displayedEvents.Count; i++)
        {
            buttonPool[i].SetEvent(displayedEvents[i], HandleEventSelected);
        }

        isOpen = true;

        if (panel != null)
        {
            panel.SetActive(true);
        }

        return true;
    }

    public void Close()
    {
        isOpen = false;

        ClearButtons();
        displayedEvents.Clear();

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void CollectEvents(IReadOnlyList<FieldEventData> candidates)
    {
        displayedEvents.Clear();

        if (candidates == null)
            return;

        foreach (FieldEventData eventData in candidates)
        {
            if (eventData == null)
                continue;

            if (displayedEvents.Contains(eventData))
                continue;

            displayedEvents.Add(eventData);
        }
    }

    private void EnsurePoolSize(int requiredCount)
    {
        if (core == null || buttonTemplate == null)
        {
            Debug.LogWarning("UI_FieldEventCandidateSelect: Core 또는 Button Template이 없습니다.");

            return;
        }

        while (buttonPool.Count < requiredCount)
        {
            UI_FieldEventCandidateButton newButton = Instantiate(buttonTemplate, core);

            newButton.name = $"FieldEventCandidate_{buttonPool.Count}";

            newButton.Clear();

            buttonPool.Add(newButton);
        }
    }

    private void ClearButtons()
    {
        foreach (UI_FieldEventCandidateButton button in buttonPool)
        {
            if (button == null)
                continue;

            button.Clear();
        }
    }

    private void HandleEventSelected(FieldEventData selectedEvent)
    {
        if (!isOpen || selectedEvent == null)
            return;

        // 연속 클릭 방지
        isOpen = false;

        ClearButtons();
        displayedEvents.Clear();

        if (panel != null)
        {
            panel.SetActive(false);
        }

        OnEventSelected?.Invoke(selectedEvent);
    }
}
