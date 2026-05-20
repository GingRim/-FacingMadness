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

    public void OnHit(in DamageStruct damageInfo)
    {
        if (hp == null)
            return;

        hp.TakeDamage(damageInfo);

        Debug.Log($"피해 적용: {damageInfo.damageAmount}");
    }



    public void OnRestore(in RestoreStruct restoreInfo)
    {
        hitpointModule.TakeRestore(restoreInfo);
    }
}
