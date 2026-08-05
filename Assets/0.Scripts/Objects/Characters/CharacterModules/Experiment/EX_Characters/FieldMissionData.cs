using UnityEngine;

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

    public string MissionId => missionId;
    public string MissionName => missionName;
    public string Description => description;
    public Sprite MissionImage => missionImage;
}