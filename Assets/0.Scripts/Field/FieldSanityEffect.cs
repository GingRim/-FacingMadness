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
            int before = sanity.CurrentSanity;

            sanity.TakeSanityDamage(amount);

            int actualDamage = before - sanity.CurrentSanity;

            if (actualDamage > 0)
            {
                context.AddPostResultPopupMessage($"정신력이 {actualDamage} 감소");
            }
        }
        else
        {
            sanity.RestoreSanity(amount);
        }
    }

}
