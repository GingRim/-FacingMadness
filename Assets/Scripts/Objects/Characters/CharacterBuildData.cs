using System;
using UnityEngine;

/// <summary>
/// 캐릭터 생성 결과 데이터
/// 생성 화면 또는 데모 프리셋에서 만들어진 값을 담는다.
/// </summary>
[System.Serializable]

public class CharacterBuildData
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

}
