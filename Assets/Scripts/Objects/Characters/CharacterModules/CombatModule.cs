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

        ArmorModule armor = GetComponent<ArmorModule>();

        if (armor != null)
        {
            finalDamage = armor.GetReducedDamage(finalDamage, damageInfo.damageType);
        }

        StatusEffectModule status = GetComponent<StatusEffectModule>();

        if (status != null)
        {
            finalDamage = status.ReduceDamageByStatus(finalDamage, damageInfo.damageType);
        }

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
