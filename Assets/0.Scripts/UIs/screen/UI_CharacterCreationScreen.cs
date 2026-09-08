using UnityEngine;

public class UI_CharacterCreationScreen : UI_ScreenBase
{
    private CharacterBuildData selectedBuildData;
    private bool isCreating;

    public CharacterBuildData SelectedBuildData => selectedBuildData;
    public bool HasSelection => selectedBuildData != null;

    private void OnEnable()
    {
        InputManager.OnPause -= CanelPause;
        InputManager.OnPause += CanelPause;
    }

    private void OnDisable()
    {
        InputManager.OnPause -= CanelPause;
    }


    void CanelPause(bool value)
    {
        if (UIManager.GetUIM2(UIType.Pause).isActiveAndEnabled)
        {
            UIManager.CloseUIM2(UIType.Pause);
        }
        else
        {
            UIManager.ToggleUIM2(UIType.Pause);
        }

    }

    /// <summary>
    /// 직업 버튼에서 만든 캐릭터 데이터를 현재 선택으로 보관합니다.
    /// </summary>
    public void SelectBuildData(CharacterBuildData buildData)
    {
        if (buildData == null)
            return;

        selectedBuildData = buildData;

        Debug.Log(
            $"캐릭터 선택: {selectedBuildData.characterName}");
    }

    /// <summary>
    /// 선택 데이터를 실제 플레이어로 생성하고 그 플레이어로 튜토리얼을 시작합니다.
    /// </summary>
    public bool TryCreateAndStartTutorial()
    {
        if (isCreating)
            return false;

        if (selectedBuildData == null)
        {
            Debug.LogWarning(
                "캐릭터를 먼저 선택해야 합니다.");
            return false;
        }

        DemoCharacterSpawner characterSpawner = FindFirstObjectByType<DemoCharacterSpawner>(FindObjectsInactive.Include);

        TutorialFieldFlowController flowController = FindFirstObjectByType<TutorialFieldFlowController>(FindObjectsInactive.Include);

        if (characterSpawner == null || flowController == null)
        {
            Debug.LogWarning(
                "캐릭터 생성기 또는 튜토리얼 시작 처리기를 찾지 못했습니다.");
            return false;
        }

        isCreating = true;

        CharacterBase createdCharacter =
            characterSpawner.SpawnPlayerCharacter(selectedBuildData);

        if (createdCharacter == null)
        {
            isCreating = false;
            return false;
        }

        bool started = flowController.StartTutorialField();

        if (!started)
        {
            isCreating = false;
            return false;
        }

        return true;
    }

}



