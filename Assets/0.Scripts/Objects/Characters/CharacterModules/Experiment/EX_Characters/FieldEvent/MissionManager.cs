using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 선택된 챕터에 포함된 미션을 추첨하고,
/// 플레이어가 선택한 미션을 관리합니다.
/// </summary>
public class MissionManager : ManagerBase
{
    private readonly List<FieldMissionData> drawnMissions = new();

    private FieldChapterData currentChapter;
    private FieldMissionData selectedMission;

    public FieldChapterData CurrentChapter =>
        currentChapter;

    public IReadOnlyList<FieldMissionData> DrawnMissions =>
        drawnMissions;

    public FieldMissionData SelectedMission =>
        selectedMission;

    public bool HasCurrentChapter =>
        currentChapter != null;

    public bool HasSelectedMission =>
        selectedMission != null;

    public event Action<FieldChapterData> OnChapterChanged;

    public event Action<IReadOnlyList<FieldMissionData>>
        OnMissionsDrawn;

    public event Action<FieldMissionData> OnMissionSelected;

    /// <summary>
    /// MissionManager를 GameManager에 연결합니다.
    /// 현재는 별도의 초기화 작업 없이 연결 완료를 기다립니다.
    /// </summary>
    /// <param name="newManager">연결할 GameManager입니다.</param>
    /// <returns>초기화 진행을 위한 코루틴입니다.</returns>
    protected override System.Collections.IEnumerator OnConnected(
        GameManager newManager)
    {
        yield return null;
    }

    /// <summary>
    /// GameManager와의 연결이 해제될 때
    /// 현재 챕터와 미션 정보를 초기화합니다.
    /// </summary>
    protected override void OnDisconnected()
    {
        ResetMission();
    }

    /// <summary>
    /// 미션을 추첨할 현재 챕터를 설정합니다.
    /// 기존 추첨 목록과 선택된 미션은 초기화합니다.
    /// </summary>
    /// <param name="chapter">새로 설정할 챕터 데이터입니다.</param>
    /// <returns>챕터 설정에 성공하면 true를 반환합니다.</returns>
    public bool SetChapter(
        FieldChapterData chapter)
    {
        if (chapter == null)
        {
            Debug.LogWarning(
                "MissionManager: 설정할 챕터가 없습니다.");

            return false;
        }

        currentChapter = chapter;
        selectedMission = null;

        drawnMissions.Clear();

        Debug.Log(
            $"현재 챕터 설정: {currentChapter.ChapterName}");

        OnChapterChanged?.Invoke(currentChapter);

        return true;
    }

    /// <summary>
    /// 현재 챕터에 포함된 미션 중 설정된 개수만큼
    /// 중복 없이 무작위로 추첨합니다.
    /// </summary>
    public void DrawMissions()
    {
        drawnMissions.Clear();
        selectedMission = null;

        if (currentChapter == null)
        {
            Debug.LogWarning(
                "MissionManager: 현재 챕터가 없습니다.");

            OnMissionsDrawn?.Invoke(drawnMissions);
            return;
        }

        List<FieldMissionData> availableMissions =
            CreateAvailableMissionList();

        if (availableMissions.Count == 0)
        {
            Debug.LogWarning(
                "MissionManager: 추첨할 미션이 없습니다.");

            OnMissionsDrawn?.Invoke(drawnMissions);
            return;
        }

        int requestedCount = Mathf.Max(
            1,
            currentChapter.MissionDrawCount);

        int actualDrawCount = Mathf.Min(
            requestedCount,
            availableMissions.Count);

        for (int i = 0; i < actualDrawCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(
                0,
                availableMissions.Count);

            FieldMissionData mission =
                availableMissions[randomIndex];

            drawnMissions.Add(mission);
            availableMissions.RemoveAt(randomIndex);
        }

        Debug.Log(
            $"미션 {drawnMissions.Count}개 추첨 완료");

        OnMissionsDrawn?.Invoke(drawnMissions);
    }

    /// <summary>
    /// 플레이어에게 공개된 미션 중 하나를 선택합니다.
    /// </summary>
    /// <param name="mission">선택할 미션 데이터입니다.</param>
    /// <returns>미션 선택에 성공하면 true를 반환합니다.</returns>
    public bool SelectMission(
        FieldMissionData mission)
    {
        if (mission == null)
        {
            return false;
        }

        if (!drawnMissions.Contains(mission))
        {
            Debug.LogWarning(
                "MissionManager: 공개되지 않은 미션은 " +
                "선택할 수 없습니다.");

            return false;
        }

        selectedMission = mission;

        Debug.Log(
            $"미션 선택: {selectedMission.MissionName}");

        OnMissionSelected?.Invoke(selectedMission);

        return true;
    }

    /// <summary>
    /// 추첨된 미션 목록에서 인덱스에 해당하는 미션을 선택합니다.
    /// </summary>
    /// <param name="index">선택할 미션의 목록 인덱스입니다.</param>
    /// <returns>미션 선택에 성공하면 true를 반환합니다.</returns>
    public bool SelectMission(int index)
    {
        if (index < 0 ||
            index >= drawnMissions.Count)
        {
            return false;
        }

        return SelectMission(drawnMissions[index]);
    }

    /// <summary>
    /// 현재 챕터에서 null과 중복을 제외한
    /// 추첨 가능한 미션 목록을 생성합니다.
    /// </summary>
    /// <returns>추첨 가능한 미션 목록입니다.</returns>
    private List<FieldMissionData>
        CreateAvailableMissionList()
    {
        List<FieldMissionData> result = new();

        if (currentChapter == null)
        {
            return result;
        }

        foreach (FieldMissionData mission
                 in currentChapter.Missions)
        {
            if (mission == null)
                continue;

            if (result.Contains(mission))
                continue;

            result.Add(mission);
        }

        return result;
    }

    /// <summary>
    /// 현재 챕터, 추첨된 미션과
    /// 선택된 미션 정보를 모두 초기화합니다.
    /// </summary>
    public void ResetMission()
    {
        currentChapter = null;
        selectedMission = null;

        drawnMissions.Clear();
    }
}