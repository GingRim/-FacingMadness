using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffectValue
{
    [Tooltip("주사위 결과에 더할 고정 수치입니다.")]
    [SerializeField]
    private int fixedValue;

    [Tooltip("굴릴 주사위 개수입니다. 0이면 고정 수치만 사용합니다.")]
    [SerializeField, Min(0)]
    private int diceCount = 1;

    [Tooltip("효과 계산에 사용할 주사위입니다.")]
    [SerializeField]
    private FieldEffectDiceType diceType = FieldEffectDiceType.D4;

    public int Roll()
    {
        int result = fixedValue;

        for (int i = 0; i < diceCount; i++)
        {
            result += RollDice();
        }

        return Mathf.Max(0, result);
    }

    private int RollDice()
    {
        switch (diceType)
        {
            case FieldEffectDiceType.D4:
                return Dice.RollD4();

            case FieldEffectDiceType.D6:
                return Dice.RollD6();

            case FieldEffectDiceType.D8:
                return Dice.RollD8();

            case FieldEffectDiceType.D10:
                return Dice.RollD10();

            default:
                return 0;
        }
    }
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("기본 정보")]
    public string cardName;

    [TextArea(3, 10)]
    public string description;

    [SerializeField]
    private Sprite illustration;

    [Header("카드 표시 정보")]
    [Tooltip("카드 우측 상단에 표시할 간단한 아이콘입니다.")]
    [SerializeField]
    private Sprite icon;

    [Tooltip("무색 아이템 카드가 함께 점유할 능력치 카드 한도입니다. 색상 카드는 색상에 따라 자동 결정됩니다.")]
    [SerializeField]
    private StatType deckCapacityStat = StatType.None;

    [Header("카드 색상")]
    public CardColorType color;

    [Header("능력치 카드 등급")]
    [Tooltip("능력치 카드의 등급입니다. 등급마다 판정과 전투 효과에 +2가 적용됩니다.")]
    [SerializeField, Range(1, 5)]
    private int cardGrade = 1;

    [Header("특수 카드")]
    public bool isOneUse;

    [Header("키워드")]
    [SerializeField] private List<CardKeywordType> keywords = new();

    [Tooltip("무색 카드와 키워드 카드가 생성될 때 사용할 최대 내구도입니다. 기본값은 3입니다.")]
    [SerializeField, Min(0)]
    private int baseDurability = 3;

    [Header("무색 기본 전투 효과")]
    [Tooltip("키워드가 없는 무색 카드를 행동으로 사용할 때의 피해입니다.")]
    [SerializeField]
    private CardEffectValue colorlessDamage = new();

    [Tooltip("키워드가 없는 무색 카드를 보조 행동으로 사용할 때의 임시 장갑입니다.")]
    [SerializeField]
    private CardEffectValue colorlessArmor = new();

    [Header("마법 카드")]
    public MagicCardType magicCardType = MagicCardType.None;

    [Header("자색 카드 생성 목록")]
    public CardData forbiddenMagicCard;
    public CardData attackMagicCard;
    public CardData defenseMagicCard;
    public CardData buffMagicCard;

    public Sprite Illustration => illustration;

    public Sprite Icon => icon;

    public StatType DeckCapacityStat
    {
        get
        {
            switch (color)
            {
                case CardColorType.Red:
                    return StatType.Strength;

                case CardColorType.Yellow:
                    return StatType.Agility;

                case CardColorType.Green:
                    return StatType.Health;

                case CardColorType.Blue:
                    return StatType.Intelligence;

                case CardColorType.Purple:
                    return StatType.Will;

                case CardColorType.Colorless:
                    return deckCapacityStat;

                default:
                    return StatType.None;
            }
        }
    }

    public IReadOnlyList<CardKeywordType> Keywords => keywords;

    public int CardGrade => Mathf.Clamp(cardGrade, 1, 5);

    public int GradeBonus => CardGrade * 2;

    public bool IsAbilityCard =>
        color == CardColorType.Red ||
        color == CardColorType.Yellow ||
        color == CardColorType.Green ||
        color == CardColorType.Blue ||
        color == CardColorType.Purple;

    public int BaseDurability => UsesDurability
        ? Mathf.Max(0, baseDurability)
        : 0;

    public int RollColorlessDamage()
    {
        return colorlessDamage != null
            ? colorlessDamage.Roll()
            : Dice.RollD4();
    }

    public int RollColorlessArmor()
    {
        return colorlessArmor != null
            ? colorlessArmor.Roll()
            : Dice.RollD4();
    }

    /// <summary>
    /// 무색 카드 또는 키워드 카드인지 확인해 내구도 사용 여부를 반환합니다.
    /// </summary>
    public bool UsesDurability
    {
        get
        {
            return color == CardColorType.Colorless ||
                   HasValidKeyword;
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
        cardGrade = Mathf.Clamp(cardGrade, 1, 5);

        if (deckCapacityStat == StatType._Length)
        {
            deckCapacityStat = StatType.None;
        }

        if (keywords == null)
        {
            keywords = new List<CardKeywordType>();
        }

        // _Length만 제거합니다.
        // 새 슬롯의 기본값인 None은 키워드를 선택할 수 있도록 유지합니다.
        keywords.RemoveAll(keyword => keyword == CardKeywordType._Length);

        // 중복 키워드 제거
        for (int i = keywords.Count - 1; i >= 0; i--)
        {
            // None 슬롯은 키워드 선택 전 임시 항목이므로 중복 제거하지 않습니다.
            if (keywords[i] == CardKeywordType.None)
                continue;

            if (keywords.IndexOf(keywords[i]) != i)
            {
                keywords.RemoveAt(i);
            }
        }

        // 유효한 키워드가 있는 카드만 무색 카드로 변경합니다.
        if (HasValidKeyword)
        {
            color = CardColorType.Colorless;
        }

        if (UsesDurability)
        {
            // 새 카드처럼 값이 설정되지 않은 경우에만 기본값 3 적용
            if (baseDurability <= 0)
            {
                baseDurability = 3;
            }
        }
        else
        {
            baseDurability = 0;
        }
    }

#endif

    /// <summary>
    /// 이 카드 원본을 기반으로 새로운 런타임 카드 한 장을 생성합니다.
    /// </summary>
    public CardInstance CreateInstance(int initialDurability = -1)
    {
        return new CardInstance(this, initialDurability);
    }

    /// <summary>
    /// 실제로 사용할 수 있는 키워드가 하나 이상 있는지 확인합니다.
    /// None과 _Length는 키워드로 취급하지 않습니다.
    /// </summary>
    private bool HasValidKeyword
    {
        get
        {
            if (keywords == null)
                return false;

            foreach (CardKeywordType keyword in keywords)
            {
                if (keyword != CardKeywordType.None &&
                    keyword != CardKeywordType._Length)
                {
                    return true;
                }
            }

            return false;
        }
    }

}
