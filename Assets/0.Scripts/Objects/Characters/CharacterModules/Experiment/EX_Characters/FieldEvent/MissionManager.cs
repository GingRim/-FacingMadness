using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : ManagerBase
{
    [Header("미션 목록")]
    [SerializeField]
    private List<FieldMissionData> missionPool = new();

    [Header("제시할 미션 수")]
    [SerializeField, Min(1)]
    private int drawCount = 3;

    private readonly List<FieldMissionData> drawnMissions = new();

    private FieldMissionData selectedMission;

    public IReadOnlyList<FieldMissionData> DrawnMissions => drawnMissions;

    public FieldMissionData SelectedMission => selectedMission;

    public bool HasSelectedMission => selectedMission != null;

    public event Action<IReadOnlyList<FieldMissionData>> OnMissionsDrawn;

    public event Action<FieldMissionData> OnMissionSelected;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        yield return null;
    }

    protected override void OnDisconnected()
    {
        drawnMissions.Clear();
        selectedMission = null;
    }

    public void DrawMissions()
    {
        drawnMissions.Clear();
        selectedMission = null;

        List<FieldMissionData> availableMissions = CreateAvailableMissionList();

        if (availableMissions.Count == 0)
        {
            Debug.LogWarning("MissionManager: 뽑을 수 있는 미션이 없습니다.");

            OnMissionsDrawn?.Invoke(drawnMissions);
            return;
        }

        int actualDrawCount = Mathf.Min(drawCount, availableMissions.Count);

        for (int i = 0; i < actualDrawCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableMissions.Count);

            FieldMissionData mission = availableMissions[randomIndex];

            drawnMissions.Add(mission);
            availableMissions.RemoveAt(randomIndex);
        }

        OnMissionsDrawn?.Invoke(drawnMissions);
    }

    public bool SelectMission(FieldMissionData mission)
    {
        if (mission == null)
            return false;

        if (!drawnMissions.Contains(mission))
        {
            Debug.LogWarning(
                "MissionManager: 제시되지 않은 미션은 " +
                "선택할 수 없습니다.");

            return false;
        }

        selectedMission = mission;

        OnMissionSelected?.Invoke(selectedMission);

        return true;
    }

    public bool SelectMission(int index)
    {
        if (index < 0 || index >= drawnMissions.Count)
        {
            return false;
        }

        return SelectMission(drawnMissions[index]);
    }

    private List<FieldMissionData>
        CreateAvailableMissionList()
    {
        List<FieldMissionData> result = new();

        foreach (FieldMissionData mission in missionPool)
        {
            if (mission == null)
                continue;

            if (result.Contains(mission))
                continue;

            result.Add(mission);
        }

        return result;
    }

    public void ResetMission()
    {
        drawnMissions.Clear();
        selectedMission = null;
    }

    private void Shuffle(List<FieldMissionData> missions)
    {
        for (int i = missions.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            FieldMissionData temp = missions[i];

            missions[i] = missions[randomIndex];

            missions[randomIndex] = temp;
        }
    }

}