using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Monster/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public string monsterName;

    [Header("프리팹")]
    public CharacterBase monsterPrefab;

    [Header("능력치")]
    public int strength = 4;
    public int agility = 4;
    public int health = 4;
    public int intelligence = 4;
    public int will = 4;

    [Header("기본 장갑")]
    public int baseArmor = 0;

    [Header("난이도 보정")]
    public int difficultyModifier = 0;

    [Header("캐릭터 아이콘")]
    public Sprite Icon;

    [Header("전투 AI")]
    [InspectorName("몬스터 AI 설정")]
    [Tooltip("이 몬스터가 전투에서 사용할 행동과 대응 규칙입니다. 비어 있으면 기본 AI로 행동합니다.")]
    public MonsterAIProfile aiProfile;
}
