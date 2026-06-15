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
        ArmorModule armor = GetComponent<ArmorModule>();

        int finalDamage = damageInfo.damageAmount;

        if (armor != null)
        {
         finalDamage = armor.GetReducedDamage(damageInfo.damageAmount, damageInfo.damageType);
        }

        damageInfo.damageAmount = finalDamage;

        // 기존 생명력 감소 처리
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
