using UnityEngine;

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

        int finalDamage = damageInfo.damageAmount;

        // 1. 공격자 상태 효과: 의욕 / 무기력 등
        if (damageInfo.from != null)
        {
            CharacterBase attacker = damageInfo.from.GetComponent<CharacterBase>();

            if (attacker != null)
            {
                StatusEffectModule attackerStatus = attacker.GetModule<StatusEffectModule>();

                if (attackerStatus != null)
                {
                    finalDamage = attackerStatus.ModifyOutgoingDamage(finalDamage);
                }
            }
        }

        // 2. 방어자 장갑
        ArmorModule armor = Owner.GetModule<ArmorModule>();

        if (armor != null)
        {
            finalDamage = armor.GetReducedDamage(finalDamage, damageInfo.damageType);
        }

        // 3. 방어자 상태 효과: 가속 피해 감소, 취약 피해 증가 등
        StatusEffectModule defenderStatus = Owner.GetModule<StatusEffectModule>();

        if (defenderStatus != null)
        {
            finalDamage = defenderStatus.ModifyIncomingDamage(finalDamage, damageInfo.damageType);
        }

        finalDamage = Mathf.Max(0, finalDamage);

        damageInfo.damageAmount = finalDamage;

        hp.TakeDamage(damageInfo);

        Debug.Log($"{Owner.name} 최종 피해: {damageInfo.damageAmount} / 현재 HP {hp.Current}/{hp.Max}");
    }



    public void OnRestore(in RestoreStruct restoreInfo)
    {
        if (hp == null)
            return;

        hp.TakeRestore(restoreInfo);
    }
}
