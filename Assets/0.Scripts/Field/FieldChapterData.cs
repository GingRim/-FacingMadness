using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 메인 시나리오에 포함되는 하나의 챕터 정보를 보관합니다.
/// 하나의 챕터는 여러 필드 미션을 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "FieldChapter", menuName = "Facing Madness/Scenario/Field Chapter")]
public class FieldChapterData : ScriptableObject
{
    [Header("챕터 식별 정보")]
    [SerializeField]
    private string chapterId;

    [SerializeField]
    private string chapterName;

    [SerializeField, TextArea(3, 10)]
    private string description;

    [Header("챕터 표시")]
    [SerializeField]
    private Sprite chapterImage;

    [Header("포함된 미션")]
    [SerializeField]
    private List<FieldMissionData> missions = new();

    [Header("미션 공개 개수")]
    [SerializeField, Min(1)]
    private int missionDrawCount = 3;

    public string ChapterId => chapterId;
    public string ChapterName => chapterName;
    public string Description => description;
    public Sprite ChapterImage => chapterImage;

    public IReadOnlyList<FieldMissionData> Missions => missions;

    public int MissionDrawCount => missionDrawCount;

#if UNITY_EDITOR
    /// <summary>
    /// 인스펙터에 등록된 미션 목록과 공개 개수를 올바르게 정리합니다.
    /// </summary>
    private void OnValidate()
    {
        missionDrawCount = Mathf.Max(1, missionDrawCount);

        missions.RemoveAll(mission => mission == null);
    }
#endif
}