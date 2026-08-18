using UnityEngine;

[CreateAssetMenu(fileName = "NewMissionProgressEffect", menuName = "Field/Event Effect/Mission Progress")]

public class FieldMissionProgressEffect : FieldEventEffect
{
    [SerializeField]
    private string objectiveId;

    [SerializeField, Min(1)]
    private int amount = 1;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.FieldManager == null)
        {
            return;
        }

        context.FieldManager.AddMissionProgress(objectiveId, amount);
    }
}
