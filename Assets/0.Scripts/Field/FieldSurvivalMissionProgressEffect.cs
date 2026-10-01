using UnityEngine;

/// <summary>
/// 이벤트의 모든 결과 효과가 끝난 뒤 플레이어가 살아 있을 때만
/// 지정한 미션 목표를 진행시킨다.
/// </summary>
[CreateAssetMenu(fileName = "NewSurvivalMissionProgressEffect", menuName = "Field/Event Effect/Survival Mission Progress")]
public class FieldSurvivalMissionProgressEffect : FieldEventEffect
{
    [SerializeField]
    private string objectiveId;

    [SerializeField, Min(1)]
    private int amount = 1;

    public override void Execute(FieldEventContext context)
    {
        if (context == null)
            return;

        context.AddAfterResultAction(ApplyIfAlive);
    }

    private void ApplyIfAlive(FieldEventContext context)
    {
        if (context == null ||
            context.FieldManager == null ||
            context.Character == null)
        {
            return;
        }

        HitpointModules hitpoint =
            context.Character.GetModule<HitpointModules>();

        if (hitpoint == null || hitpoint.IsEmpty)
        {
            Debug.Log("생존 미션 목표 미달성: 이벤트 종료 후 플레이어가 생존하지 못했습니다.");
            return;
        }

        context.FieldManager.AddMissionProgress(
            objectiveId,
            Mathf.Max(1, amount));
    }
}
