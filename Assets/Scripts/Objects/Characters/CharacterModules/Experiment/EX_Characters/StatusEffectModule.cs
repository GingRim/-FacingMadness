using System;
using UnityEngine;

public class StatusEffectModule : CharacterModule
{
    public sealed override System.Type RegistrationType
        => typeof(StatusEffectModule);

    [SerializeField]
    private int[] stacks = new int[(int)StatusEffectType._Length];

    private StatusEffectHandler[] handlers;

    private void Awake()
    {
        EnsureStackSize();
        InitializeHandlers();
    }

    private void EnsureStackSize()
    {
        int needSize = (int)StatusEffectType._Length;

        if (stacks == null)
        {
            stacks = new int[needSize];
            return;
        }

        if (stacks.Length == needSize)
            return;

        int[] newStacks = new int[needSize];

        int copyLength = Mathf.Min(stacks.Length, newStacks.Length);

        for (int i = 0; i < copyLength; i++)
        {
            newStacks[i] = stacks[i];
        }

        stacks = newStacks;

        Debug.Log($"StatusEffect stacks 배열 크기 보정: {copyLength} -> {needSize}");
    }

    private void InitializeHandlers()
    {
        handlers = new StatusEffectHandler[(int)StatusEffectType._Length];

        RegisterHandler(new HasteStatus());
        // RegisterHandler(new BlessingStatus());
        RegisterHandler(new Motivation());
        // RegisterHandler(new BindStatus());
    }

    
    private void RegisterHandler(StatusEffectHandler handler)
    {
        handler.Initialize(this);
        handlers[(int)handler.Type] = handler;
    }

    
    public void AddStatus(StatusEffectType type, int value)
    {
        if (!IsValidType(type) || value <= 0)
            return;
        
        value = OffsetStatus(type, value);

        if (value <= 0)
            return;

        int index = (int)type;
        int maxStack = GetMaxStack(type);

        stacks[index] = Mathf.Min(maxStack, stacks[index] + value);

        Debug.Log($"{type} {value}중첩 부여 / 현재 {stacks[index]}중첩");
    }


    public void ReduceStatus(StatusEffectType type, int value)
    {
        if (!IsValidType(type) || value <= 0)
            return;

        int index = (int)type;

        if (stacks[index] <= 0)
            return;

        int before = stacks[index];

        stacks[index] = Mathf.Max(0, stacks[index] - value);

        Debug.Log(
            $"{type} 감소: {before} -> {stacks[index]}");

        if (stacks[index] == 0)
        {
            Debug.Log($"{type} 해제");
        }
    }


    public void ClearStatus(StatusEffectType type)
    {
        EnsureStackSize();

        if (!IsValidType(type))
            return;

        stacks[(int)type] = 0;
    }


    public int GetStack(StatusEffectType type)
    {
        if (!IsValidType(type))
            return 0;

        return stacks[(int)type];
    }


    public int GetJudgeBonus()
    {
        if (HasStatus(StatusEffectType.Motivation))
        return 2;
        

        if (HasStatus(StatusEffectType.Lethargy))
        return -2;

        return 0;
    }


    public void ConsumeJudgeStatus()
    {
        if (GetStack(StatusEffectType.Motivation) > 0)
        {
            ReduceStatus(StatusEffectType.Motivation, 1);
        }

        if (GetStack(StatusEffectType.Lethargy) > 0)
        {
            ReduceStatus(StatusEffectType.Lethargy, 1);
        }
    }

    
    public int GetInitiativeBonus()
    {
        int bonus = 0;

        if (HasStatus(StatusEffectType.Haste))
        {
            bonus += GetStack(StatusEffectType.Haste) * 5;
        }

        if (HasStatus(StatusEffectType.Bind))
        {
            bonus -= GetStack(StatusEffectType.Bind) * 5;
        }

        return bonus;
    }


    public int ReduceDamageByStatus(int damage, DamageType damageType)
    {
        int finalDamage = damage;

        int hasteStack = GetStack(StatusEffectType.Haste);

        if (hasteStack > 0 &&
            damageType != DamageType.Magic)
        {
            int reduceAmount = (hasteStack / 2) * 2;

            finalDamage = Mathf.Max(0, finalDamage - reduceAmount);

            Debug.Log($"가속 피해 감소: 가속 {hasteStack} / 감소 {reduceAmount} / 피해 {finalDamage}");
        }

        return finalDamage;
    }

    
    private int GetMaxStack(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Haste:
                return 6;

            case StatusEffectType.Bind:
            case StatusEffectType.Motivation:
            case StatusEffectType.Lethargy:
                return 5;

            case StatusEffectType.Vulnerable:
            case StatusEffectType.Doom:
                return 10;

            case StatusEffectType.Blessing:
            case StatusEffectType.Curse:
            case StatusEffectType.Stun:
            case StatusEffectType.DrawBlock:
                return 1;

            default:
                return 0;
        }
    }

    
    private bool IsValidType(StatusEffectType type)
    {
        return type > StatusEffectType.None && type < StatusEffectType._Length;
    }
    
    
    public bool HasStatus(StatusEffectType type)
    {
        if (!IsValidType(type))
            return false;

        return stacks[(int)type] > 0;
    }


    private int OffsetStatus(StatusEffectType type, int value)
    {
        StatusEffectType oppositeType = GetOppositeType(type);

        if (oppositeType == StatusEffectType.None)
            return value;

        int oppositeStack = GetStack(oppositeType);

        if (oppositeStack <= 0)
            return value;

        int offsetAmount = Mathf.Min(value, oppositeStack);

        ReduceStatus(oppositeType, offsetAmount);

        int remainValue = value - offsetAmount;

        Debug.Log($"{type}이 {oppositeType}과 {offsetAmount} 상쇄됨 / 남은 {type}: {remainValue}");

        return remainValue;
    }

    private StatusEffectType GetOppositeType(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Haste:
                return StatusEffectType.Bind;

            case StatusEffectType.Bind:
                return StatusEffectType.Haste;

            case StatusEffectType.Motivation:
                return StatusEffectType.Lethargy;

            case StatusEffectType.Lethargy:
                return StatusEffectType.Motivation;

            case StatusEffectType.Blessing:
                return StatusEffectType.Curse;

            case StatusEffectType.Curse:
                return StatusEffectType.Blessing;

            default:
                return StatusEffectType.None;
        }
    }


    public int RollJudgeDice()
    {
        bool hasBlessing = HasStatus(StatusEffectType.Blessing);
        bool hasCurse = HasStatus(StatusEffectType.Curse);

        if (hasBlessing)
        {
            int first = Dice.RollD10();
            int second = Dice.RollD10();
            int result = Mathf.Max(first, second);

            Debug.Log($"축복 판정: {first}, {second} 중 높은 값 {result}");

            return result;
        }

        if (hasCurse)
        {
            int first = Dice.RollD10();
            int second = Dice.RollD10();
            int result = Mathf.Min(first, second);

            Debug.Log($"저주 판정: {first}, {second} 중 낮은 값 {result}");

            return result;
        }

        return Dice.RollD10();
    }
   
    
    /// <summary>
    /// 의욕 및 무기력
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public int ModifyOutgoingDamage(int damage)
    {
        if (HasStatus(StatusEffectType.Motivation))
        {
            damage += 2;
        }

        if (HasStatus(StatusEffectType.Lethargy))
        {
            damage -= 2;
        }

        return Mathf.Max(0, damage);
    }

    
    public int ModifyIncomingDamage(int damage, DamageType damageType)
    {
        // 취약: 받는 피해 +2 / 중첩
        if (HasStatus(StatusEffectType.Vulnerable))
        {
            damage += GetStack(StatusEffectType.Vulnerable) * 2;
        }

        // 가속: 2중첩당 받는 물리 피해 -2
        if (damageType == DamageType.Physical && HasStatus(StatusEffectType.Haste))
        {
            int hasteStack = GetStack(StatusEffectType.Haste);
            damage -= (hasteStack / 2) * 2;
        }

        return Mathf.Max(0, damage);
    }


    /// <summary>
    /// 종언 부여
    /// </summary>
    /// <param name="value"></param>
    public void AddDoom(int value)
    {
        if (value <= 0)
            return;

        int index = (int)StatusEffectType.Doom;

        stacks[index] = Mathf.Clamp(value, 1, GetMaxStack(StatusEffectType.Doom));

        Debug.Log($"종언 {stacks[index]} 부여");
    }

    
    /// <summary>
    /// 종언 전용 인 카운트
    /// </summary>
    public void TickDoom()
    {
        if (!HasStatus(StatusEffectType.Doom))
            return;

        int index = (int)StatusEffectType.Doom;

        stacks[index]--;

        Debug.Log($"종언 감소 / 현재 {stacks[index]}");

        if (stacks[index] <= 0)
        {
            stacks[index] = 0;
            ApplyDoomDeath();
        }
    }

    
    /// <summary>
    /// 종언 즉사 효과
    /// </summary>
    private void ApplyDoomDeath()
    {
        CharacterBase character = GetComponent<CharacterBase>();

        if (character == null)
            return;

        CombatModule combat = character.GetModule<CombatModule>();

        if (combat == null)
            return;

        int damage = 999999;

        DamageStruct damageInfo = new DamageStruct
        {
            from = gameObject,
            instigator = character.Controller,
            damageAmount = damage,
            critical = false,
            damageType = DamageType.Magic
        };

        combat.OnHit(damageInfo);

        Debug.Log("종언 발동: 즉사");
    }

    public bool ConsumeDrawBlock()
    {
        if (!HasStatus(StatusEffectType.DrawBlock))
            return false;

        ClearStatus(StatusEffectType.DrawBlock);

        Debug.Log("드로우 제한으로 인해 드로우가 취소됨");

        return true;
    }

    public bool CanDraw()
    {
        return !HasStatus(StatusEffectType.DrawBlock);
    }

    /// <summary>
    /// 턴 종료 시 처리되는 상태 이상.
    /// </summary>
    public void OnTurnEnd()
    {
        Debug.Log("StatusEffectModule OnTurnEnd 실행");

        ReduceStatus(StatusEffectType.Haste, 1);
        ReduceStatus(StatusEffectType.Bind, 1);

        TickDoom();
    }

    public void OnRoundEnd()
    {
        Debug.Log("StatusEffectModule OnRoundEnd 실행");

        ClearStatus(StatusEffectType.Vulnerable);
        ClearStatus(StatusEffectType.Blessing);
        ClearStatus(StatusEffectType.Curse);
    }
}
