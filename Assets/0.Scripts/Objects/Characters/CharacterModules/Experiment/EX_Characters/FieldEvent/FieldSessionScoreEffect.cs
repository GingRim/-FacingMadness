using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFieldSessionScoreEffect",
    menuName = "Field/Event Effect/Session Score")]
public class FieldSessionScoreEffect : FieldEventEffect
{
    [SerializeField]
    private int scoreAmount;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.FieldManager == null)
            return;

        context.FieldManager.AddSessionScore(scoreAmount);
    }
}
