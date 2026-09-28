using System.Collections.Generic;
using UnityEngine;

public class FieldEventSelectionController : MonoBehaviour
{
    [Header("필드")]
    [SerializeField]
    private FieldManager fieldManager;

    [Header("후보 생성")]
    [SerializeField]
    private FieldEventCandidateBuilder candidateBuilder;

    [Header("후보 선택 UI")]
    [SerializeField]
    private UI_FieldEventCandidateSelect candidateSelectUI;

    [Header("이벤트 실행")]
    [SerializeField]
    private FieldEventRunner eventRunner;

    private FieldEventContext pendingContext;

    public bool IsSelecting =>
        candidateSelectUI != null && candidateSelectUI.IsOpen;

    private void OnEnable()
    {
        ResolveRuntimeReferences();
        BindCandidateSelectUI();
    }

    private void OnDisable()
    {
        if (candidateSelectUI != null)
        {
            candidateSelectUI.OnEventSelected -= HandleEventSelected;
            candidateSelectUI.Close();
        }

        pendingContext = null;
    }

    /// <summary>
    /// 실행 가능한 다음 이벤트 후보 3~5개를 생성하고
    /// 선택 UI를 엽니다.
    /// </summary>
    public bool OpenNextEventSelection(FieldEventContext context)
    {
        ResolveRuntimeReferences();
        BindCandidateSelectUI();

        if (context == null)
        {
            Debug.LogWarning(
                "이벤트 후보 생성 실패: " +
                "FieldEventContext가 없습니다.");

            return false;
        }

        if (fieldManager == null ||
            candidateBuilder == null ||
            candidateSelectUI == null ||
            eventRunner == null)
        {
            Debug.LogWarning(
                "FieldEventSelectionController: " +
                "필요한 참조가 연결되지 않았습니다.");

            return false;
        }

        if (fieldManager.CurrentFieldRoot == null)
        {
            Debug.LogWarning(
                "이벤트 후보 생성 실패: " +
                "현재 필드가 없습니다.");

            return false;
        }

        CharacterBase character = context.Character;

        if (character == null)
        {
            character = fieldManager.CurrentPlayer;
        }

        IReadOnlyList<FieldEventData> candidates =
            candidateBuilder.BuildCandidates(
                fieldManager,
                character,
                fieldManager.CurrentFieldRoot.EventPool,
                eventRunner);

        pendingContext = context;

        bool opened = candidateSelectUI.Open(candidates);

        if (!opened)
        {
            pendingContext = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// GameManager가 생성한 FieldManager와 필드 이벤트 구성 요소를
    /// 실행 시점에 찾아 연결합니다.
    /// </summary>
    private void ResolveRuntimeReferences()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Field != null)
        {
            fieldManager = GameManager.Instance.Field;

            // 후보 검사와 실제 이벤트 실행이 반드시 FieldManager와
            // 같은 Runner에서 처리되도록 런타임 참조를 통일합니다.
            if (fieldManager.EventRunner != null)
            {
                eventRunner = fieldManager.EventRunner;
            }
        }

        if (candidateBuilder == null)
        {
            candidateBuilder =
                FindFirstObjectByType<FieldEventCandidateBuilder>(
                    FindObjectsInactive.Include);
        }

        if (candidateSelectUI == null)
        {
            candidateSelectUI =
                FindFirstObjectByType<UI_FieldEventCandidateSelect>(
                    FindObjectsInactive.Include);
        }

        if (eventRunner == null)
        {
            eventRunner =
                FindFirstObjectByType<FieldEventRunner>(
                    FindObjectsInactive.Include);
        }
    }

    private void BindCandidateSelectUI()
    {
        if (candidateSelectUI == null)
            return;

        candidateSelectUI.OnEventSelected -= HandleEventSelected;
        candidateSelectUI.OnEventSelected += HandleEventSelected;
    }

    private void HandleEventSelected(FieldEventData selectedEvent)
    {
        if (selectedEvent == null)
            return;

        FieldEventContext context = pendingContext;

        pendingContext = null;

        if (context == null)
        {
            Debug.LogWarning(
                "이벤트 실행 실패: " +
                "대기 중인 Context가 없습니다.");

            return;
        }

        if (eventRunner == null)
            return;

        if (!eventRunner.OpenEvent(selectedEvent, context))
        {
            Debug.LogWarning(
                $"이벤트 실행 실패: " +
                $"{selectedEvent.EventName}");
        }
    }
}
