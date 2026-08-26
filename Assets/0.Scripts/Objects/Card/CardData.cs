using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("기본 정보")]
    public string cardName;

    [TextArea(3, 10)]
    public string description;

    [SerializeField]
    private Sprite illustration;

    [Header("카드 색상")]
    public CardColorType color;

    [Header("특수 카드")]
    public bool isOneUse;

    [Header("키워드")]
    [SerializeField]
    private List<CardKeywordType> keywords = new();

    [Tooltip("새로 획득하는 이 카드의 기본 내구도입니다. " + "기존 카드에 런타임으로 키워드를 각인할 때는 1+1D10을 사용합니다.")]
    [SerializeField, Min(0)]
    private int baseDurability;

    [Header("필드 판정")]
    [SerializeField, Min(2)]
    private int fieldCheckTarget = 5;

    [Header("마법 카드")]
    public MagicCardType magicCardType = MagicCardType.None;

    [Header("자색 카드 생성 목록")]
    public CardData forbiddenMagicCard;
    public CardData attackMagicCard;
    public CardData defenseMagicCard;
    public CardData buffMagicCard;

    public Sprite Illustration => illustration;

    public IReadOnlyList<CardKeywordType> Keywords => keywords;

    public int BaseDurability => UsesDurability ? Mathf.Max(1, baseDurability) : 0;

    /// <summary>
    /// 카드를 필드에서 직접 사용할 때 적용하는 판정 목표치입니다.
    /// 이벤트 선택지의 목표치와는 별도로 사용합니다.
    /// </summary>
    public int FieldCheckTarget => Mathf.Max(2, fieldCheckTarget);

    /// <summary>
    /// 키워드 중 하나라도 내구도를 요구하는지 확인합니다.
    /// </summary>
    public bool UsesDurability
    {
        get
        {
            if (keywords == null)
                return false;

            foreach (CardKeywordType keyword in keywords)
            {
                if (CardKeywordRules.UsesDurability(keyword))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 카드 원본에 지정 키워드가 포함되어 있는지 확인합니다.
    /// </summary>
    public bool HasKeyword(CardKeywordType keyword)
    {
        if (keyword == CardKeywordType.None ||
            keywords == null)
        {
            return false;
        }

        return keywords.Contains(keyword);
    }

#if UNITY_EDITOR

    /// <summary>
    /// 인스펙터에서 잘못된 키워드와 중복 키워드를 정리합니다.
    /// </summary>
    private void OnValidate()
    {
        if (keywords == null)
        {
            keywords = new List<CardKeywordType>();
        }

        keywords.RemoveAll(keyword => keyword == CardKeywordType.None || keyword == CardKeywordType._Length);

        for (int i = keywords.Count - 1; i >= 0; i--)
        {
            if (keywords.IndexOf(keywords[i]) != i)
            {
                keywords.RemoveAt(i);
            }
        }

        if (UsesDurability)
        {
            baseDurability = Mathf.Max(1, baseDurability);
        }
        else
        {
            baseDurability = 0;
        }

        fieldCheckTarget = Mathf.Max(2, fieldCheckTarget);
    }

#endif

    /// <summary>
    /// 이 카드 원본을 기반으로 새로운 런타임 카드 한 장을 생성합니다.
    /// </summary>
    public CardInstance CreateInstance(int initialDurability = -1)
    {
        return new CardInstance(this, initialDurability);
    }

}
