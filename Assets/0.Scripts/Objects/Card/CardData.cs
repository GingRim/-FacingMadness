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

/// <summary>
/// 카드에 직접 등록할 전투 효과의 종류입니다.
/// 키워드는 카드 분류에만 사용하고 실제 효과는 이 값으로 결정합니다.
/// </summary>
public enum CardCombatEffectType
{
    [InspectorName("없음")] None,
    [InspectorName("피해")] Damage,
    [InspectorName("생명력 회복")] RestoreHealth,
    [InspectorName("임시 장갑 획득")] TemporaryArmor,
    [InspectorName("상태 효과 부여")] ApplyStatus
}

/// <summary>
/// 카드 효과를 받을 대상입니다.
/// </summary>
public enum CardCombatEffectTarget
{
    [InspectorName("자신")] Self,
    [InspectorName("아군")] Ally,
    [InspectorName("적")] Enemy
}

/// <summary>
/// 선택한 대상을 기준으로 실제 효과를 받을 캐릭터입니다.
/// </summary>
public enum CardCombatEffectRecipient
{
    [InspectorName("카드 사용자")] User,
    [InspectorName("선택한 대상")] SelectedTarget
}

[System.Serializable]
public class CardCombatEffectData
{
    [Tooltip("이 효과를 사용할 행동 방식입니다.")]
    [InspectorName("사용 행동")]
    [SerializeField]
    private CardUseCost useCost = CardUseCost.Action;

    [Tooltip("이 카드를 놓을 수 있는 대상입니다.")]
    [InspectorName("카드를 놓을 대상")]
    [SerializeField]
    private CardCombatEffectTarget target = CardCombatEffectTarget.Enemy;

    [Tooltip("효과를 카드 사용자와 선택한 대상 중 누구에게 적용할지 정합니다.")]
    [InspectorName("효과 적용 대상")]
    [SerializeField]
    private CardCombatEffectRecipient recipient = CardCombatEffectRecipient.SelectedTarget;

    [Tooltip("실행할 전투 효과입니다.")]
    [InspectorName("효과 종류")]
    [SerializeField]
    private CardCombatEffectType effectType = CardCombatEffectType.Damage;

    [Tooltip("피해 효과일 때 적용할 피해 속성입니다. 총기처럼 원거리 공격이면 원거리 전투를 선택합니다.")]
    [InspectorName("피해 속성")]
    [SerializeField]
    private DamageType damageType = DamageType.Hand_to_hand_combat;

    [Tooltip("피해 효과일 때 적용할 타격·참격·관통·화염 형식입니다.")]
    [SerializeField]
    private DamageFormType damageForm = DamageFormType.Blunt;

    [Tooltip("상태 효과 부여를 선택했을 때 적용할 상태입니다.")]
    [InspectorName("부여할 상태")]
    [SerializeField]
    private StatusEffectType statusType = StatusEffectType.None;

    [Tooltip("주사위와 고정값을 합산해 효과 수치를 계산합니다.")]
    [InspectorName("효과 수치")]
    [SerializeField]
    private CardEffectValue value = new();

    public CardUseCost UseCost => useCost;
    public CardCombatEffectTarget Target => target;
    public CardCombatEffectRecipient Recipient => recipient;
    public CardCombatEffectType EffectType => effectType;
    public DamageType DamageType => damageType;
    public DamageFormType DamageForm => damageForm;
    public StatusEffectType StatusType => statusType;
    public int RollValue() => value != null ? value.Roll() : 0;

    public bool IsValid =>
        useCost != CardUseCost.None &&
        useCost != CardUseCost._Length &&
        effectType != CardCombatEffectType.None;
}

[System.Serializable]
public class CardAttackStyleData
{
    [Tooltip("능력치 판정을 사용하는 공격 방식의 우측 색상 영역 안에 표시할 아이콘입니다.")]
    [SerializeField]
    private Sprite icon;

    [Tooltip("선택창에 보여 줄 공격 방식 이름입니다. 예: 힘으로 내려찍기, 기술로 베기")]
    [SerializeField]
    private string displayName = "공격";

    [Tooltip("선택창에 표시할 간단한 설명입니다.")]
    [TextArea(1, 3)]
    [SerializeField]
    private string description;

    [Tooltip("피해에 보정치를 더할 능력치입니다.")]
    [SerializeField]
    private StatType scalingStat = StatType.Strength;

    [Tooltip("활성화하면 공격 전에 기존 이벤트와 같은 능력치 판정을 진행합니다.")]
    [InspectorName("능력치 판정 사용")]
    [SerializeField]
    private bool requiresStatCheck;

    [Tooltip("능력치 판정의 목표치입니다.")]
    [InspectorName("판정 목표치")]
    [SerializeField, Min(2)]
    private int checkTarget = 5;

    [Tooltip("근접·원거리 중 공격 거리 분류입니다.")]
    [SerializeField]
    private DamageType damageType = DamageType.Hand_to_hand_combat;

    [Tooltip("타격·참격·관통·화염 중 실제 피해 형식입니다.")]
    [SerializeField]
    private DamageFormType damageForm = DamageFormType.Blunt;

    [Tooltip("능력치 보정치와 합산할 기본 피해입니다.")]
    [SerializeField]
    private CardEffectValue damage = new();

    public string DisplayName => string.IsNullOrWhiteSpace(displayName)
        ? "공격"
        : displayName.Trim();
    public string Description => description;
    public Sprite Icon => icon;
    public StatType ScalingStat => scalingStat;
    public bool RequiresStatCheck => requiresStatCheck;
    public int CheckTarget => Mathf.Max(2, checkTarget);
    public DamageType DamageType => damageType;
    public DamageFormType DamageForm => damageForm;
    public int RollDamage() => damage != null ? damage.Roll() : 0;

    public bool IsValid =>
        scalingStat != StatType.None &&
        scalingStat != StatType._Length &&
        damageForm != DamageFormType._Length;
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

    [Header("필드 핸드 발동")]
    [Tooltip("핵심 키워드 카드가 필드 턴 시작 시 핸드에 있으면 발생시킬 이벤트입니다. 매 턴 발생시키려면 이벤트를 Repeatable로 설정하십시오.")]
    [SerializeField]
    private FieldEventData handFieldEvent;

    [Tooltip("핸드에 핵심 카드가 여러 종류 있으면 높은 우선순위의 이벤트 하나만 발생합니다.")]
    [SerializeField]
    private int handFieldEventPriority;

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

    [Header("키워드 카드 전투 효과")]
    [Tooltip("활성화하면 키워드의 고정 효과 대신 아래 목록에 등록한 효과를 사용합니다. 빈 목록은 전투 효과 없음을 뜻합니다.")]
    [InspectorName("키워드 전투 효과 직접 설정")]
    [SerializeField]
    private bool useCustomKeywordCombatEffects;

    [Tooltip("카드가 실행할 공격·자신·아군 효과를 각각 등록합니다.")]
    [InspectorName("전투 효과 목록")]
    [SerializeField]
    private List<CardCombatEffectData> keywordCombatEffects = new();

    [Header("공격 방식 선택")]
    [Tooltip("등록한 방식이 있으면 적을 공격할 때 선택지로 표시됩니다.")]
    [SerializeField]
    private List<CardAttackStyleData> attackStyles = new();

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

    public FieldEventData HandFieldEvent => handFieldEvent;

    public int HandFieldEventPriority => handFieldEventPriority;

    public bool UsesCustomKeywordCombatEffects => useCustomKeywordCombatEffects;

    public IReadOnlyList<CardCombatEffectData> KeywordCombatEffects => keywordCombatEffects;

    public IReadOnlyList<CardAttackStyleData> AttackStyles => attackStyles;

    public bool HasAttackStyles => attackStyles != null && attackStyles.Count > 0;

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

    public bool HasConfiguredCombatEffect(
        CardUseCost useCost,
        CardCombatEffectTarget target)
    {
        if (!useCustomKeywordCombatEffects || keywordCombatEffects == null)
            return false;

        foreach (CardCombatEffectData effect in keywordCombatEffects)
        {
            if (effect != null && effect.IsValid &&
                effect.UseCost == useCost && effect.Target == target)
            {
                return true;
            }
        }

        return false;
    }

    public CardAttackStyleData GetAttackStyle(int index)
    {
        if (attackStyles == null || index < 0 || index >= attackStyles.Count)
            return null;

        CardAttackStyleData style = attackStyles[index];
        return style != null && style.IsValid ? style : null;
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

        if (keywordCombatEffects == null)
        {
            keywordCombatEffects = new List<CardCombatEffectData>();
        }

        if (attackStyles == null)
        {
            attackStyles = new List<CardAttackStyleData>();
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
