using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 필드의 시작과
/// 필드 전투 이후 복귀를 관리합니다.
/// </summary>
public class TutorialFieldFlowController : MonoBehaviour
{
    [Header("튜토리얼 필드")]
    [SerializeField]
    private MissionFieldRoot tutorialFieldRoot;

    [SerializeField]
    private GameObject fieldCanvasRoot;

    [Header("플레이어")]
    [SerializeField]
    private DemoCharacterSpawner characterSpawner;

    [Header("화면 전환")]
    [SerializeField] private ScreenChangeType screenChangeType = ScreenChangeType.ScreenChanger;
    
    private readonly List<CharacterBase> preparedPlayers = new();
    public bool HasStarted { get; private set; }

    private bool isWaitingForBattleReturn;
    private bool shouldStartFieldAfterBattle;

    private void Awake()
    {
        ResolveReferences();

        if (fieldCanvasRoot == null)
        {
            Debug.LogWarning(
                "TutorialFieldFlowController: " +
                "시작 시 FieldCanvas를 찾지 못했습니다.");

            return;
        }

        SetFieldCanvasActive(false);
    }

    private void OnEnable()
    {
        BattleManager.OnBattleStarted -=
            HandleBattleStarted;

        BattleManager.OnBattleStarted +=
            HandleBattleStarted;

        BattleManager.OnBattleEnded -=
            HandleBattleEnded;

        BattleManager.OnBattleEnded +=
            HandleBattleEnded;

        UI_RewardWindow.OnRewardClosed -=
            HandleRewardClosed;

        UI_RewardWindow.OnRewardClosed +=
            HandleRewardClosed;
    }

    private void OnDisable()
    {
        BattleManager.OnBattleStarted -=
            HandleBattleStarted;

        BattleManager.OnBattleEnded -=
            HandleBattleEnded;

        UI_RewardWindow.OnRewardClosed -=
            HandleRewardClosed;
    }

    /// <summary>
    /// 튜토리얼 필드를 처음 시작합니다.
    /// </summary>
    public bool StartTutorialField()
    {
        if (HasStarted)
            return false;

        ResolveReferences();

        FieldManager fieldManager =
            GameManager.Instance != null
                ? GameManager.Instance.Field
                : null;

        if (fieldManager == null)
        {
            Debug.LogWarning(
                "TutorialFieldFlowController: " +
                "FieldManager가 없습니다.");

            return false;
        }

        if (tutorialFieldRoot == null ||
            fieldCanvasRoot == null)
        {
            Debug.LogWarning(
                "TutorialFieldFlowController: " +
                "FieldCanvas 또는 튜토리얼 필드가 없습니다.");

            return false;
        }

        if (characterSpawner == null ||
            !characterSpawner.EnsureCharactersSpawned() ||
            characterSpawner.SpawnedCharacters == null ||
            characterSpawner.SpawnedCharacters.Count == 0)
        {
            Debug.LogWarning(
                "TutorialFieldFlowController: " +
                "생성된 EX 플레이어가 없습니다.");

            return false;
        }

        SetFieldCanvasActive(true);
        InitializeInactiveFieldUI();

        List<CharacterBase> players = new();

        foreach (CharacterBase character in
                 characterSpawner.SpawnedCharacters)
        {
            if (character != null &&
                !players.Contains(character))
            {
                players.Add(character);
            }
        }

        if (!fieldManager.StartExistingField(
                tutorialFieldRoot,
                players))
        {
            SetFieldCanvasActive(false);

            return false;
        }

        RefreshFieldHand(
            fieldManager.CurrentPlayer);

        HasStarted = true;

        UIManager.OpenScreenM2(
            UIType.Field,
            screenChangeType);

        return true;
    }

    /// <summary>
    /// 필드를 거치지 않고 시작한 전투에서 승리하면
    /// 보상창을 닫은 뒤 튜토리얼 필드를 시작하도록 예약합니다.
    /// </summary>
    public bool PrepareFieldStartAfterBattle(IReadOnlyList<CharacterBase> players)
    {
        preparedPlayers.Clear();

        if (players != null)
        {
            foreach (CharacterBase player in players)
            {
                if (player != null &&
                    !preparedPlayers.Contains(player))
                {
                    preparedPlayers.Add(player);
                }
            }
        }

        if (preparedPlayers.Count == 0)
        {
            shouldStartFieldAfterBattle = false;
            return false;
        }

        shouldStartFieldAfterBattle = true;
        return true;
    }

    /// <summary>
    /// 이미 진행 중인 필드에서 전투가 시작되면
    /// 필드 데이터는 유지하고 화면만 끕니다.
    /// </summary>
    private void HandleBattleStarted()
    {
        if (!HasStarted)
            return;

        isWaitingForBattleReturn = false;

        SetFieldCanvasActive(false);

        UIManager.OpenScreenM2(
            UIType.Battle,
            screenChangeType);
    }

    /// <summary>
    /// 전투 종료 결과를 기록합니다.
    /// 승리한 경우 보상창이 닫힐 때까지 기다립니다.
    /// </summary>
    private void HandleBattleEnded(
        bool victory)
    {
        if (!HasStarted &&
            !shouldStartFieldAfterBattle)
        {
            return;
        }

        isWaitingForBattleReturn = victory;

        if (!victory)
        {
            shouldStartFieldAfterBattle = false;
            SetFieldCanvasActive(false);
        }
    }

    /// <summary>
    /// 보상창이 닫히면 기존 필드로 복귀하거나
    /// 예약된 튜토리얼 필드를 새로 시작합니다.
    /// </summary>
    private void HandleRewardClosed()
    {
        if (!isWaitingForBattleReturn)
            return;

        isWaitingForBattleReturn = false;

        // 타이틀에서 전투로 바로 들어온 경우입니다.
        // 아직 필드가 없으므로 튜토리얼 필드를 새로 시작합니다.
        if (!HasStarted)
        {
            bool shouldStartField =
                shouldStartFieldAfterBattle;

            shouldStartFieldAfterBattle = false;

            if (shouldStartField &&
                !StartTutorialField())
            {
                Debug.LogWarning(
                    "TutorialFieldFlowController: " +
                    "전투 보상 종료 후 튜토리얼 필드 시작에 실패했습니다.");
            }

            return;
        }

        // 필드에서 시작한 전투라면
        // 기존 필드 상태를 그대로 다시 표시합니다.
        shouldStartFieldAfterBattle = false;

        SetFieldCanvasActive(true);

        FieldManager fieldManager =
            GameManager.Instance != null
                ? GameManager.Instance.Field
                : null;

        if (fieldManager != null)
        {
            RefreshFieldHand(
                fieldManager.CurrentPlayer);
        }

        UIManager.OpenScreenM2(
            UIType.Field,
            screenChangeType);
    }

    private void SetFieldCanvasActive(
        bool active)
    {
        if (fieldCanvasRoot != null &&
            fieldCanvasRoot.activeSelf != active)
        {
            fieldCanvasRoot.SetActive(active);
        }
    }

    private void RefreshFieldHand(
        CharacterBase player)
    {
        if (player == null ||
            fieldCanvasRoot == null)
        {
            return;
        }

        UI_Hand handUI =
            fieldCanvasRoot.GetComponentInChildren<UI_Hand>(
                true);

        if (handUI == null)
            return;

        DeckModule deck =
            player.GetModule<DeckModule>();

        if (deck != null)
        {
            handUI.RefreshFromDeck(deck);
        }
        else
        {
            handUI.ClearHand();
        }
    }

    private void ResolveReferences()
    {
        if (fieldCanvasRoot == null)
        {
            Canvas[] canvases =
                FindObjectsByType<Canvas>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (Canvas canvas in canvases)
            {
                if (canvas != null &&
                    canvas.gameObject.name ==
                    "FieldCanvas")
                {
                    fieldCanvasRoot =
                        canvas.gameObject;

                    break;
                }
            }
        }

        if (tutorialFieldRoot == null)
        {
            if (fieldCanvasRoot != null)
            {
                tutorialFieldRoot =
                    fieldCanvasRoot
                        .GetComponentInChildren<MissionFieldRoot>(
                            true);
            }

            if (tutorialFieldRoot == null)
            {
                tutorialFieldRoot =
                    FindFirstObjectByType<MissionFieldRoot>(
                        FindObjectsInactive.Include);
            }
        }

        if (fieldCanvasRoot == null &&
            tutorialFieldRoot != null)
        {
            Canvas fieldCanvas =
                tutorialFieldRoot
                    .GetComponentInParent<Canvas>(
                        true);

            if (fieldCanvas != null)
            {
                fieldCanvasRoot =
                    fieldCanvas.gameObject;
            }
        }

        if (characterSpawner == null)
        {
            characterSpawner =
                FindFirstObjectByType<DemoCharacterSpawner>(
                    FindObjectsInactive.Include);
        }
    }

    /// <summary>
    /// 비활성 상태에서 이벤트를 구독해야 하는
    /// 필드 UI를 처음 한 번 활성화합니다.
    /// 각 UI는 Awake에서 자신의 패널을 다시 닫습니다.
    /// </summary>
    private void InitializeInactiveFieldUI()
    {
        UI_FieldEvent eventUI =
            fieldCanvasRoot
                .GetComponentInChildren<UI_FieldEvent>(
                    true);

        if (eventUI != null &&
            !eventUI.gameObject.activeSelf)
        {
            eventUI.gameObject.SetActive(true);
        }

        UI_FieldCardSelector cardSelector =
            fieldCanvasRoot
                .GetComponentInChildren<UI_FieldCardSelector>(
                    true);

        if (cardSelector != null &&
            !cardSelector.gameObject.activeSelf)
        {
            cardSelector.gameObject.SetActive(true);
        }
    }
}