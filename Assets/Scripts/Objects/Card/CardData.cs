using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("기본 정보")]
    public string cardName;

    public Sprite illustration;

    [Header("분류")]
    public CardColorType color;

    public CardTagType[] tags;

    [Header("코스트")]
    public CardCostData[] costs;

    [Header("수치")]
    public int damage;
    public int restoreAmount;

    [Header("설명")]
    [TextArea]
    public string description;
}
