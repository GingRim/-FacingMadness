using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFieldSurvivalInformationReward",
    menuName = "Field/Event Effect/Survival Information Reward")]
public class FieldSurvivalInformationRewardEffect : FieldEventEffect
{
    [SerializeField]
    private FieldInformationData information;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.Character == null || information == null)
            return;

        context.AddAfterResultAction(GrantIfAlive);
    }

    private void GrantIfAlive(FieldEventContext context)
    {
        if (context == null || context.Character == null)
            return;

        HitpointModules hitpoint =
            context.Character.GetModule<HitpointModules>();

        if (hitpoint == null || hitpoint.IsEmpty)
            return;

        FieldInformationInventory inventory =
            FieldInformationInventory.GetOrCreate(context.Character);

        inventory?.Add(information);
    }
}
