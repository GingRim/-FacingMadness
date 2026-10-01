using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 한 세션 전체를 구성하는 시나리오 데이터입니다.
/// 시나리오는 순서대로 진행되는 챕터 단계 목록을 가집니다.
/// </summary>
[CreateAssetMenu(fileName = "MainScenario", menuName = "Facing Madness/Scenario/Main Scenario")]
public class MainScenarioData : ScriptableObject
{
    [Header("시나리오 식별 정보")]
    [SerializeField]
    private string scenarioId;

    [SerializeField]
    private string scenarioName;

    [SerializeField, TextArea(3, 10)]
    private string description;

    [Header("시나리오 표시")]
    [SerializeField]
    private Sprite scenarioImage;

    [Header("챕터 진행 단계")]
    [SerializeField]
    private List<ChapterStageData> chapterStages = new();

    public string ScenarioId => scenarioId;
    public string ScenarioName => scenarioName;
    public string Description => description;
    public Sprite ScenarioImage => scenarioImage;

    public IReadOnlyList<ChapterStageData> ChapterStages =>
        chapterStages;

    public int StageCount => chapterStages.Count;

#if UNITY_EDITOR
    /// <summary>
    /// 인스펙터에 등록된 챕터 단계와 후보 목록을 정리합니다.
    /// </summary>
    private void OnValidate()
    {
        chapterStages.RemoveAll(stage => stage == null);

        foreach (ChapterStageData stage
                 in chapterStages)
        {
            stage.RemoveInvalidChapters();
        }
    }
#endif
}