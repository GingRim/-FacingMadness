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
        InitializeHandlers();
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

        if (TryOffset(type, ref value))
        {
            if(value <= 0)
            return;
        }

        int index = (int)type;

        stacks[index] = Mathf.Clamp(stacks[index] + value, 0, GetMaxStack(type));

        Debug.Log($"{type} {value} 획득 / 현재 {type}: {stacks[index]}");
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
        int bonus = 0;

        if (HasStatus(StatusEffectType.Motivation))
        {
            bonus += 2;
        }

        if (HasStatus(StatusEffectType.Lethargy))
        {
            bonus -= 2;
        }

        return bonus;
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
        int hasteStack = GetStack(StatusEffectType.Haste);

        return hasteStack * 5;
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
                return 10;

            case StatusEffectType.Blessing:
            case StatusEffectType.Curse:
            case StatusEffectType.Stun:
                return 1;

            default:
                return 0;
        }
    }

    private bool IsValidType(StatusEffectType type)
    {
        return type > StatusEffectType.None &&
               type < StatusEffectType._Length;
    }
    public bool HasStatus(StatusEffectType type)
    {
        return GetStack(type) > 0;
    }
    private bool TryOffset(StatusEffectType type, ref int value)
    {
        StatusEffectType oppositeType = GetOppositeType(type);

        if (oppositeType == StatusEffectType.None)
            return false;

        int oppositeStack = GetStack(oppositeType);

        if (oppositeStack <= 0)
            return false;

        int offsetAmount = Mathf.Min(value, oppositeStack);

        ReduceStatus(oppositeType, offsetAmount);

        value -= offsetAmount;

        Debug.Log(
            $"{type}이 {oppositeType}과 {offsetAmount} 상쇄됨 / 남은 {type}: {value}");

        return true;
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

}
