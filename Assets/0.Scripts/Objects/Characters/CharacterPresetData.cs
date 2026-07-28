using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 데모용 캐릭터 프리셋 데이터
/// 정식 버전에서는 캐릭터 생성 화면 결과가 CharacterBuildData를 만든다.
/// </summary>

[CreateAssetMenu(fileName = "NewCharacterPreset", menuName = "Character/Preset")]
public class CharacterPresetData : ScriptableObject
{
    [Header("기본 정보")]
    public string characterName;

    [Header("LV")]
    public int level;

    [Header("능력치")]
    public int strength;
    public int agility;
    public int health;
    public int intelligence;
    public int will;

    [Header("기본 덱")]
    public DeckData startDeck;

    [Header("캐릭터 아이콘")]
    public Sprite Icon;

    /// <summary>
    /// 프리셋 데이터를 실제 생성용 데이터로 변환한다.
    /// </summary>
    public CharacterBuildData ToBuildData()
    {
        return new CharacterBuildData
        {
            characterName = characterName,

            level = level,

            strength = strength,
            agility = agility,
            health = health,
            intelligence = intelligence,
            will = will,

            startDeck = startDeck,

            Icon = Icon
        };
    }
}
