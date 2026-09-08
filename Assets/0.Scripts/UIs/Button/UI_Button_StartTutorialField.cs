using UnityEngine;

/// <summary>
/// 캐릭터 생성 완료 버튼을 튜토리얼 필드 시작에 연결합니다.
/// </summary>
public class UI_Button_StartTutorialField : MonoBehaviour
{
    private UI_CharacterCreationScreen creationScreen;

    public void Open()
    {
        if (creationScreen == null)
        {
            creationScreen =
                GetComponentInParent<UI_CharacterCreationScreen>();

            if (creationScreen == null)
            {
                creationScreen =
                    FindFirstObjectByType<UI_CharacterCreationScreen>(
                    FindObjectsInactive.Include);
            }
        }

        if (creationScreen == null)
        {
            Debug.LogWarning(
                "캐릭터 생성 화면을 찾지 못했습니다.");
            return;
        }

        creationScreen.TryCreateAndStartTutorial();
    }
}
