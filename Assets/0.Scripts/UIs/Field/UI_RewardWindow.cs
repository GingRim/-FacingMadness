using System;
using UnityEngine;

public class UI_RewardWindow : OpenableUIBase
{
    /// <summary>
    /// 보상창이 실제로 닫혔을 때 발생합니다.
    /// 필드 전투가 끝난 후 필드 화면으로 복귀하는 시점에 사용합니다.
    /// </summary>
    public static event Action OnRewardClosed;

    public override void Close()
    {
        bool wasOpen = IsOpen;

        base.Close();

        if (wasOpen)
        {
            BattleManager battleManager =
                GameManager.Instance != null
                    ? GameManager.Instance.Battle
                    : null;

            // 승리 전투는 보상창을 닫는 시점까지 결과 처리를 유지하고
            // 다음 화면으로 넘어가기 전에 전투 상태를 비웁니다.
            battleManager?.ResetBattleState();

            OnRewardClosed?.Invoke();
        }
    }
}
