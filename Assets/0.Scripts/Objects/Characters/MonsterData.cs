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
}
