using UnityEngine;



[CreateAssetMenu(fileName = "NewFieldCardCheckBranch", menuName = "Field/Event Effect/Card Check Branch")]
public class FieldCardCheckBranchEffect : FieldEventEffect
{

    [Header("성공")]
    [SerializeField, TextArea(2, 6)]
    private string successText;

    [SerializeField]
    private FieldEventEffect[] successEffects;

    [Header("실패")]
    [SerializeField, TextArea(2, 6)]
    private string failureText;

    [SerializeField]
    private FieldEventEffect[] failureEffects;

    [Header("펌블")]
    [SerializeField, TextArea(2, 6)]
    private string fumbleText;

    [SerializeField]
    private FieldEventEffect[] fumbleEffects;

    public override void Execute(FieldEventContext context)
    {
        if (context == null)
            return;

        if (!context.HasCardCheck)
        {
            context.SetResultText("카드 판정 정보를 찾을 수 없습니다.");

            return;
        }

        switch (context.CardCheck.Result)
        {
            case FieldCardCheckResult.Success:
                context.SetResultText(successText);
                ApplyEffects(successEffects, context);
                break;

            case FieldCardCheckResult.Failure:
                context.SetResultText(failureText);
                ApplyEffects(failureEffects, context);
                break;

            case FieldCardCheckResult.Fumble:
                context.SetResultText(fumbleText);
                ApplyEffects(fumbleEffects, context);
                break;
        }
    }


    private void ApplyEffects(FieldEventEffect[] effects, FieldEventContext context)
    {
        if (effects == null)
            return;

        foreach (FieldEventEffect effect in effects)
        {
            if (effect == null)
                continue;

            effect.Execute(context);
        }
    }
}
