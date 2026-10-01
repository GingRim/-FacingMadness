using UnityEngine;

/// <summary>
/// 현재 진행 중인 적색 라인 이벤트의 라인에 피해를 줍니다.
/// 라인 체력이 0이 되면 Normal로 변경하지만 이번 이동은 완료하지 않습니다.
/// </summary>
[CreateAssetMenu(
    fileName = "NewFieldLineDamageEffect",
    menuName = "Field/Event Effect/Red Line Damage")]
public class FieldLineDamageEffect : FieldEventEffect
{
    [SerializeField]
    private FieldEffectValue value;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.FieldManager == null || value == null)
            return;

        int requestedDamage = value.Roll();

        if (!context.FieldManager.TryDamagePendingRedLine(
                requestedDamage,
                out FieldLine damagedLine,
                out int appliedDamage))
        {
            Debug.LogWarning(
                "FieldLineDamageEffect: 피해를 적용할 적색 라인이 없습니다.");
            return;
        }

        string lineName =
            string.IsNullOrWhiteSpace(damagedLine.LineId)
                ? damagedLine.name
                : damagedLine.LineId;

        if (damagedLine.CurrentHitpoint <= 0)
        {
            context.AddResultMessage(
                $"{lineName}에 {appliedDamage} 피해를 주어 파괴했습니다. " +
                "경로가 일반 라인으로 변경되었습니다.");
            return;
        }

        context.AddResultMessage(
            $"{lineName}에 {appliedDamage} 피해 " +
            $"(체력 {damagedLine.CurrentHitpoint}/{damagedLine.MaximumHitpoint})");
    }
}
