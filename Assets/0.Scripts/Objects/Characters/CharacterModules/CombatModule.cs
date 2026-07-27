using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatModule : CharacterModule
{

    public sealed override System.Type RegistrationType => typeof(CombatModule);

    protected HitpointModules hitpointModule;
   
    private HitpointModules hp;

    public override void OnRegistration(CharacterBase owner)
    {
        base.OnRegistration(owner);
        hp = owner.GetModule<HitpointModules>();
    }
   


    public void OnHit(DamageStruct damageInfo)
    {
        if (Owner == null)
        {
            Debug.LogError("피해 처리 실패: CombatModule Owner 없음");
            return;
        }

        HitpointModules hp = Owner.GetModule<HitpointModules>();

        if (hp == null)
        {
            Debug.LogError($"{Owner.name}: HitpointModules 없음. 피해 처리 불가");
            return;
        }
        
        CharacterBase attacker = GetAttacker(damageInfo);

        int finalDamage = Mathf.Max(0, damageInfo.damageAmount);

        int diceValue = damageInfo.diceValue;

        // 로그에 필요한 임시 기록
        int logDiceValue = damageInfo.diceValue;
        
        int armorReduction = 0;

        bool shouldCounter = false;
        bool damageNegated = false;



        Debug.Log($"{Owner.name} 피격 시작 / " + $"기본 피해:{finalDamage} / " + $"타입:{damageInfo.damageType} / " +
                  $"대응:{damageInfo.reactionType}");

        // 1. 공격자의 버프·디버프 적용
        finalDamage = ApplyPrimaryDamageModifier(attacker, Owner, finalDamage, damageInfo.damageType);

        finalDamage = Mathf.Max(0, finalDamage);

        Debug.Log($"1차 피해 보정 후: {finalDamage}");

        // 2. 대응 적용
        switch (damageInfo.reactionType)
        {
            case ActionType.Guard:
                finalDamage =
                    ApplyGuardReaction(
                        Owner,
                        finalDamage);
                break;

            case ActionType.Evade:
                if (TryEvadeReaction(Owner, finalDamage))
                {
                    damageNegated = true;
                    finalDamage = 0;

                    Debug.Log(
                        $"{Owner.name}: 회피 성공 / 피해 무효");
                }
                else
                {
                    Debug.Log(
                        $"{Owner.name}: 회피 실패");
                }

                break;

            case ActionType.Counterattack:
                shouldCounter = attacker != null;
                break;

            case ActionType.None:
            default:
                break;
        }
        
        finalDamage = Mathf.Max(0, finalDamage);

        // 회피 성공 시 장갑과 받는 피해 보정을 적용하지 않음
        if (!damageNegated)
        {
            // 3. 장갑 적용
            int beforeArmorDamage = finalDamage;

            finalDamage = ApplyArmorReduction(Owner, finalDamage, damageInfo.damageType);

            finalDamage = Mathf.Max(0, finalDamage);

            armorReduction = Mathf.Max(0, beforeArmorDamage - finalDamage);

            Debug.Log($"장갑 감소:{armorReduction} / " + $"{beforeArmorDamage} → {finalDamage}");

            // 4. 받는 피해 최종 보정
            finalDamage = ApplyFinalIncomingDamageModifier(Owner, finalDamage, damageInfo.damageType);

            finalDamage = Mathf.Max(0, finalDamage);
        }

        int totalModifier = finalDamage + armorReduction - diceValue;

        // 계산 정보가 살아 있는 동안 로그 전송
        WriteCardRollLog(damageInfo);

        // 5. HP 감소
        damageInfo.damageAmount = finalDamage;

        damageInfo.armorReduction = armorReduction;

        hp.TakeDamage(damageInfo);

        BattleManager.ClaimBattleLog($"{Owner.name} 최종 피해 적용: {finalDamage}");

        // 6. 반격
        if (shouldCounter)
        {
            ResolveCounterDamage(Owner, attacker);
        }

    }

    private void WriteCardRollLog(DamageStruct damageInfo)
    {
        if (damageInfo.diceValue <= 0)
            return;

        int modifier = damageInfo.damageAmount - damageInfo.diceValue;

        if (damageInfo.highCritical)
        {
            BattleManager.ClaimBattleLog($"상위 크리티컬 ({damageInfo.diceValue})");
        }
        else if (damageInfo.critical)
        {
            BattleManager.ClaimBattleLog($"크리티컬 ({damageInfo.diceValue})");
        }

        string modifierText;

        if (modifier >= 0)
        {
            modifierText = $"+ 보정치 {modifier}";
        }
        else
        {
            modifierText = $"- 보정치 {Mathf.Abs(modifier)}";
        }

        BattleManager.ClaimBattleLog($"주사위 {damageInfo.diceValue} " + $"{modifierText} " + $"= {damageInfo.damageAmount}");
    }




    private CharacterBase GetAttacker(DamageStruct damageInfo)
    {
        if (damageInfo.from == null)
            return null;

        return damageInfo.from.GetComponent<CharacterBase>();
    }

    private int ApplyPrimaryDamageModifier(CharacterBase attacker, CharacterBase defender, int damage, DamageType damageType)
    {
        int result = damage;

        if (attacker != null)
        {
            StatusEffectModule attackerStatus = attacker.GetModule<StatusEffectModule>();

            if (attackerStatus != null)
            {
                result = attackerStatus.ModifyOutgoingDamage(result);
            }
        }

        return result;
    }

    private int ApplyGuardReaction(CharacterBase defender, int damage)
    {
        int strengthBonus = 0;

        StatModules stat = defender.GetModule<StatModules>();

        if (stat != null)
            strengthBonus = stat.GetModifier(StatType.Strength);

        int guardValue = Dice.RollD10() + strengthBonus;

        int result = Mathf.Max(0, damage - guardValue);

        Debug.Log(
            $"{defender.name}: 방어 대응 / 감소량:{guardValue} / {damage} → {result}"
        );

        return result;
    }

    private bool TryEvadeReaction(CharacterBase defender, int damage)
    {
        int agilityBonus = 0;
        int hasteEvadeBonus = 0;

        StatModules stat = defender.GetModule<StatModules>();

        if (stat != null)
            agilityBonus = stat.GetModifier(StatType.Agility);

        StatusEffectModule status = defender.GetModule<StatusEffectModule>();

        if (status != null)
            hasteEvadeBonus = status.GetEvadeBonusByHaste();

        int dice = Dice.RollD10();

        int evadeValue = dice + agilityBonus + hasteEvadeBonus;

        Debug.Log(
            $"{defender.name}: 회피 판정 / 피해:{damage} / " +
            $"주사위:{dice} / 민첩 보정:{agilityBonus} / " +
            $"가속 보정:{hasteEvadeBonus} / 총회피값:{evadeValue}"
        );

        return evadeValue >= damage;
    }

    private int ApplyArmorReduction(CharacterBase defender, int damage, DamageType damageType)
    {
        ArmorModule armor = defender.GetModule<ArmorModule>();

        if (armor == null)
            return damage;

        return armor.GetReducedDamage(
            damage,
            damageType
        );
    }

    private int ApplyFinalIncomingDamageModifier(CharacterBase defender, int damage, DamageType damageType)
    {
        int result = damage;

        StatusEffectModule defenderStatus = defender.GetModule<StatusEffectModule>();

        if (defenderStatus != null)
        {
            result =
                defenderStatus.ModifyIncomingDamage(
                    result,
                    damageType
                );
        }

        return result;
    }

    private void ResolveCounterDamage(CharacterBase defender, CharacterBase attacker)
    {
        if (defender == null || attacker == null)
            return;

        CombatModule attackerCombat = attacker.GetModule<CombatModule>();

        if (attackerCombat == null)
        {
            Debug.LogWarning($"{attacker.name}: CombatModule 없음. 반격 실패");
            return;
        }

        int healthBonus = 0;

        StatModules stat = defender.GetModule<StatModules>();

        if (stat != null)
            healthBonus = stat.GetModifier(StatType.Health);

        int counterDice = Dice.RollD8();

        DamageStruct counterDamageInfo = new DamageStruct
        {
            from = defender.gameObject,
            instigator = defender.Controller,

            diceValue = counterDice,

            damageAmount = Mathf.Max(0, counterDice + healthBonus),

            critical = false,
            highCritical = false,

            damageType = DamageType.Hand_to_hand_combat,

            canCounter = false,
            reactionType = ActionType.None
        };

        Debug.Log($"{defender.name}: 반격 발동 → {attacker.name} / " + $"주사위:{counterDice} + 건강 보정:{healthBonus} " +
            $"= {counterDamageInfo.damageAmount}");

        attackerCombat.OnHit(counterDamageInfo);
    }

    public void OnRestore(in RestoreStruct restoreInfo)
    {
        if (hp == null)
            return;

        hp.TakeRestore(restoreInfo);
    }
}
