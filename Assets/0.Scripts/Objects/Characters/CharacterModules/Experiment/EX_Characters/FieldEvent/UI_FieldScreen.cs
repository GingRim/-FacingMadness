using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// UIManager가 생성하는 필드 화면의 최상위 스크립트입니다.
/// GameManager가 런타임에 생성한 FieldManager를 자동으로 연결합니다.
/// </summary>
public class UI_FieldScreen : UI_ScreenBase
{
    [Header("필드 매니저")]
    [SerializeField]
    private FieldManager fieldManager;

    [Header("손패")]
    [SerializeField]
    private UI_Hand handUI;

    [Header("현재 플레이어")]
    [SerializeField]
    private TextMeshProUGUI playerNameText;

    [Header("행동력")]
    [SerializeField]
    private TextMeshProUGUI actionPointText;

    [Header("턴")]
    [SerializeField]
    private TextMeshProUGUI fieldTurnText;

    [Header("필드 카드 사용 공간")]
    [SerializeField]
    private UI_FieldCardUseDropTarget fieldCardUseArea;

    [Header("필드 로그")]
    [SerializeField]
    private UI_FieldLog fieldLog;

    private CharacterBase boundPlayer;
    private ActionPointModule boundActionPoint;
    private DeckModule boundDeck;
    private Coroutine fieldManagerBindRoutine;

    private void OnEnable()
    {
        ResolveRuntimeFieldManager();
        RegisterFieldManager();
        RefreshCurrentFieldState();

        if (!HasRuntimeFieldManager())
        {
            fieldManagerBindRoutine =
                StartCoroutine(WaitForRuntimeFieldManager());
        }
    }

    private void OnDisable()
    {
        StopFieldManagerBindRoutine();
        UnregisterFieldManager();
        UnbindPlayer();
    }

    /// <summary>
    /// GameManager가 생성해 보관 중인 FieldManager를 우선 사용합니다.
    /// 인스펙터 참조는 독립 실행용 예비 연결로만 유지합니다.
    /// </summary>
    private void ResolveRuntimeFieldManager()
    {
        if (!HasRuntimeFieldManager())
            return;

        FieldManager runtimeFieldManager =
            GameManager.Instance.Field;

        if (fieldManager == runtimeFieldManager)
            return;

        UnregisterFieldManager();
        UnbindPlayer();

        fieldManager = runtimeFieldManager;
    }

    private bool HasRuntimeFieldManager()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.Field != null;
    }

    /// <summary>
    /// 필드 화면이 GameManager보다 먼저 활성화된 경우
    /// 런타임 FieldManager가 생성될 때까지 기다린 뒤 연결합니다.
    /// </summary>
    private IEnumerator WaitForRuntimeFieldManager()
    {
        while (isActiveAndEnabled && !HasRuntimeFieldManager())
        {
            yield return null;
        }

        fieldManagerBindRoutine = null;

        if (!isActiveAndEnabled || !HasRuntimeFieldManager())
            yield break;

        ResolveRuntimeFieldManager();
        RegisterFieldManager();
        RefreshCurrentFieldState();
    }

    private void StopFieldManagerBindRoutine()
    {
        if (fieldManagerBindRoutine == null)
            return;

        StopCoroutine(fieldManagerBindRoutine);
        fieldManagerBindRoutine = null;
    }

    private void RegisterFieldManager()
    {
        if (fieldManager == null)
            return;

        fieldManager.OnFieldTurnStarted -= HandleFieldTurnStarted;
        fieldManager.OnFieldTurnStarted += HandleFieldTurnStarted;

        fieldManager.OnCurrentPlayerChanged -= HandleCurrentPlayerChanged;
        fieldManager.OnCurrentPlayerChanged += HandleCurrentPlayerChanged;

        fieldManager.OnFieldLog -= HandleFieldLog;
        fieldManager.OnFieldLog += HandleFieldLog;
    }

    private void UnregisterFieldManager()
    {
        if (fieldManager == null)
            return;

        fieldManager.OnFieldTurnStarted -= HandleFieldTurnStarted;
        fieldManager.OnCurrentPlayerChanged -= HandleCurrentPlayerChanged;
        fieldManager.OnFieldLog -= HandleFieldLog;
    }

    private void RefreshCurrentFieldState()
    {
        if (fieldManager == null)
        {
            ClearScreen();
            ClearTurnTexts();
            return;
        }

        BindPlayer(fieldManager.CurrentPlayer);
        RefreshTurnTexts(fieldManager.TotalFieldTurn);
    }

    private void HandleFieldTurnStarted(
        CharacterBase player,
        int completedTurnCount)
    {
        BindPlayer(player);
        RefreshTurnTexts(completedTurnCount);

        if (fieldCardUseArea != null)
        {
            fieldCardUseArea.ResetDisplay();
        }
    }

    private void HandleCurrentPlayerChanged(CharacterBase player)
    {
        BindPlayer(player);
    }

    private void HandleFieldLog(string message)
    {
        if (fieldLog == null)
        {
            fieldLog = GetComponentInChildren<UI_FieldLog>(true);
        }

        fieldLog?.AddLog(message);
    }

    private void BindPlayer(CharacterBase player)
    {
        UnbindPlayer();

        boundPlayer = player;

        if (boundPlayer == null)
        {
            ClearScreen();
            return;
        }

        if (playerNameText != null)
        {
            playerNameText.SetText(boundPlayer.DisplayName);
        }

        boundDeck =
            boundPlayer.GetModule<DeckModule>();

        if (boundDeck != null)
        {
            boundDeck.OnCardZonesChanged -= RefreshHand;
            boundDeck.OnCardZonesChanged += RefreshHand;
        }

        if (handUI != null)
        {
            if (boundDeck != null)
            {
                handUI.RefreshFromDeck(boundDeck);
            }
            else
            {
                handUI.ClearHand();
            }
        }

        boundActionPoint =
            boundPlayer.GetModule<ActionPointModule>();

        if (boundActionPoint == null)
        {
            RefreshActionPoint(0, 0);
            return;
        }

        boundActionPoint.OnActionPointChanged -= RefreshActionPoint;
        boundActionPoint.OnActionPointChanged += RefreshActionPoint;

        RefreshActionPoint(
            boundActionPoint.Current,
            boundActionPoint.Max);
    }

    private void UnbindPlayer()
    {
        if (boundDeck != null)
        {
            boundDeck.OnCardZonesChanged -= RefreshHand;
        }

        if (boundActionPoint != null)
        {
            boundActionPoint.OnActionPointChanged -= RefreshActionPoint;
        }

        boundDeck = null;
        boundActionPoint = null;
        boundPlayer = null;
    }

    private void RefreshHand()
    {
        if (handUI == null)
            return;

        if (boundDeck != null)
        {
            handUI.RefreshFromDeck(boundDeck);
        }
        else
        {
            handUI.ClearHand();
        }
    }

    private void RefreshActionPoint(int current, int maximum)
    {
        if (actionPointText != null)
        {
            actionPointText.SetText(
                $"행동력 {current}/{maximum}");
        }
    }

    private void RefreshTurnTexts(int completedTurnCount)
    {
        // totalFieldTurn은 끝난 턴의 개수이므로
        // 현재 진행 중인 턴은 +1입니다.
        int currentTurn = completedTurnCount + 1;

        if (fieldTurnText != null)
        {
            fieldTurnText.SetText(
                $"필드 턴 {currentTurn}");
        }

    }

    private void ClearScreen()
    {
        if (playerNameText != null)
        {
            playerNameText.SetText(string.Empty);
        }

        RefreshActionPoint(0, 0);

        if (handUI != null)
        {
            handUI.ClearHand();
        }
    }

    private void ClearTurnTexts()
    {
        if (fieldTurnText != null)
        {
            fieldTurnText.SetText(string.Empty);
        }

    }
}
