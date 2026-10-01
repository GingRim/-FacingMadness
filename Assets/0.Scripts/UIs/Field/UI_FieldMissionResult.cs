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
    private FieldSessionResult sessionResult;

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
        ResolveFieldManager();
        RegisterFieldManager();

        // 전투 화면 동안 필드 UI가 비활성화되어 사망 이벤트를
        // 직접 받지 못했더라도 다시 켜질 때 결과를 복구합니다.
        if (fieldManager != null &&
            fieldManager.CurrentSessionResult != null)
        {
            HandleSessionEnded(fieldManager.CurrentSessionResult);
        }
    }

    private void ResolveFieldManager()
    {
        if (fieldManager != null)
            return;

        fieldManager = GameManager.Instance != null
            ? GameManager.Instance.Field
            : null;

        if (fieldManager == null)
        {
            fieldManager = FindFirstObjectByType<FieldManager>(
                FindObjectsInactive.Include);
        }
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

        fieldManager.OnSessionEnded -= HandleSessionEnded;
        fieldManager.OnSessionEnded += HandleSessionEnded;
    }

    private void UnregisterFieldManager()
    {
        if (fieldManager == null)
            return;

        fieldManager.OnMissionCleared -= HandleMissionCleared;

        fieldManager.OnFieldGameOver -= HandleFieldGameOver;
        fieldManager.OnSessionEnded -= HandleSessionEnded;
    }

    private void HandleMissionCleared(FieldMissionData mission)
    {
        // 새 세션 엔딩 이벤트가 먼저 처리된 경우 중복으로 열지 않습니다.
        if (sessionResult != null)
            return;

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
        // 새 세션 엔딩 이벤트가 먼저 처리된 경우 중복으로 열지 않습니다.
        if (sessionResult != null)
            return;

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

    private void HandleSessionEnded(FieldSessionResult result)
    {
        if (result == null)
            return;

        sessionResult = result;
        resultMission = result.Mission;
        missionCleared = !result.IsDeath;

        FieldSessionEndingData ending = result.Ending;

        if (titleText != null)
        {
            string rank = ending != null ? ending.EndingRank : string.Empty;
            string title = ending != null ? ending.EndingTitle : string.Empty;
            string heading = string.Join(" ", new[] { rank, title })
                .Trim();

            titleText.SetText(
                string.IsNullOrEmpty(heading)
                    ? (result.IsDeath ? "사망" : "세션 종료")
                    : heading);
        }

        if (descriptionText != null)
        {
            string description = ending != null
                ? ending.Description
                : string.Empty;

            if (string.IsNullOrEmpty(description))
            {
                description = result.IsDeath
                    ? "플레이어가 사망했습니다."
                    : $"최종 점수: {result.Score}";
            }

            descriptionText.SetText(description);
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
        sessionResult = null;

        Close();

        if (fieldManager != null)
        {
            fieldManager.EndField();
        }

        OnResultConfirmed?.Invoke(completedMission, wasCleared);
    }
}
