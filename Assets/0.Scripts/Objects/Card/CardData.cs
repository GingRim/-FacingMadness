using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("기본 정보")]
    public string cardName;

    //public Sprite illustration;

    [Header("카드 색상")]
    public CardColorType color;

    [Header("특수 카드 유무")]
    public bool isOneUse;


    [Header("마법 카드")]
    public MagicCardType magicCardType = MagicCardType.None;

    [Header("자색 카드 생성 목록")]
    public CardData forbiddenMagicCard;
    public CardData attackMagicCard;
    public CardData defenseMagicCard;
    public CardData buffMagicCard;


    [Header("설명")]
    [TextArea]
    public string description;
}
