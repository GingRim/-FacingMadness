using System;
using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// 시나리오의 한 진행 단계를 나타냅니다.
/// 등록된 챕터 후보 중 하나가 무작위로 공개됩니다.
/// </summary>
[Serializable]
public class ChapterStageData
{
    [Header("진행 단계")]
    [SerializeField, Min(1)]
    private int stageNumber = 1;

    [SerializeField]
    private string stageName;

    [Header("챕터 후보")]
    [SerializeField]
    private List<FieldChapterData> chapterCandidates = new();

    public int StageNumber => stageNumber;
    public string StageName => stageName;

    public IReadOnlyList<FieldChapterData> ChapterCandidates =>
        chapterCandidates;

    /// <summary>
    /// 챕터 후보 목록에서 null 항목을 제거합니다.
    /// </summary>
    public void RemoveInvalidChapters()
    {
        chapterCandidates.RemoveAll(
            chapter => chapter == null);
    }
}