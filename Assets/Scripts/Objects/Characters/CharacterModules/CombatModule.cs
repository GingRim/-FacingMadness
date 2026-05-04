using UnityEngine;

public class CombatModule : CharacterModule
{
    protected HitpointModules hitpointModule;
    public void OnHit(in DamageStruct damageInfo)
    {
        hitpointModule.TakeDamage(damageInfo);
    }

    public void OnRestore(in RestoreStruct restoreInfo)
    {
        hitpointModule.TakeRestore(restoreInfo);
    }
}
