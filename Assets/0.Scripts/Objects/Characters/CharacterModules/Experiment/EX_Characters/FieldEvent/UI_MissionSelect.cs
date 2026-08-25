using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 현재 챕터에서 추첨된 미션을 최대 3개까지 표시하고,
/// 플레이어가 선택한 미션을 MissionManager에 전달합니다.
/// </summary>
public class UI_MissionSelect : UIBase
{
    [Header("현재 챕터")]
    [SerializeField]
    private TextMeshProUGUI chapterNameText;

    [Header("미션 버튼")]
    [SerializeField]
    private UI_MissionButton[] missionButtons;

    private MissionManager missionManager;

    public bool IsOpen => gameObject.activeSelf;

    public event Action<FieldMissionData> OnMissionConfirmed;

    /// <summary>
    /// 미션 버튼의 선택 이벤트를 연결하고
    /// 미션 선택 화면을 초기 상태로 닫습니다.
    /// </summary>
    private void Awake()
    {
        BindButtons();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 오브젝트가 제거될 때 버튼과 매니저 이벤트 연결을 해제합니다.
    /// </summary>
    private void OnDestroy()
    {
        UnbindButtons();
        UnbindManager();
    }

    /// <summary>
    /// 등록된 모든 미션 버튼의 선택 이벤트를 연결합니다.
    /// </summary>
    private void BindButtons()
    {
        if (missionButtons == null)
            return;

        foreach (UI_MissionButton button
                 in missionButtons)
        {
            if (button == null)
                continue;

            button.OnSelected -=
                HandleMissionButtonSelected;

            button.OnSelected +=
                HandleMissionButtonSelected;
        }
    }

    /// <summary>
    /// 등록된 모든 미션 버튼의 선택 이벤트 연결을 해제합니다.
    /// </summary>
    private void UnbindButtons()
    {
        if (missionButtons == null)
            return;

        foreach (UI_MissionButton button
                 in missionButtons)
        {
            if (button == null)
                continue;

            button.OnSelected -=
                HandleMissionButtonSelected;
        }
    }

    /// <summary>
    /// 미션 선택 화면에서 사용할 MissionManager를 연결합니다.
    /// </summary>
    /// <param name="manager">연결할 MissionManager입니다.</param>
    private void BindManager(
        MissionManager manager)
    {
        UnbindManager();

        missionManager = manager;

        if (missionManager == null)
            return;

        missionManager.OnChapterChanged -=
            HandleChapterChanged;

        missionManager.OnChapterChanged +=
            HandleChapterChanged;

        missionManager.OnMissionsDrawn -=
            RefreshMissions;

        missionManager.OnMissionsDrawn +=
            RefreshMissions;
    }

    /// <summary>
    /// 현재 연결된 MissionManager의 이벤트 연결을 해제합니다.
    /// </summary>
    private void UnbindManager()
    {
        if (missionManager == null)
            return;

        missionManager.OnChapterChanged -=
            HandleChapterChanged;

        missionManager.OnMissionsDrawn -=
            RefreshMissions;

        missionManager = null;
    }

    /// <summary>
    /// 미션 선택 화면을 열고 현재 챕터의 미션을 추첨합니다.
    /// </summary>
    /// <param name="manager">
    /// 현재 챕터가 설정된 MissionManager입니다.
    /// </param>
    public void Open(
        MissionManager manager)
    {
        if (manager == null)
        {
            Debug.LogWarning(
                "UI_MissionSelect: MissionManager가 없습니다.");

            return;
        }

        BindManager(manager);

        gameObject.SetActive(true);

        ClearButtons();

        HandleChapterChanged(
            missionManager.CurrentChapter);

        missionManager.DrawMissions();
    }

    /// <summary>
    /// 미션 선택 화면을 닫고 현재 표시 내용을 초기화합니다.
    /// </summary>
    public void Close()
    {
        UnbindManager();
        ClearButtons();
        ClearChapterTitle();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 현재 챕터가 변경되면 화면 상단에 챕터 이름을 표시합니다.
    /// </summary>
    /// <param name="chapter">새로 설정된 챕터 데이터입니다.</param>
    private void HandleChapterChanged(
        FieldChapterData chapter)
    {
        if (chapterNameText == null)
            return;

        if (chapter == null)
        {
            chapterNameText.SetText(string.Empty);
            return;
        }

        chapterNameText.SetText(
            chapter.ChapterName);
    }

    /// <summary>
    /// 추첨된 미션을 미션 버튼에 순서대로 표시합니다.
    /// 사용하지 않는 버튼은 비활성화합니다.
    /// </summary>
    /// <param name="missions">추첨된 미션 목록입니다.</param>
    private void RefreshMissions(
        IReadOnlyList<FieldMissionData> missions)
    {
        ClearButtons();

        if (missions == null ||
            missionButtons == null)
        {
            return;
        }

        int displayCount = Mathf.Min(
            missions.Count,
            missionButtons.Length);

        for (int i = 0;
             i < displayCount;
             i++)
        {
            if (missionButtons[i] == null)
                continue;

            missionButtons[i].SetMission(
                missions[i]);
        }
    }

    /// <summary>
    /// 모든 미션 버튼의 표시 정보와 선택 상태를 초기화합니다.
    /// </summary>
    private void ClearButtons()
    {
        if (missionButtons == null)
            return;

        foreach (UI_MissionButton button
                 in missionButtons)
        {
            if (button == null)
                continue;

            button.Clear();
        }
    }

    /// <summary>
    /// 화면 상단의 챕터 이름을 초기화합니다.
    /// </summary>
    private void ClearChapterTitle()
    {
        if (chapterNameText != null)
        {
            chapterNameText.SetText(
                string.Empty);
        }
    }

    /// <summary>
    /// 미션 버튼에서 선택된 미션을 MissionManager에 전달합니다.
    /// 선택에 성공하면 외부에 확정 이벤트를 보내고 화면을 닫습니다.
    /// </summary>
    /// <param name="mission">플레이어가 선택한 미션입니다.</param>
    private void HandleMissionButtonSelected(
        FieldMissionData mission)
    {
        if (missionManager == null ||
            mission == null)
        {
            return;
        }

        bool selected =
            missionManager.SelectMission(mission);

        if (!selected)
            return;

        OnMissionConfirmed?.Invoke(mission);

        Close();
    }
}