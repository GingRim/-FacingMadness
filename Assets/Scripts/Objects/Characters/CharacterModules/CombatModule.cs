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
        int finalDamage = damageInfo.damageAmount;

        // 1. 공격자 상태 효과: 의욕 / 무기력 등
        if (damageInfo.from != null)
        {
            CharacterBase attacker = damageInfo.from.GetComponent<CharacterBase>();

            StatusEffectModule attackerStatus = attacker != null ? attacker.GetModule<StatusEffectModule>() : null;

            if (attackerStatus != null)
            {
                finalDamage = attackerStatus.ModifyOutgoingDamage(finalDamage);
            }
        }

        // 2. 방어자 장갑
        ArmorModule armor = GetComponent<ArmorModule>();

        if (armor != null)
        {
            finalDamage = armor.GetReducedDamage(finalDamage, damageInfo.damageType);
        }

        // 3. 방어자 상태 효과: 가속 피해 감소, 취약 피해 증가 등
        StatusEffectModule defenderStatus = GetComponent<StatusEffectModule>();

        if (defenderStatus != null)
        {
            finalDamage = defenderStatus.ModifyIncomingDamage(finalDamage, damageInfo.damageType);
        }

        finalDamage = Mathf.Max(0, finalDamage);

        damageInfo.damageAmount = finalDamage;

        hp.TakeDamage(damageInfo);

        Debug.Log($"최종 피해: {damageInfo.damageAmount}");
    }



    public void OnRestore(in RestoreStruct restoreInfo)
    {
        if (hp == null)
            return;

        hp.TakeRestore(restoreInfo);
    }
}
