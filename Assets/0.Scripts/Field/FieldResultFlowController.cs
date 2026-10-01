using UnityEngine;

public class FieldResultFlowController : MonoBehaviour
{
    [Header("결과 UI")]
    [SerializeField]
    private UI_FieldMissionResult resultUI;

    private void OnEnable()
    {
        if (resultUI == null)
            return;

        resultUI.OnResultConfirmed -= HandleResultConfirmed;

        resultUI.OnResultConfirmed += HandleResultConfirmed;
    }

    private void OnDisable()
    {
        if (resultUI != null)
        {
            resultUI.OnResultConfirmed -= HandleResultConfirmed;
        }
    }

    private void HandleResultConfirmed(FieldMissionData mission, bool cleared)
    {
        // 현재 필드 화면 종료
        UIManager.CloseUIM2(UIType.Field);

        if (cleared)
        {
            OpenReward(mission);
        }
        else
        {
            OpenGameOver();
        }
    }

    private void OpenReward(FieldMissionData completedMission)
    {
        UIBase rewardUI = UIManager.OpenUIM2(UIType.Reward);

        if (rewardUI == null)
        {
            Debug.LogWarning("FieldResultFlowController: " + "Reward UI를 찾지 못했습니다.");

            return;
        }

        string missionName = completedMission != null ? completedMission.MissionName : "알 수 없는 미션";

        Debug.Log($"필드 보상 UI 공개: {missionName}");

        // 보상 데이터가 만들어지면 여기에서 전달
        //
        // UI_RewardWindow rewardWindow =
        //     rewardUI as UI_RewardWindow;
        //
        // rewardWindow?.SetReward(
        //     completedMission.RewardData);
    }

    private void OpenGameOver()
    {
        UIBase gameOverUI = UIManager.OpenUIM2(UIType.GameOver);

        if (gameOverUI == null)
        {
            Debug.LogWarning("FieldResultFlowController: " + "GameOver UI를 찾지 못했습니다.");

            return;
        }

        Debug.Log("필드 게임 오버 UI 공개");
    }
}
