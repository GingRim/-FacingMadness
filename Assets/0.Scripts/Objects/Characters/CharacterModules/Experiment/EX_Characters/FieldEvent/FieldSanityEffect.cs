using UnityEngine;


[CreateAssetMenu(fileName = "NewFieldSanityEffect", menuName = "Field/Event Effect/Sanity")]
public class FieldSanityEffect : FieldEventEffect
{
    [SerializeField]
    private FieldSanityEffectType effectType;

    [SerializeField]
    private FieldEffectValue value;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.Character == null || value == null)
        {
            return;
        }

        SanityModule sanity = context.Character.GetModule<SanityModule>();

        if (sanity == null)
            return;

        int amount = value.Roll();

        if (effectType == FieldSanityEffectType.Damage)
        {
            sanity.TakeSanityDamage(amount);
        }
        else
        {
            sanity.RestoreSanity(amount);
        }
    }

}
