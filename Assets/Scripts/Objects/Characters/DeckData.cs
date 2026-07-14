using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 덱 데이터
/// 시작 덱 및 고정 덱 구성을 저장한다.
/// </summary>
[CreateAssetMenu(fileName = "NewDeckData", menuName = "Card/DeckData")]
public class DeckData : ScriptableObject
{
    [Header("덱 카드 목록")]

    /// <summary>
    /// 덱에 포함되는 카드 리스트
    /// </summary>
    public List<CardData> cards = new();
}

