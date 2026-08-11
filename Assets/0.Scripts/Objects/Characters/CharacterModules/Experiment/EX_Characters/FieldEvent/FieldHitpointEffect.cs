using UnityEngine;


[CreateAssetMenu(fileName = "NewFieldHitpointEffect", menuName = "Field/Event Effect/Hitpoint")]
public class FieldHitpointEffect : FieldEventEffect
{
    
    [SerializeField]
    private FieldHitpointEffectType effectType;

    [SerializeField]
    private FieldEffectValue value;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.Character == null || value == null)
        {
            return;
        }

        HitpointModules hitpoint = context.Character.GetModule<HitpointModules>();

        if (hitpoint == null)
            return;

        int amount = value.Roll();

        if (effectType == FieldHitpointEffectType.Damage)
        {
            DamageStruct damageInfo = new DamageStruct
            {
                from = null,
                instigator = null,
                damageAmount = amount,
                damageType = DamageType.Physical,
                canCounter = false,
                reactionType = ActionType.None
            };

            hitpoint.TakeDamage(damageInfo);
        }
        else
        {
            RestoreStruct restoreInfo = new RestoreStruct
            {
                from = null,
                instigator = null,
                restoreAmount = amount
            };

            hitpoint.TakeRestore(restoreInfo);
        }
    }

}
