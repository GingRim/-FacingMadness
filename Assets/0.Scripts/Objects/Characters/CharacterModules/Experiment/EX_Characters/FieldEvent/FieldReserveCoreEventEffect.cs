using UnityEngine;

/// <summary>
/// 다음 이벤트 후보 목록에 핵심 이벤트가 포함되도록 예약하는
/// 필드 이벤트 선택지 효과다.
/// </summary>
[CreateAssetMenu(fileName = "NewReserveCoreEventEffect", menuName = "Field/Event Effect/Reserve Core Event")]
public class FieldReserveCoreEventEffect : FieldEventEffect
{
    /// <summary>
    /// 현재 플레이어의 다음 이벤트 후보에
    /// 핵심 이벤트가 포함되도록 예약한다.
    /// </summary>
    /// <param name="context">현재 이벤트 실행 정보</param>
    public override void Execute(FieldEventContext context)
    {
        if (context == null)
        {
            Debug.LogWarning("핵심 이벤트 예약 실패: 이벤트 실행 정보가 없습니다.");

            return;
        }

        if (context.FieldManager == null)
        {
            Debug.LogWarning("핵심 이벤트 예약 실패: FieldManager가 없습니다.");

            return;
        }

        CharacterBase character = context.Character;

        if (character == null)
        {
            Debug.LogWarning("핵심 이벤트 예약 실패: 대상 캐릭터가 없습니다.");

            return;
        }

        context.FieldManager.ReserveCoreEventForNextSelection(character);

        context.AddResultMessage("다음 이벤트 후보에 핵심 이벤트가 등장합니다.");
    }
}
