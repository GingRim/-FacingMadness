using System.Collections.Generic;
using UnityEngine;

public class MonsterAIModule : CharacterModule
{
    public sealed override System.Type RegistrationType => typeof(MonsterAIModule);

    public void ExecuteTurn(BattleManager battle)
    {
        if (Owner == null)
        {
            Debug.LogWarning("MonsterAI 실행 실패: Owner 없음");
            return;
        }

        if (battle == null)
        {
            Debug.LogWarning($"{Owner.name}: BattleManager 없음");
            return;
        }

        CharacterBase target = FindTarget(battle);

        if (target == null)
        {
            Debug.LogWarning($"{Owner.name}: 공격 대상 없음. 턴 종료");
            battle.EndTurn();
            return;
        }

        BasicAttack(target);

        battle.EndTurn();
    }

    private CharacterBase FindTarget(BattleManager battle)
    {
        List<CharacterBase> enemies =
            battle.GetEnemiesOf(Owner);

        if (enemies == null || enemies.Count <= 0)
            return null;

        // 지금은 가장 앞의 플레이어 공격
        return enemies[0];
    }

    private void BasicAttack(CharacterBase target)
    {
        if (target == null)
            return;

        CombatModule combat =
            target.GetModule<CombatModule>();

        if (combat == null)
        {
            Debug.LogWarning($"{target.name}: CombatModule 없음");
            return;
        }

        int damage = Dice.RollD6();

        DamageStruct damageInfo = new DamageStruct
        {
            from = Owner.gameObject,
            instigator = null,
            damageAmount = damage,
            critical = false,
            highCritical = false,
            damageType = DamageType.Hand_to_hand_combat
        };

        combat.OnHit(damageInfo);

        Debug.Log($"{Owner.name} 기본 공격 → {target.name} / 피해 {damage}");
    }
}
