using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFieldSessionEndEffect",
    menuName = "Field/Event Effect/Session End")]
public class FieldSessionEndEffect : FieldEventEffect
{
    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.FieldManager == null)
            return;

        // 결과 화면을 먼저 확인한 뒤 세션을 끝낼 수 있도록
        // 현재 이벤트 화면이 닫힌 다음 엔딩을 확정합니다.
        context.AddEventClosedAction(
            () => context.FieldManager.RequestSessionEnd());
    }
}
