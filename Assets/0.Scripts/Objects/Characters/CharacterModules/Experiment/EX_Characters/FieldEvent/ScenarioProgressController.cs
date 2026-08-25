using System;
using UnityEngine;


/// <summary>
/// 시나리오의 챕터 공개, 챕터 스토리와
/// 미션 선택 화면 사이의 진행을 연결합니다.
/// 튜토리얼에서는 세션 선택 과정 없이 자동으로 시작합니다.
/// </summary>
public class ScenarioProgressController : MonoBehaviour
{
    [Header("관리자")]
    [SerializeField]
    private ScenarioManager scenarioManager;

    [SerializeField]
    private MissionManager missionManager;

    [Header("튜토리얼")]
    [SerializeField]
    private MainScenarioData tutorialScenario;

    [SerializeField]
    private bool startTutorialAutomatically = true;

    [Header("챕터 스토리 UI")]
    [SerializeField]
    private UI_ChapterStory chapterStoryUI;

    [Header("미션 선택 UI")]
    [SerializeField]
    private UI_MissionSelect missionSelectUI;

    private FieldChapterData currentChapter;

    private bool hasStarted;

    public FieldChapterData CurrentChapter =>
        currentChapter;

    public bool HasStarted =>
        hasStarted;

    public event Action<FieldMissionData>
        OnMissionConfirmed;

    /// <summary>
    /// 시나리오, 챕터 스토리와
    /// 미션 선택 UI의 이벤트를 연결합니다.
    /// </summary>
    private void OnEnable()
    {
        BindEvents();
    }

    /// <summary>
    /// 자동 시작이 설정되어 있으면
    /// 세션 선택 과정 없이 튜토리얼 시나리오를 시작합니다.
    /// </summary>
    private void Start()
    {
        if (!startTutorialAutomatically)
            return;

        StartTutorialScenario();
    }

    /// <summary>
    /// 시나리오, 챕터 스토리와
    /// 미션 선택 UI의 이벤트 연결을 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        UnbindEvents();
    }

    /// <summary>
    /// 진행에 필요한 모든 이벤트를 연결합니다.
    /// </summary>
    private void BindEvents()
    {
        if (scenarioManager != null)
        {
            scenarioManager.OnChapterRevealed -=
                HandleChapterRevealed;

            scenarioManager.OnChapterRevealed +=
                HandleChapterRevealed;
        }

        if (chapterStoryUI != null)
        {
            chapterStoryUI.OnStoryCompleted -=
                HandleChapterStoryCompleted;

            chapterStoryUI.OnStoryCompleted +=
                HandleChapterStoryCompleted;
        }

        if (missionSelectUI != null)
        {
            missionSelectUI.OnMissionConfirmed -=
                HandleMissionConfirmed;

            missionSelectUI.OnMissionConfirmed +=
                HandleMissionConfirmed;
        }
    }

    /// <summary>
    /// 진행에 사용한 모든 이벤트 연결을 해제합니다.
    /// </summary>
    private void UnbindEvents()
    {
        if (scenarioManager != null)
        {
            scenarioManager.OnChapterRevealed -=
                HandleChapterRevealed;
        }

        if (chapterStoryUI != null)
        {
            chapterStoryUI.OnStoryCompleted -=
                HandleChapterStoryCompleted;
        }

        if (missionSelectUI != null)
        {
            missionSelectUI.OnMissionConfirmed -=
                HandleMissionConfirmed;
        }
    }

    /// <summary>
    /// 인스펙터에 설정된 튜토리얼 시나리오를 시작합니다.
    /// 세션 선택 화면은 열지 않습니다.
    /// </summary>
    public void StartTutorialScenario()
    {
        if (hasStarted)
            return;

        if (tutorialScenario == null)
        {
            Debug.LogWarning(
                "ScenarioProgressController: " +
                "튜토리얼 시나리오가 없습니다.");

            return;
        }

        bool started =
            StartScenario(tutorialScenario);

        if (started)
        {
            hasStarted = true;
        }
    }

    /// <summary>
    /// 지정한 시나리오를 시작하고
    /// 첫 번째 단계의 챕터 하나를 무작위로 결정합니다.
    /// </summary>
    /// <param name="scenario">시작할 시나리오 데이터입니다.</param>
    /// <returns>시나리오 시작에 성공하면 true를 반환합니다.</returns>
    public bool StartScenario(
        MainScenarioData scenario)
    {
        if (scenarioManager == null)
        {
            Debug.LogWarning(
                "ScenarioProgressController: " +
                "ScenarioManager가 없습니다.");

            return false;
        }

        currentChapter = null;

        return scenarioManager.SelectScenario(
            scenario);
    }

    /// <summary>
    /// 시스템이 결정한 챕터를 MissionManager에 전달하고
    /// 해당 챕터의 스토리 UI를 엽니다.
    /// </summary>
    /// <param name="stage">현재 시나리오의 챕터 단계입니다.</param>
    /// <param name="chapter">시스템이 무작위로 결정한 챕터입니다.</param>
    private void HandleChapterRevealed(
        ChapterStageData stage,
        FieldChapterData chapter)
    {
        if (chapter == null)
        {
            Debug.LogWarning(
                "ScenarioProgressController: " +
                "결정된 챕터가 없습니다.");

            return;
        }

        if (missionManager == null)
        {
            Debug.LogWarning(
                "ScenarioProgressController: " +
                "MissionManager가 없습니다.");

            return;
        }

        currentChapter = chapter;

        bool chapterSet =
            missionManager.SetChapter(
                currentChapter);

        if (!chapterSet)
            return;

        if (chapterStoryUI == null)
        {
            Debug.LogWarning(
                "ScenarioProgressController: " +
                "챕터 스토리 UI가 없습니다.");

            return;
        }

        Debug.Log(
            $"챕터 스토리 공개: " +
            $"{currentChapter.ChapterName}");

        chapterStoryUI.Open(
            currentChapter);
    }

    /// <summary>
    /// 챕터 스토리에서 스킵 버튼을 누르면
    /// 현재 챕터의 미션 선택 화면을 엽니다.
    /// </summary>
    /// <param name="chapter">스토리 표시를 완료한 챕터입니다.</param>
    private void HandleChapterStoryCompleted(
        FieldChapterData chapter)
    {
        if (chapter == null ||
            chapter != currentChapter)
        {
            return;
        }

        OpenMissionSelection();
    }

    /// <summary>
    /// 현재 챕터에 등록된 미션을 추첨하고
    /// 미션 선택 화면을 엽니다.
    /// </summary>
    public void OpenMissionSelection()
    {
        if (currentChapter == null)
        {
            Debug.LogWarning(
                "ScenarioProgressController: " +
                "현재 챕터가 없습니다.");

            return;
        }

        if (missionManager == null ||
            missionSelectUI == null)
        {
            Debug.LogWarning(
                "ScenarioProgressController: " +
                "미션 선택 연결이 부족합니다.");

            return;
        }

        if (missionManager.CurrentChapter !=
            currentChapter)
        {
            bool chapterSet =
                missionManager.SetChapter(
                    currentChapter);

            if (!chapterSet)
                return;
        }

        missionSelectUI.Open(
            missionManager);
    }

    /// <summary>
    /// 미션 선택 UI에서 확정된 미션을
    /// 다음 필드 진입 단계에 전달합니다.
    /// </summary>
    /// <param name="mission">플레이어가 선택한 미션입니다.</param>
    private void HandleMissionConfirmed(
        FieldMissionData mission)
    {
        if (mission == null)
            return;

        Debug.Log(
            $"미션 확정: {mission.MissionName}");

        OnMissionConfirmed?.Invoke(
            mission);
    }

    /// <summary>
    /// 현재 챕터를 통과 처리하고
    /// 다음 단계의 챕터 하나를 무작위로 결정합니다.
    /// </summary>
    public void CompleteCurrentChapter()
    {
        if (scenarioManager == null)
            return;

        currentChapter = null;

        scenarioManager.CompleteCurrentChapter();
    }

    /// <summary>
    /// 튜토리얼 진행 상태를 초기화합니다.
    /// </summary>
    public void ResetProgress()
    {
        hasStarted = false;
        currentChapter = null;

        chapterStoryUI?.Close();
        missionSelectUI?.Close();

        missionManager?.ResetMission();
        scenarioManager?.ResetScenario();
    }
}