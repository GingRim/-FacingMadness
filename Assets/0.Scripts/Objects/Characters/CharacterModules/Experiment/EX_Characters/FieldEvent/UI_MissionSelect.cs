using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_MissionSelect : UIBase
{
    [Header("미션 버튼 3개")]
    [SerializeField]
    private UI_MissionButton[] missionButtons;

    private MissionManager missionManager;

    public bool IsOpen => gameObject.activeSelf;

    public event Action<FieldMissionData> OnMissionConfirmed;

    private void Awake()
    {
        BindButtons();

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UnbindButtons();
        UnbindManager();
    }

    private void BindButtons()
    {
        if (missionButtons == null)
            return;

        foreach (UI_MissionButton button in missionButtons)
        {
            if (button == null)
                continue;

            button.OnSelected -= HandleMissionButtonSelected;

            button.OnSelected += HandleMissionButtonSelected;
        }
    }

    private void UnbindButtons()
    {
        if (missionButtons == null)
            return;

        foreach (UI_MissionButton button in missionButtons)
        {
            if (button == null)
                continue;

            button.OnSelected -= HandleMissionButtonSelected;
        }
    }

    private void BindManager(MissionManager manager)
    {
        UnbindManager();

        missionManager = manager;

        if (missionManager == null)
            return;

        missionManager.OnMissionsDrawn -= RefreshMissions;

        missionManager.OnMissionsDrawn += RefreshMissions;
    }

    private void UnbindManager()
    {
        if (missionManager == null)
            return;

        missionManager.OnMissionsDrawn -= RefreshMissions;

        missionManager = null;
    }

    public void Open(MissionManager manager)
    {
        if (manager == null)
        {
            Debug.LogWarning("UI_MissionSelect: MissionManager가 없습니다.");

            return;
        }

        BindManager(manager);

        gameObject.SetActive(true);

        ClearButtons();

        missionManager.DrawMissions();
    }

    public void Close()
    {
        UnbindManager();
        ClearButtons();

        gameObject.SetActive(false);
    }

    private void RefreshMissions(IReadOnlyList<FieldMissionData> missions)
    {
        ClearButtons();

        if (missions == null || missionButtons == null)
        {
            return;
        }

        int displayCount = Mathf.Min(missions.Count, missionButtons.Length);

        for (int i = 0; i < displayCount; i++)
        {
            if (missionButtons[i] == null)
                continue;

            missionButtons[i].SetMission(missions[i]);
        }
    }

    private void ClearButtons()
    {
        if (missionButtons == null)
            return;

        foreach (UI_MissionButton button in missionButtons)
        {
            if (button == null)
                continue;

            button.Clear();
        }
    }

    private void HandleMissionButtonSelected(FieldMissionData mission)
    {
        if (missionManager == null || mission == null)
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
