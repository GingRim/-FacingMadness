using System;
using UnityEngine;

public class HasteStatus : StatusEffectHandler
{
    public override StatusEffectType Type => StatusEffectType.Haste;
    public override int MaxStack => 6;

    public override int GetInitiativeBonus(int stack)
    {
        return stack * 5;
    }

    public override int ReduceDamage(int damage, DamageType damageType, int stack)
    {
        if (damageType == DamageType.Magic)
            return damage;

        int reduce = (stack / 2) * 2;

        return UnityEngine.Mathf.Max(0, damage - reduce);
    }

    public override void OnTurnEnd()
    {
        owner.ReduceStatus(StatusEffectType.Haste, 1);
    }

    public override void OnBattleEnd()
    {
        owner.ClearStatus(StatusEffectType.Haste);
    }
}
