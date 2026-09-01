using System.Collections.Generic;
using UnityEngine;

public class FieldEventCandidateBuilder : MonoBehaviour
{
    [Header("이벤트 후보 개수")]
    [SerializeField, Range(3, 5)]
    private int minimumCount = 3;

    [SerializeField, Range(3, 5)]
    private int maximumCount = 5;

    private readonly List<FieldEventData> availableEvents = new();
    private readonly List<FieldEventData> candidates = new();

    public IReadOnlyList<FieldEventData> BuildCandidates(FieldManager fieldManager, CharacterBase character, IReadOnlyList<FieldEventData> source, FieldEventRunner eventRunner)
    {
        candidates.Clear();
        CollectAvailableEvents(source, eventRunner);

        if (availableEvents.Count == 0)
        {
            Debug.LogWarning(
                "FieldEventCandidateBuilder: " +
                "실행 가능한 이벤트가 없습니다.");

            return candidates;
        }

        int min = Mathf.Clamp(minimumCount, 3, 5);
        int max = Mathf.Clamp(maximumCount, min, 5);

        int requestedCount = Random.Range(min, max + 1);

        int candidateCount = Mathf.Min(requestedCount, availableEvents.Count);

        bool coreEventIncluded = false;

        if (fieldManager != null && fieldManager.HasCoreEventReservation(character))
        {
            FieldEventData coreEvent = TakeRandomCoreEvent();

            if (coreEvent != null)
            {
                candidates.Add(coreEvent);
                coreEventIncluded = true;
            }
            else
            {
                Debug.LogWarning(
                    "핵심 이벤트 예약이 있지만 " +
                    "실행 가능한 Core 이벤트가 없습니다.");
            }
        }

        Shuffle(availableEvents);

        foreach (FieldEventData eventData in availableEvents)
        {
            if (candidates.Count >= candidateCount)
                break;

            if (eventData == null)
                continue;

            candidates.Add(eventData);
        }

        Shuffle(candidates);

        // 핵심 이벤트가 실제 후보에 포함됐을 때만 예약 소비
        if (coreEventIncluded)
        {
            fieldManager.ConsumeCoreEventReservation(character);
        }

        if (candidates.Count < 3)
        {
            Debug.LogWarning(
                $"실행 가능한 이벤트 후보가 " +
                $"{candidates.Count}개뿐입니다. " +
                "MissionFieldRoot의 Event Pool을 확인하세요.");
        }

        return candidates;
    }

    private void CollectAvailableEvents(IReadOnlyList<FieldEventData> source, FieldEventRunner eventRunner)
    {
        availableEvents.Clear();

        if (source == null || eventRunner == null)
            return;

        foreach (FieldEventData eventData in source)
        {
            if (!eventRunner.CanOpenEvent(eventData))
                continue;

            if (availableEvents.Contains(eventData))
                continue;

            availableEvents.Add(eventData);
        }
    }

    private FieldEventData TakeRandomCoreEvent()
    {
        List<FieldEventData> coreEvents = new();

        foreach (FieldEventData eventData in availableEvents)
        {
            if (eventData == null)
                continue;

            if (eventData.EventType == FieldEventType.Core)
            {
                coreEvents.Add(eventData);
            }
        }

        if (coreEvents.Count == 0)
            return null;

        FieldEventData selected =
            coreEvents[Random.Range(0, coreEvents.Count)];

        availableEvents.Remove(selected);

        return selected;
    }

    private void Shuffle(List<FieldEventData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            (list[i], list[randomIndex]) =
                (list[randomIndex], list[i]);
        }
    }
}
