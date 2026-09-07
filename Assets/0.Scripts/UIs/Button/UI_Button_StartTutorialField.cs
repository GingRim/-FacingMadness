using UnityEngine;

/// <summary>
/// 캐릭터 생성 완료 버튼을 튜토리얼 필드 시작에 연결합니다.
/// </summary>
public class UI_Button_StartTutorialField : MonoBehaviour
{
    private TutorialFieldFlowController flowController;

    public void Open()
    {
        if (flowController == null)
        {
            flowController =
                FindFirstObjectByType<TutorialFieldFlowController>(
                    FindObjectsInactive.Include);
        }

        if (flowController == null)
        {
            Debug.LogWarning(
                "튜토리얼 필드 시작 처리기를 찾지 못했습니다.");
            return;
        }

        flowController.StartTutorialField();
    }
}
