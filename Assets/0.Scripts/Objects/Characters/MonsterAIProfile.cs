using System;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterAIActionType
{
    [InspectorName("공격")]
    Attack,
    [InspectorName("자신 회복")]
    HealSelf,
    [InspectorName("임시 장갑 획득")]
    GainTemporaryArmor,
    [InspectorName("자신에게 상태 부여")]
    ApplyStatusToSelf,
    [InspectorName("적에게 상태 부여")]
    ApplyStatusToEnemy,
    [InspectorName("턴 종료")]
    EndTurn
}

public enum MonsterAITargetType
{
    [InspectorName("자신")]
    Self,
    [InspectorName("첫 번째 적")]
    FirstEnemy,
    [InspectorName("무작위 적")]
    RandomEnemy,
    [InspectorName("체력이 가장 낮은 적")]
    LowestHealthEnemy
}

public enum MonsterAIConditionType
{
    [InspectorName("항상")]
    Always,
    [InspectorName("자신 체력이 지정 비율 이하")]
    SelfHealthAtMostPercent,
    [InspectorName("자신 체력이 지정 비율 이상")]
    SelfHealthAtLeastPercent,
    [InspectorName("대상 체력이 지정 비율 이하")]
    TargetHealthAtMostPercent,
    [InspectorName("대상 체력이 지정 비율 이상")]
    TargetHealthAtLeastPercent,
    [InspectorName("자신이 지정 상태 보유")]
    SelfHasStatus,
    [InspectorName("자신이 지정 상태 미보유")]
    SelfMissingStatus,
    [InspectorName("대상이 지정 상태 보유")]
    TargetHasStatus,
    [InspectorName("대상이 지정 상태 미보유")]
    TargetMissingStatus,
    [InspectorName("현재 라운드가 지정값 이상")]
    RoundAtLeast,
    [InspectorName("현재 라운드가 지정값 이하")]
    RoundAtMost
}

public enum MonsterAIDiceType
{
    [InspectorName("주사위 없음")]
    None,
    [InspectorName("1D4")]
    D4,
    [InspectorName("1D6")]
    D6,
    [InspectorName("1D8")]
    D8,
    [InspectorName("1D10")]
    D10,
    [InspectorName("1D12")]
    D12,
    [InspectorName("1D20")]
    D20
}

[Serializable]
public class MonsterAICondition
{
    [InspectorName("조건 종류")]
    [Tooltip("이 조건이 검사할 내용입니다.")]
    [SerializeField] private MonsterAIConditionType conditionType;
    [InspectorName("기준 체력 비율")]
    [Tooltip("체력 비율 조건에서 사용하는 기준값입니다. 50이면 최대 체력의 50%입니다.")]
    [SerializeField, Range(0, 100)] private int percent = 50;
    [InspectorName("기준 라운드")]
    [Tooltip("라운드 조건에서 사용하는 기준 라운드입니다.")]
    [SerializeField, Min(0)] private int round = 1;
    [InspectorName("검사할 상태")]
    [Tooltip("상태 보유 여부 조건에서 검사할 상태입니다.")]
    [SerializeField] private StatusEffectType statusType;

    public bool IsMet(CharacterBase self, CharacterBase target, BattleManager battle)
    {
        switch (conditionType)
        {
            case MonsterAIConditionType.Always:
                return true;

            case MonsterAIConditionType.SelfHealthAtMostPercent:
                return GetHealthPercent(self) <= percent;

            case MonsterAIConditionType.SelfHealthAtLeastPercent:
                return GetHealthPercent(self) >= percent;

            case MonsterAIConditionType.TargetHealthAtMostPercent:
                return GetHealthPercent(target) <= percent;

            case MonsterAIConditionType.TargetHealthAtLeastPercent:
                return GetHealthPercent(target) >= percent;

            case MonsterAIConditionType.SelfHasStatus:
                return HasStatus(self, statusType);

            case MonsterAIConditionType.SelfMissingStatus:
                return !HasStatus(self, statusType);

            case MonsterAIConditionType.TargetHasStatus:
                return HasStatus(target, statusType);

            case MonsterAIConditionType.TargetMissingStatus:
                return !HasStatus(target, statusType);

            case MonsterAIConditionType.RoundAtLeast:
                return battle != null && battle.Round >= round;

            case MonsterAIConditionType.RoundAtMost:
                return battle != null && battle.Round <= round;

            default:
                return false;
        }
    }

    private static int GetHealthPercent(CharacterBase character)
    {
        HitpointModules hp = character != null
            ? character.GetModule<HitpointModules>()
            : null;

        if (hp == null || hp.Max <= 0)
            return 0;

        return Mathf.RoundToInt(hp.Current * 100f / hp.Max);
    }

    private static bool HasStatus(CharacterBase character, StatusEffectType type)
    {
        StatusEffectModule status = character != null
            ? character.GetModule<StatusEffectModule>()
            : null;

        return status != null && status.HasStatus(type);
    }
}

[Serializable]
public class MonsterAIAction
{
    [Header("기본 정보")]
    [InspectorName("행동 식별자")]
    [Tooltip("행동을 구분하는 고유 식별자입니다. 같은 프로필 안에서 겹치지 않게 작성합니다.")]
    [SerializeField] private string actionId;
    [InspectorName("표시 이름")]
    [Tooltip("전투 로그에 표시할 행동 이름입니다.")]
    [SerializeField] private string displayName = "행동";
    [InspectorName("행동 종류")]
    [Tooltip("실제로 실행할 행동의 종류입니다.")]
    [SerializeField] private MonsterAIActionType actionType;
    [InspectorName("대상 선택 방식")]
    [Tooltip("행동을 적용할 대상을 정하는 방식입니다.")]
    [SerializeField] private MonsterAITargetType targetType = MonsterAITargetType.FirstEnemy;

    [Header("선택 규칙")]
    [InspectorName("우선순위")]
    [Tooltip("사용 가능한 행동 중 숫자가 가장 높은 우선순위만 후보가 됩니다.")]
    [SerializeField] private int priority;
    [InspectorName("선택 가중치")]
    [Tooltip("우선순위가 같은 후보끼리 무작위 선택할 때의 비중입니다. 값이 클수록 선택될 가능성이 높습니다.")]
    [SerializeField, Min(1)] private int weight = 1;
    [InspectorName("행동력 비용")]
    [Tooltip("이 행동을 한 번 실행할 때 소비하는 행동력입니다.")]
    [SerializeField, Min(0)] private int actionPointCost = 1;
    [InspectorName("턴당 최대 사용 횟수")]
    [Tooltip("한 턴에 사용할 수 있는 최대 횟수입니다. 0이면 제한하지 않습니다.")]
    [SerializeField, Min(0)] private int maxUsesPerTurn = 1;
    [InspectorName("전투당 최대 사용 횟수")]
    [Tooltip("한 전투에서 사용할 수 있는 최대 횟수입니다. 0이면 제한하지 않습니다.")]
    [SerializeField, Min(0)] private int maxUsesPerBattle;
    [InspectorName("재사용 대기 라운드")]
    [Tooltip("사용 후 다시 사용할 수 없게 막는 라운드 수입니다. 0이면 쿨다운이 없습니다.")]
    [SerializeField, Min(0)] private int cooldownRounds;
    [InspectorName("사용 조건")]
    [Tooltip("행동을 후보로 올리기 위해 모두 충족해야 하는 조건 목록입니다. 비어 있으면 조건 없이 사용합니다.")]
    [SerializeField] private List<MonsterAICondition> conditions = new();

    [Header("효과 값")]
    [InspectorName("고정값")]
    [Tooltip("주사위와 능력치 보정에 더하는 고정 수치입니다.")]
    [SerializeField] private int fixedValue;
    [InspectorName("주사위 개수")]
    [Tooltip("굴릴 주사위의 개수입니다.")]
    [SerializeField, Min(0)] private int diceCount = 1;
    [InspectorName("주사위 종류")]
    [Tooltip("효과 수치를 계산할 때 사용하는 주사위 종류입니다.")]
    [SerializeField] private MonsterAIDiceType diceType = MonsterAIDiceType.D10;
    [InspectorName("보정 능력치")]
    [Tooltip("효과 수치에 보정치를 더할 능력치입니다.")]
    [SerializeField] private StatType modifierStat = StatType.Strength;
    [InspectorName("부여할 상태")]
    [Tooltip("상태 부여 행동으로 적용할 상태입니다.")]
    [SerializeField] private StatusEffectType statusType;
    [InspectorName("피해 종류")]
    [Tooltip("공격 행동이 입히는 피해 종류입니다.")]
    [SerializeField] private DamageType damageType = DamageType.Hand_to_hand_combat;
    [InspectorName("피해 형식")]
    [Tooltip("공격이 입히는 타격·참격·관통·화염 형식입니다.")]
    [SerializeField] private DamageFormType damageForm = DamageFormType.Blunt;
    [InspectorName("반격 허용")]
    [Tooltip("공격 대상이 반격 대응을 선택할 수 있는지 결정합니다.")]
    [SerializeField] private bool canCounter = true;
    [InspectorName("치명타 허용")]
    [Tooltip("공격 시 몬스터의 민첩 판정으로 치명타를 발생시킬 수 있는지 결정합니다.")]
    [SerializeField] private bool allowCritical = true;

    public string ActionId => actionId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? actionType.ToString() : displayName;
    public MonsterAIActionType ActionType => actionType;
    public MonsterAITargetType TargetType => targetType;
    public int Priority => priority;
    public int Weight => Mathf.Max(1, weight);
    public int ActionPointCost => Mathf.Max(0, actionPointCost);
    public int MaxUsesPerTurn => Mathf.Max(0, maxUsesPerTurn);
    public int MaxUsesPerBattle => Mathf.Max(0, maxUsesPerBattle);
    public int CooldownRounds => Mathf.Max(0, cooldownRounds);
    public IReadOnlyList<MonsterAICondition> Conditions => conditions;
    public int FixedValue => fixedValue;
    public int DiceCount => Mathf.Max(0, diceCount);
    public MonsterAIDiceType DiceType => diceType;
    public StatType ModifierStat => modifierStat;
    public StatusEffectType StatusType => statusType;
    public DamageType DamageType => damageType;
    public DamageFormType DamageForm => damageForm;
    public bool CanCounter => canCounter;
    public bool AllowCritical => allowCritical;
}

[Serializable]
public class MonsterAIReaction
{
    [Header("기본 정보")]
    [InspectorName("대응 식별자")]
    [Tooltip("대응을 구분하는 고유 식별자입니다. 같은 프로필 안에서 겹치지 않게 작성합니다.")]
    [SerializeField] private string reactionId;
    [InspectorName("대응 종류")]
    [Tooltip("피격 전에 선택할 대응 종류입니다. 방어, 회피, 반격을 사용할 수 있습니다.")]
    [SerializeField] private ActionType reactionType = ActionType.Guard;
    [InspectorName("우선순위")]
    [Tooltip("사용 가능한 대응 중 숫자가 가장 높은 우선순위만 후보가 됩니다.")]
    [SerializeField] private int priority;
    [InspectorName("선택 가중치")]
    [Tooltip("우선순위가 같은 후보끼리 무작위 선택할 때의 비중입니다.")]
    [SerializeField, Min(1)] private int weight = 1;
    [InspectorName("발동 확률")]
    [Tooltip("조건을 충족한 뒤 이 대응을 시도할 확률입니다.")]
    [SerializeField, Range(0, 100)] private int chancePercent = 100;
    [InspectorName("행동력 비용")]
    [Tooltip("이 대응을 한 번 실행할 때 소비하는 행동력입니다.")]
    [SerializeField, Min(0)] private int actionPointCost = 1;
    [InspectorName("라운드당 최대 사용 횟수")]
    [Tooltip("한 라운드에 사용할 수 있는 최대 횟수입니다. 0이면 제한하지 않습니다.")]
    [SerializeField, Min(0)] private int maxUsesPerRound = 1;
    [InspectorName("능력치 판정 사용")]
    [Tooltip("확률과 조건 외에 1D100 능력치 이하 판정도 요구할지 결정합니다.")]
    [SerializeField] private bool requiresStatRoll;
    [InspectorName("판정 능력치")]
    [Tooltip("능력치 판정을 사용할 경우 1D100 결과와 비교할 능력치입니다.")]
    [SerializeField] private StatType rollUnderStat = StatType.Intelligence;
    [InspectorName("발동 조건")]
    [Tooltip("대응을 후보로 올리기 위해 모두 충족해야 하는 조건 목록입니다.")]
    [SerializeField] private List<MonsterAICondition> conditions = new();

    public string ReactionId => reactionId;
    public ActionType ReactionType => reactionType;
    public int Priority => priority;
    public int Weight => Mathf.Max(1, weight);
    public int ChancePercent => Mathf.Clamp(chancePercent, 0, 100);
    public int ActionPointCost => Mathf.Max(0, actionPointCost);
    public int MaxUsesPerRound => Mathf.Max(0, maxUsesPerRound);
    public bool RequiresStatRoll => requiresStatRoll;
    public StatType RollUnderStat => rollUnderStat;
    public IReadOnlyList<MonsterAICondition> Conditions => conditions;
}

[CreateAssetMenu(fileName = "NewMonsterAIProfile", menuName = "Monster/AI Profile")]
public class MonsterAIProfile : ScriptableObject
{
    [Header("턴 행동 제한")]
    [InspectorName("턴당 전체 행동 수")]
    [Tooltip("몬스터가 자기 턴에 실행할 수 있는 전체 행동 수의 상한입니다.")]
    [SerializeField, Min(1)] private int maxActionsPerTurn = 1;

    [Header("행동 목록")]
    [InspectorName("행동")]
    [Tooltip("몬스터가 자기 턴에 조건과 우선순위를 확인하여 선택할 행동들입니다.")]
    [SerializeField] private List<MonsterAIAction> actions = new();

    [Header("대응 목록")]
    [InspectorName("대응")]
    [Tooltip("몬스터가 공격받을 때 조건과 우선순위를 확인하여 선택할 대응들입니다.")]
    [SerializeField] private List<MonsterAIReaction> reactions = new();

    public int MaxActionsPerTurn => Mathf.Max(1, maxActionsPerTurn);
    public IReadOnlyList<MonsterAIAction> Actions => actions;
    public IReadOnlyList<MonsterAIReaction> Reactions => reactions;
}
