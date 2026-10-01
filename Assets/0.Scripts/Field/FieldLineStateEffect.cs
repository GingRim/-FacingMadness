using UnityEngine;


[CreateAssetMenu(fileName = "NewFieldLineStateEffect", menuName = "Field/Event Effect/Line State")]

public class FieldLineStateEffect : FieldEventEffect
{
    [SerializeField]
    private string targetLineId;

    [SerializeField]
    private FieldLineStateEffectType effectType;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.FieldManager == null || context.FieldManager.CurrentFieldRoot == null)
        {
            return;
        }

        FieldLine line = context.FieldManager.CurrentFieldRoot.FindLine(targetLineId);

        if (line == null)
        {
            Debug.LogWarning($"라인 상태 변경 실패: {targetLineId}를 찾지 못했습니다.");

            return;
        }


        switch (effectType)
        {
            case FieldLineStateEffectType.RevealHidden:
                line.Discover();
                break;

            case FieldLineStateEffectType.UnlockRed:
                line.ClearBlock();
                break;

            case FieldLineStateEffectType.ChangeToNormal:
                line.ChangeType(FieldLineType.Normal);
                break;

            case FieldLineStateEffectType.ChangeToRed:
                line.ChangeType(FieldLineType.Red);
                break;

            case FieldLineStateEffectType.ChangeToHidden:
                line.ChangeType(FieldLineType.Hidden);
                break;
        }
    }

}
