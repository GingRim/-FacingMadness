using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;


/// <summary>
/// 세션에서 사용할 시나리오와 현재 챕터 진행 단계를 관리합니다.
/// 각 단계에 도달하면 후보 중 챕터 하나를 무작위로 공개합니다.
/// </summary>
public class ScenarioManager : ManagerBase
{
    [Header("시나리오 목록")]
    [SerializeField]
    private List<MainScenarioData> scenarioPool = new();

    private MainScenarioData selectedScenario;
    private ChapterStageData currentStage;
    private FieldChapterData currentChapter;

    private int currentStageIndex = -1;

    public IReadOnlyList<MainScenarioData> ScenarioPool =>
        scenarioPool;

    public MainScenarioData SelectedScenario =>
        selectedScenario;

    public ChapterStageData CurrentStage =>
        currentStage;

    public FieldChapterData CurrentChapter =>
        currentChapter;

    public int CurrentStageIndex =>
        currentStageIndex;

    public bool HasSelectedScenario =>
        selectedScenario != null;

    public bool HasCurrentChapter =>
        currentChapter != null;

    public event Action<MainScenarioData>
        OnScenarioSelected;

    public event Action<
        ChapterStageData,
        FieldChapterData>
        OnChapterRevealed;

    public event Action<MainScenarioData>
        OnScenarioCompleted;

    /// <summary>
    /// ScenarioManager를 GameManager에 연결합니다.
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
    /// 현재 시나리오 진행 상태를 초기화합니다.
    /// </summary>
    protected override void OnDisconnected()
    {
        ResetScenario();
    }

    /// <summary>
    /// 진행할 시나리오를 선택하고 첫 번째 챕터 단계를 시작합니다.
    /// </summary>
    /// <param name="scenario">선택할 시나리오입니다.</param>
    /// <returns>
    /// 시나리오 선택과 첫 챕터 공개에 성공하면 true를 반환합니다.
    /// </returns>
    public bool SelectScenario(
        MainScenarioData scenario)
    {
        if (scenario == null)
        {
            Debug.LogWarning(
                "ScenarioManager: 선택할 시나리오가 없습니다.");

            return false;
        }

        if (!scenarioPool.Contains(scenario))
        {
            Debug.LogWarning(
                "ScenarioManager: 등록되지 않은 시나리오입니다.");

            return false;
        }

        ResetScenario();

        selectedScenario = scenario;
        currentStageIndex = 0;

        Debug.Log(
            $"시나리오 시작: {selectedScenario.ScenarioName}");

        OnScenarioSelected?.Invoke(
            selectedScenario);

        return RevealCurrentChapter();
    }

    /// <summary>
    /// 현재 진행 단계의 후보 목록에서
    /// 챕터 하나를 중복 없이 무작위로 선택하여 공개합니다.
    /// </summary>
    /// <returns>챕터 공개에 성공하면 true를 반환합니다.</returns>
    public bool RevealCurrentChapter()
    {
        if (selectedScenario == null)
        {
            Debug.LogWarning(
                "ScenarioManager: 선택된 시나리오가 없습니다.");

            return false;
        }

        if (currentStageIndex < 0 ||
            currentStageIndex >=
            selectedScenario.StageCount)
        {
            Debug.LogWarning(
                "ScenarioManager: 현재 챕터 단계가 올바르지 않습니다.");

            return false;
        }

        currentStage =
            selectedScenario.ChapterStages[currentStageIndex];

        if (currentStage == null)
        {
            Debug.LogWarning(
                "ScenarioManager: 챕터 단계 데이터가 없습니다.");

            return false;
        }

        List<FieldChapterData> candidates =
            CreateAvailableChapterList(currentStage);

        if (candidates.Count == 0)
        {
            Debug.LogWarning(
                $"ScenarioManager: " +
                $"{currentStage.StageNumber}단계에 " +
                "사용 가능한 챕터가 없습니다.");

            return false;
        }

        int randomIndex = UnityEngine.Random.Range(
            0,
            candidates.Count);

        currentChapter = candidates[randomIndex];

        Debug.Log(
            $"챕터 공개: {currentChapter.ChapterName}");

        OnChapterRevealed?.Invoke(
            currentStage,
            currentChapter);

        return true;
    }

    /// <summary>
    /// 현재 챕터를 통과하고 다음 진행 단계로 이동합니다.
    /// 마지막 단계를 통과했다면 시나리오 완료 이벤트를 발생시킵니다.
    /// </summary>
    /// <returns>
    /// 다음 챕터가 공개되거나 시나리오가 완료되면 true를 반환합니다.
    /// </returns>
    public bool CompleteCurrentChapter()
    {
        if (selectedScenario == null ||
            currentChapter == null)
        {
            return false;
        }

        Debug.Log(
            $"챕터 통과: {currentChapter.ChapterName}");

        currentChapter = null;
        currentStage = null;

        currentStageIndex++;

        if (currentStageIndex >=
            selectedScenario.StageCount)
        {
            Debug.Log(
                $"시나리오 완료: " +
                $"{selectedScenario.ScenarioName}");

            OnScenarioCompleted?.Invoke(
                selectedScenario);

            return true;
        }

        return RevealCurrentChapter();
    }

    /// <summary>
    /// 지정한 단계에서 null과 중복을 제외한
    /// 챕터 후보 목록을 생성합니다.
    /// </summary>
    /// <param name="stage">후보 목록을 가져올 챕터 단계입니다.</param>
    /// <returns>무작위 선택에 사용할 챕터 후보 목록입니다.</returns>
    private List<FieldChapterData>
        CreateAvailableChapterList(
            ChapterStageData stage)
    {
        List<FieldChapterData> result = new();

        if (stage == null ||
            stage.ChapterCandidates == null)
        {
            return result;
        }

        foreach (FieldChapterData chapter
                 in stage.ChapterCandidates)
        {
            if (chapter == null)
                continue;

            if (result.Contains(chapter))
                continue;

            result.Add(chapter);
        }

        return result;
    }

    /// <summary>
    /// 현재 시나리오, 챕터 단계와 공개된 챕터를 초기화합니다.
    /// </summary>
    public void ResetScenario()
    {
        selectedScenario = null;
        currentStage = null;
        currentChapter = null;

        currentStageIndex = -1;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 인스펙터의 시나리오 목록에서 null 항목을 제거합니다.
    /// </summary>
    private void OnValidate()
    {
        scenarioPool.RemoveAll(scenario => scenario == null);
    }
#endif
}