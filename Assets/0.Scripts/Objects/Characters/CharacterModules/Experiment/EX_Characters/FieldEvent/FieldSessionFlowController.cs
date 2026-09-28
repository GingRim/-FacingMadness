using System.Collections;
using UnityEngine;

/// <summary>
/// 필드 세션 엔딩과 게임 오버를 표시하고 새 게임이 가능한 상태로 정리합니다.
/// </summary>
public class FieldSessionFlowController : MonoBehaviour
{
    private FieldManager boundFieldManager;
    private bool resultIsOpen;

    private void Update()
    {
        if (boundFieldManager != null)
            return;

        FieldManager fieldManager = GameManager.Instance != null
            ? GameManager.Instance.Field
            : null;

        if (fieldManager == null)
            return;

        boundFieldManager = fieldManager;
        boundFieldManager.OnSessionEnded -= HandleSessionEnded;
        boundFieldManager.OnSessionEnded += HandleSessionEnded;
    }

    private void OnDestroy()
    {
        if (boundFieldManager != null)
            boundFieldManager.OnSessionEnded -= HandleSessionEnded;
    }

    private void HandleSessionEnded(FieldSessionResult result)
    {
        if (result == null || resultIsOpen)
            return;

        resultIsOpen = true;

        FieldSessionEndingData ending = result.Ending;
        string title = result.IsDeath
            ? "게임 오버"
            : ending != null && !string.IsNullOrWhiteSpace(ending.EndingTitle)
                ? ending.EndingTitle
                : "세션 종료";

        string description = ending != null
            ? ending.Description
            : string.Empty;

        if (string.IsNullOrWhiteSpace(description))
        {
            description = result.IsDeath
                ? "생명력 또는 정신력이 0이 되어 세션이 종료되었습니다."
                : "튜토리얼 세션이 종료되었습니다.";
        }

        StartCoroutine(FinishSession(title, description));
    }

    private IEnumerator FinishSession(string title, string description)
    {
        // 사망 이벤트와 전투 종료 콜백이 모두 끝난 뒤 상태를 정리합니다.
        yield return null;

        ResetSessionAndReturnToTitle();
        UIManager.ClaimPopUp(title, description, "확인");
        resultIsOpen = false;
    }

    private void ResetSessionAndReturnToTitle()
    {
        if (boundFieldManager != null)
            boundFieldManager.EndField();

        TutorialFieldFlowController tutorialFlow =
            FindFirstObjectByType<TutorialFieldFlowController>(
                FindObjectsInactive.Include);

        tutorialFlow?.ResetTutorialSession();

        DemoCharacterSpawner characterSpawner =
            FindFirstObjectByType<DemoCharacterSpawner>(
                FindObjectsInactive.Include);

        characterSpawner?.ResetSpawnedCharacters();

        UI_CharacterCreationScreen creationScreen =
            FindFirstObjectByType<UI_CharacterCreationScreen>(
                FindObjectsInactive.Include);

        creationScreen?.ResetCreationState();

        UIManager.OpenScreenM2(
            UIType.Title,
            ScreenChangeType.ScreenChanger);
    }
}
