using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_FieldMissionResult : UIBase
{
    [Header("필드 매니저")]
    [SerializeField]
    private FieldManager fieldManager;

    [Header("화면")]
    [SerializeField]
    private GameObject panel;

    [Header("텍스트")]
    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [Header("버튼")]
    [SerializeField]
    private Button continueButton;

    private FieldMissionData resultMission;
    private bool missionCleared;

    /// <summary>
    /// 미션 결과 확인 후 다음 화면을 요청한다.
    /// bool 값은 클리어 여부다.
    /// </summary>
    public event Action<FieldMissionData, bool>
        OnResultConfirmed;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinue);

            continueButton.onClick.AddListener(HandleContinue);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        RegisterFieldManager();
    }

    private void OnDisable()
    {
        UnregisterFieldManager();
    }

    private void OnDestroy()
    {
        UnregisterFieldManager();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinue);
        }
    }

    private void RegisterFieldManager()
    {
        if (fieldManager == null)
            return;

        fieldManager.OnMissionCleared -= HandleMissionCleared;

        fieldManager.OnMissionCleared += HandleMissionCleared;

        fieldManager.OnFieldGameOver -= HandleFieldGameOver;

        fieldManager.OnFieldGameOver += HandleFieldGameOver;
    }

    private void UnregisterFieldManager()
    {
        if (fieldManager == null)
            return;

        fieldManager.OnMissionCleared -= HandleMissionCleared;

        fieldManager.OnFieldGameOver -= HandleFieldGameOver;
    }

    private void HandleMissionCleared(FieldMissionData mission)
    {
        resultMission = mission;
        missionCleared = true;

        if (titleText != null)
        {
            titleText.SetText("미션 완료");
        }

        if (descriptionText != null)
        {
            string missionName = mission != null ? mission.MissionName : "알 수 없는 미션";

            descriptionText.SetText($"{missionName} 미션을 완료했습니다.");
        }

        Open();
    }

    private void HandleFieldGameOver()
    {
        resultMission = fieldManager != null ? fieldManager.CurrentMission : null;

        missionCleared = false;

        if (titleText != null)
        {
            titleText.SetText("미션 실패");
        }

        if (descriptionText != null)
        {
            descriptionText.SetText("플레이어가 사망하여 미션에 실패했습니다.");
        }

        Open();
    }

    private void Open()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void Close()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void HandleContinue()
    {
        FieldMissionData completedMission = resultMission;

        bool wasCleared = missionCleared;

        resultMission = null;
        missionCleared = false;

        Close();

        if (fieldManager != null)
        {
            fieldManager.EndField();
        }

        OnResultConfirmed?.Invoke(completedMission, wasCleared);
    }
}
