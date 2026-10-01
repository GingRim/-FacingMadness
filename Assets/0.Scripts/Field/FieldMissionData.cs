using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "FieldMission", menuName = "Facing Madness/Field/Mission")]
public class FieldMissionData : ScriptableObject
{
    [Header("미션 식별 정보")]
    [SerializeField] private string missionId;
    [SerializeField] private string missionName;

    [TextArea(3, 8)]
    [SerializeField] private string description;
    
    [Header("미션 표시")]
    [SerializeField] private Sprite missionImage;

    [Header("필드 오브젝트")]
    [SerializeField]
    private string fieldObjectName;

    [Header("미션 클리어 목표")]
    [SerializeField]
    private List<FieldMissionObjectiveRequirement> objectives = new();

    [Header("세션 엔딩")]
    [Tooltip("정상적인 세션 종료 시 점수 범위에 따라 선택할 엔딩입니다.")]
    [SerializeField]
    private List<FieldSessionEndingData> sessionEndings = new();

    [Tooltip("플레이어가 사망했을 때 점수와 관계없이 강제로 사용할 엔딩입니다.")]
    [SerializeField]
    private FieldSessionEndingData deathEnding;

    [Header("조건 엔딩")]
    [Tooltip("이벤트 결과 종료 시 정보 조건과 생존 조건을 만족하면 즉시 확정할 엔딩입니다.")]
    [SerializeField]
    private List<FieldSessionEndingRequirement> conditionalEndings = new();

    public IReadOnlyList<FieldMissionObjectiveRequirement> Objectives => objectives;
    public IReadOnlyList<FieldSessionEndingData> SessionEndings => sessionEndings;
    public FieldSessionEndingData DeathEnding => deathEnding;
    public IReadOnlyList<FieldSessionEndingRequirement> ConditionalEndings => conditionalEndings;

    public string FieldObjectName => fieldObjectName;

    public string MissionId => missionId;
    public string MissionName => missionName;
    public string Description => description;
    public Sprite MissionImage => missionImage;

}
