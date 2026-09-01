using UnityEngine;

/// <summary>
/// 드래그된 카드가 놓일 수 있는 위치의 공통 기반입니다.
/// CardClick은 이 컴포넌트를 찾고 CardInstance만 전달합니다.
/// </summary>
public abstract class CardDropReceiver : MonoBehaviour
{
    /// <summary>
    /// 드롭된 실제 카드 한 장을 해당 위치의 규칙으로 처리합니다.
    /// </summary>
    public abstract bool TryReceiveCard(CardInstance card);
}
