using UnityEngine;


/// <summary>
/// 다음에 발생하는 이벤트를 핵심 이벤트로 확정하는
/// 필드 이벤트 선택지 효과다.
/// </summary>
[CreateAssetMenu(fileName = "NewForceNextCoreEventEffect", menuName = "Field/Event Effect/Force Next Core Event")]
public class FieldForceNextCoreEventEffect : FieldEventEffect
{
    /// <summary>
    /// 현재 이벤트를 진행하는 캐릭터의
    /// 다음 이벤트를 핵심 이벤트로 예약한다.
    /// </summary>
    /// <param name="context">현재 이벤트 실행 정보</param>
    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.FieldManager == null)
        {
            return;
        }

        CharacterBase character = context.Character;

        if (character == null)
        {
            Debug.LogWarning("핵심 이벤트 예약 실패: " + "대상 캐릭터가 없습니다.");

            return;
        }

        context.FieldManager.ReserveCoreEventForNextEvent(character);

        context.AddResultMessage("다음 이벤트에서 핵심 사건이 발생합니다.");
    }
}