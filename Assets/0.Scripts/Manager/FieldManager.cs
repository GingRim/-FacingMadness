using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : ManagerBase
{
    [Header("필드 구성")]
    [SerializeField] private FieldNode startingNode;
    [SerializeField] private List<FieldNode> nodes = new();

    [Header("이벤트 실행기")]
    [SerializeField] private FieldEventRunner eventRunner;

    [Header("필드 이벤트 선택")]
    [SerializeField]
    private FieldEventSelectionController fieldEventSelectionController;

    [Header("필드 턴")]
    [SerializeField, Min(1)]
    private int mythTurnInterval = 10;

    [Header("캐릭터 마커 이동")]
    [Tooltip("노드 사이를 이동하는 아이콘 애니메이션 시간입니다.")]
    [SerializeField, Min(0f)]
    private float markerMoveDuration = 0.5f;

    public int MythTurnInterval => mythTurnInterval;

    private readonly List<CharacterBase> participants = new();

    private CharacterBase currentPlayer;
    private FieldNode currentNode;
    private UI_Hand handUI;
    private UI_FieldCharacterMarkers characterMarkers;

    private int currentPlayerIndex;
    private int totalFieldTurn;
    private readonly Queue<int> pendingBattleMythTurns = new();

    private FieldLine pendingRedLine;
    private FieldNode pendingTargetNode;
    private FieldRedLineResult pendingRedLineResult;
    private bool pendingRedLineDestroyed;
    private bool isPlayerMarkerMoving;

    private Transform fieldCore;
    private GameObject currentFieldObject;
    private MissionFieldRoot currentFieldRoot;
    private bool ownsCurrentFieldObject;

    private readonly List<FieldNode> startingNodeCandidates = new();
    private readonly HashSet<CharacterBase> coreEventReservations = new();
    /// <summary>
    /// 다음 이벤트를 핵심 이벤트로 확정한 캐릭터 목록이다.
    /// </summary>
    private readonly HashSet<CharacterBase> forcedCoreEventReservations = new();

    private FieldEventData pendingHandCoreEvent;

    public IReadOnlyList<FieldNode> StartingNodeCandidates => startingNodeCandidates;

    private FieldMissionData currentMission;

    private readonly Dictionary<string, int> missionProgress = new();
    private int sessionScore;
    private FieldSessionResult currentSessionResult;

    public FieldMissionData CurrentMission => currentMission;
    public int SessionScore => sessionScore;
    public FieldSessionResult CurrentSessionResult => currentSessionResult;
    public bool HasEndedByDeath =>
        currentSessionResult != null && currentSessionResult.IsDeath;
    public GameObject CurrentFieldObject => currentFieldObject;
    public CharacterBase CurrentPlayer => currentPlayer;
    public FieldNode CurrentNode => currentNode;
    public MissionFieldRoot CurrentFieldRoot => currentFieldRoot;
    public FieldEventRunner EventRunner => eventRunner;
    public bool HasStartingNode => startingNode != null;

    public IReadOnlyList<FieldNode> Nodes => nodes;

    public bool IsSelectingStartingNode => isSelectingStartingNode;


    public IReadOnlyList<CharacterBase> Participants => participants;

    public int TotalFieldTurn => totalFieldTurn;

    public bool IsFieldActive { get; private set; }

    public bool IsPlayerMarkerMoving => isPlayerMarkerMoving;

    public FieldTurnState TurnState { get; private set; } = FieldTurnState.Inactive;

    public event Action<CharacterBase> OnCurrentPlayerChanged;
    public event Action<FieldNode> OnNodeChanged;
    public event Action<int> OnMythTurnRequested;
    public event Action OnFieldGameOver;
    public event Action<MissionFieldRoot> OnMissionFieldLoaded;
    public event Action<IReadOnlyList<FieldNode>> OnStartingNodeSelectionRequested;
    public event Action OnMadnessEntered;
    public event Action<string, int, int> OnMissionProgressChanged;
    public event Action<FieldMissionData> OnMissionCleared;
    public event Action<int> OnSessionScoreChanged;
    public event Action<FieldSessionResult> OnSessionEnded;
    public event Action<FieldNode> OnStartingNodeConfirmed;
    public event Action<CharacterBase, int> OnFieldTurnStarted;
    public event Action<string> OnFieldLog;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        ResolveRuntimeReferences();
        RegisterNodes();
        RegisterEventRunner();

        yield return null;
    }

    protected override void OnDisconnected()
    {
        UnregisterNodes();
        UnregisterEventRunner();
    }

    private void RegisterNodes()
    {
        foreach (FieldNode node in nodes)
        {
            if (node == null)
                continue;

            node.OnClicked -= HandleNodeClicked;
            node.OnClicked += HandleNodeClicked;
        }
    }

    private void UnregisterNodes()
    {
        foreach (FieldNode node in nodes)
        {
            if (node == null)
                continue;

            node.OnClicked -= HandleNodeClicked;
        }
    }

    private void RegisterEventRunner()
    {
        if (eventRunner == null)
            return;

        eventRunner.OnEventClosed -= HandleEventClosed;
        eventRunner.OnEventClosed += HandleEventClosed;
    }

    /// <summary>
    /// GameManager가 런타임에 FieldManager를 추가하므로
    /// 프리팹에서 직접 연결할 수 없는 필드 시스템을 찾습니다.
    /// </summary>
    private void ResolveRuntimeReferences()
    {
        if (eventRunner == null)
        {
            eventRunner = FindFirstObjectByType<FieldEventRunner>(
                FindObjectsInactive.Include);
        }

        if (fieldEventSelectionController == null)
        {
            fieldEventSelectionController =
                FindFirstObjectByType<FieldEventSelectionController>(
                    FindObjectsInactive.Include);
        }
    }

    private void UnregisterEventRunner()
    {
        if (eventRunner == null)
            return;

        eventRunner.OnEventClosed -= HandleEventClosed;
    }

    public void StartField(List<CharacterBase> players)
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("FieldManager: 필드 참가자가 없습니다.");

            return;
        }

        if (startingNode == null)
        {
            Debug.LogWarning("FieldManager: 시작 노드가 없습니다.");

            return;
        }

        ResetFieldState();
        BindCharacterMarkers();

        foreach (CharacterBase player in players)
        {
            if (player == null)
                continue;

            player.AddAllModuleFromObject(player.gameObject);

            participants.Add(player);
            RegisterPlayerDefeatEvents(player);
            startingNode.Enter(player);
        }

        if (participants.Count == 0)
            return;

        DrawInitialFieldHands();

        IsFieldActive = true;
        currentPlayerIndex = 0;
        totalFieldTurn = 0;
        pendingBattleMythTurns.Clear();

        // 최초 진입에서는 시작 노드 이벤트를 먼저 보여 준 뒤
        // 초기 손패의 핵심 카드 이벤트를 이어서 실행합니다.
        StartFieldTurn(true);

        // 필드 초기화 직후이므로 시작 노드는 최초 방문으로 처리한다.
        if (!TryOpenNodeEvent(startingNode, true))
        {
            if (!TryOpenPendingHandCoreEvent())
            {
                CompleteFieldAction();
            }
        }
    }

    /// <summary>
    /// 이미 FieldCanvas 안에 배치된 필드 프리팹을 등록하고
    /// 지정된 플레이어들로 필드를 시작합니다.
    /// </summary>
    public bool StartExistingField(
        MissionFieldRoot fieldRoot,
        IReadOnlyList<CharacterBase> players,
        FieldMissionData mission = null)
    {
        if (IsFieldActive)
        {
            Debug.LogWarning("FieldManager: 이미 필드가 진행 중입니다.");
            return false;
        }

        if (fieldRoot == null)
        {
            Debug.LogWarning("FieldManager: 튜토리얼 필드 루트가 없습니다.");
            return false;
        }

        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("FieldManager: 필드 참가자가 없습니다.");
            return false;
        }

        ResolveRuntimeReferences();
        RegisterEventRunner();

        currentFieldObject = fieldRoot.gameObject;
        currentFieldRoot = fieldRoot;
        ownsCurrentFieldObject = false;
        currentMission = mission;
        missionProgress.Clear();
        sessionScore = 0;
        currentSessionResult = null;

        RegisterMissionField(fieldRoot);

        if (startingNode == null)
        {
            Debug.LogWarning("FieldManager: 시작 노드가 확정되지 않았습니다.");
            currentFieldObject = null;
            currentFieldRoot = null;
            ownsCurrentFieldObject = false;
            return false;
        }

        List<CharacterBase> playerList = new List<CharacterBase>();

        foreach (CharacterBase player in players)
        {
            if (player != null && !playerList.Contains(player))
            {
                playerList.Add(player);
            }
        }

        if (playerList.Count == 0)
        {
            currentFieldObject = null;
            currentFieldRoot = null;
            ownsCurrentFieldObject = false;
            return false;
        }

        StartField(playerList);

        return IsFieldActive;
    }

    private void ResetFieldState()
    {
        foreach (FieldNode node in nodes)
        {
            if (node == null)
                continue;

            node.ResetNode();
        }

        UnregisterAllPlayerDefeatEvents();
        participants.Clear();

        currentPlayer = null;
        currentNode = null;

        pendingRedLine = null;
        pendingTargetNode = null;
        pendingRedLineResult = FieldRedLineResult.None;
        pendingRedLineDestroyed = false;
        isPlayerMarkerMoving = false;

        eventRunner?.ResetCompletedEvents();
        coreEventReservations.Clear();
        forcedCoreEventReservations.Clear();
        pendingHandCoreEvent = null;
        IsFieldActive = false;
        TurnState = FieldTurnState.Inactive;
    }

    /// <summary>
    /// 현재 필드에 캐릭터 위치 표시기를 연결합니다.
    /// 프리팹에 표시기가 없으면 필드 루트에 런타임으로 추가합니다.
    /// </summary>
    private void BindCharacterMarkers()
    {
        if (currentFieldRoot == null)
            return;

        if (characterMarkers == null)
        {
            characterMarkers =
                currentFieldRoot.GetComponentInChildren<UI_FieldCharacterMarkers>(
                    true);
        }

        if (characterMarkers == null)
        {
            characterMarkers =
                currentFieldRoot.gameObject
                    .AddComponent<UI_FieldCharacterMarkers>();
        }

        characterMarkers.Bind(this);
    }

    private void StartFieldTurn(bool deferHandCoreEvent = false)
    {
        if (!IsFieldActive || participants.Count == 0)
        {
            return;
        }

        if (currentPlayerIndex >= participants.Count)
        {
            currentPlayerIndex = 0;
        }

        CharacterBase nextPlayer = participants[currentPlayerIndex];

        SetCurrentPlayer(nextPlayer);

        currentNode = FindCharacterNode(currentPlayer);

        TurnState = FieldTurnState.TurnStart;

        InitializeActionPoint(currentPlayer);

        // 필드 진입 직후에는 이미 1D4장의 초기 손패를 받았으므로
        // 두 번째 턴부터 전투와 같은 드로우 수 계산식을 사용한다.
        if (totalFieldTurn > 0)
        {
            DerivedStatModule derived =
                currentPlayer.GetModule<DerivedStatModule>();

            if (derived != null)
            {
                int requestedCount = 1 + derived.GetDrawBonus();
                int drawnCount = DrawFieldCards(currentPlayer, requestedCount);

                Debug.Log(
                    $"{currentPlayer.DisplayName} 필드 턴 드로우: " +
                    $"요청 {requestedCount}장, 실제 {drawnCount}장");
            }
            else
            {
                Debug.LogWarning(
                    $"{currentPlayer.name}: 필드 드로우에 필요한 " +
                    "DerivedStatModule이 없습니다.");
            }
        }

        TurnState = FieldTurnState.PlayerAction;

        Debug.Log($"필드 턴 시작: {currentPlayer.DisplayName}");

        WriteFieldLog("턴 시작");

        OnFieldTurnStarted?.Invoke(currentPlayer, totalFieldTurn);

        SetCurrentPlayer(nextPlayer);

        ProcessFieldHandDurability(currentPlayer);

        currentNode = FindCharacterNode(currentPlayer);

        PrepareHandCoreEvent(currentPlayer);

        if (!deferHandCoreEvent)
        {
            TryOpenPendingHandCoreEvent();
        }
    }

    /// <summary>
    /// 드로우가 끝난 실제 핸드에서 핵심 키워드 카드를 찾고
    /// 가장 높은 우선순위의 필드 이벤트 하나를 예약합니다.
    /// </summary>
    private void PrepareHandCoreEvent(CharacterBase player)
    {
        pendingHandCoreEvent = null;

        if (player == null || eventRunner == null)
            return;

        DeckModule deck = player.GetModule<DeckModule>();

        if (deck == null)
            return;

        int selectedPriority = int.MinValue;
        FieldEventContext context =
            new FieldEventContext(player, currentNode, this);

        foreach (CardInstance card in deck.HandInstances)
        {
            if (card == null || card.Data == null ||
                !card.HasKeyword(CardKeywordType.Core))
            {
                continue;
            }

            FieldEventData eventData = card.Data.HandFieldEvent;

            if (eventData == null ||
                card.Data.HandFieldEventPriority < selectedPriority ||
                !eventRunner.CanOpenEvent(eventData, context))
            {
                continue;
            }

            pendingHandCoreEvent = eventData;
            selectedPriority = card.Data.HandFieldEventPriority;
        }
    }

    private bool TryOpenPendingHandCoreEvent()
    {
        FieldEventData eventData = pendingHandCoreEvent;
        pendingHandCoreEvent = null;

        if (eventData == null)
            return false;

        bool opened = OpenFieldEvent(eventData, currentNode, false);

        if (opened)
        {
            Debug.Log(
                $"핵심 카드 핸드 이벤트 실행: {eventData.EventName}");
        }

        return opened;
    }

    /// <summary>
    /// 필드에 처음 진입할 때 모든 참가자가
    /// 자신의 덱에서 1D4장의 초기 손패를 뽑습니다.
    /// 이후 턴에는 StartFieldTurn에서 전투와 같은
    /// 1 + 지능 드로우 보너스만큼 현재 플레이어가 뽑습니다.
    /// </summary>
    private void DrawInitialFieldHands()
    {
        foreach (CharacterBase player in participants)
        {
            if (player == null)
                continue;

            int requestedCount = Dice.RollD4();
            int actualCount = DrawFieldCards(player, requestedCount);

            Debug.Log(
                $"{player.DisplayName} 필드 초기 드로우: " +
                $"1D4={requestedCount}, 실제 {actualCount}장");
        }
    }

    /// <summary>
    /// DeckModule의 기존 드로우 처리를 사용하므로
    /// 최대 손패와 덱 소진 규칙도 그대로 적용됩니다.
    /// </summary>
    private int DrawFieldCards(CharacterBase player, int drawCount)
    {
        if (player == null || drawCount <= 0)
            return 0;

        DeckModule deck = player.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning(
                $"{player.name}: 필드 초기 드로우에 필요한 " +
                "DeckModule이 없습니다.");

            return 0;
        }

        int actualCount = 0;

        for (int i = 0; i < drawCount; i++)
        {
            CardInstance drawnCard = deck.DrawInstance();

            if (drawnCard == null)
                break;

            actualCount++;
        }

        return actualCount;
    }

    private void SetCurrentPlayer(CharacterBase player)
    {
        if (currentPlayer == player)
            return;

        currentPlayer = player;

        OnCurrentPlayerChanged?.Invoke(currentPlayer);
    }

    private FieldNode FindCharacterNode(CharacterBase character)
    {
        if (character == null)
            return null;

        foreach (FieldNode node in nodes)
        {
            if (node == null)
                continue;

            if (node.ContainsCharacter(character))
                return node;
        }

        return null;
    }

    private void InitializeActionPoint(CharacterBase player)
    {
        if (player == null)
            return;

        ActionPointModule actionPoint = player.GetModule<ActionPointModule>();

        DerivedStatModule derived = player.GetModule<DerivedStatModule>();

        LVModules level = player.GetModule<LVModules>();

        if (actionPoint == null)
        {
            Debug.LogWarning($"{player.name}: ActionPointModule이 없습니다.");

            return;
        }

        if (derived == null || level == null)
        {
            Debug.LogWarning($"{player.name}: 행동력 계산 모듈이 없습니다.");

            return;
        }

        actionPoint.PrepareTurn(derived.GetAgilityModifier(), level.Level);
    }

    private void HandleNodeClicked(FieldNode clickedNode)
    {
        if (clickedNode == null)
            return;

        if (isPlayerMarkerMoving)
            return;

        if (isSelectingStartingNode)
        {
            ConfirmStartingNode(clickedNode);
            return;
        }

        if (!IsFieldActive)
            return;

        if (TurnState != FieldTurnState.PlayerAction)
            return;

        if (currentPlayer == null)
            return;

        if (clickedNode == currentNode)
        {
            TryOpenAdditionalEvent();
            return;
        }

        TryMoveToNode(clickedNode);
    }

    private void ResolveStartingNode(MissionFieldRoot fieldRoot)
    {
        startingNode = null;

        if (fieldRoot == null)
            return;

        if (fieldRoot.FixedStartingNode != null)
        {
            startingNode = fieldRoot.FixedStartingNode;

            return;
        }

        IReadOnlyList<FieldNode> candidates = fieldRoot.StartingNodeCandidates;

        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning("FieldManager: 시작 가능한 노드가 없습니다.");

            return;
        }

        if (candidates.Count == 1)
        {
            startingNode = candidates[0];
            return;
        }

        isSelectingStartingNode = true;

        OnStartingNodeSelectionRequested?.Invoke(candidates);
    }

    public bool TryMoveToNode(FieldNode targetNode)
    {
        if (!CanAttemptMove(targetNode))
            return false;

        FieldLine line = currentNode.GetLineTo(targetNode);

        if (line == null)
            return false;

        if (line.IsHidden)
            return false;

        if (line.IsBlocked)
        {
            return TryStartRedLineEvent(line, targetNode);
        }

        if (!line.CanPass)
            return false;

        if (!TryUseActionPoint(currentPlayer, 1))
        {
            Debug.Log("행동력이 부족합니다.");
            return false;
        }

        MoveCurrentPlayerTo(targetNode);

        return true;
    }

    private bool CanAttemptMove(FieldNode targetNode)
    {
        if (isPlayerMarkerMoving)
            return false;

        if (!IsFieldActive)
            return false;

        if (TurnState != FieldTurnState.PlayerAction)
        {
            return false;
        }

        if (currentPlayer == null || currentNode == null || targetNode == null)
        {
            return false;
        }

        if (targetNode == currentNode)
            return false;

        if (!targetNode.IsHiddenAreaDiscovered)
            return false;

        return currentNode.IsConnectedTo(targetNode);
    }

    private void MoveCurrentPlayerTo(FieldNode targetNode)
    {
        if (targetNode == null || currentPlayer == null)
        {
            CompleteFieldAction();
            return;
        }

        if (isPlayerMarkerMoving)
            return;

        StartCoroutine(MoveCurrentPlayerToRoutine(targetNode));
    }

    private IEnumerator MoveCurrentPlayerToRoutine(FieldNode targetNode)
    {
        CharacterBase movingPlayer = currentPlayer;
        FieldNode sourceNode = currentNode;

        // Enter()가 호출되면 FieldNode.IsVisited가 true로 변경되므로
        // 진입 전에 최초 방문 여부를 먼저 보관한다.
        bool isFirstVisit = !targetNode.IsVisited;

        isPlayerMarkerMoving = true;

        if (characterMarkers != null)
        {
            yield return characterMarkers.AnimateCharacterToNode(
                movingPlayer,
                targetNode,
                markerMoveDuration);
        }

        // 이동 중 필드가 종료되거나 현재 캐릭터가 바뀐 경우
        // 노드 진입과 이벤트 실행을 중단합니다.
        if (!IsFieldActive || currentPlayer != movingPlayer)
        {
            isPlayerMarkerMoving = false;
            yield break;
        }

        if (sourceNode != null)
        {
            sourceNode.Exit(movingPlayer);
        }

        currentNode = targetNode;
        currentNode.Enter(movingPlayer);

        isPlayerMarkerMoving = false;

        OnNodeChanged?.Invoke(currentNode);

        // 최초 방문이면 고정 진입 이벤트,
        // 재방문이면 재진입 이벤트를 실행한다.
        if (TryOpenNodeEvent(currentNode, isFirstVisit))
        {
            yield break;
        }

        // 이벤트 풀이 없거나 UI를 열지 못한 경우
        CompleteFieldAction();
    }

    private void TryOpenAdditionalEvent()
    {
        if (currentNode == null || currentPlayer == null)
        {
            return;
        }

        ActionPointModule actionPoint =
            currentPlayer.GetModule<ActionPointModule>();

        if (actionPoint == null || !actionPoint.CanUse(1))
        {
            Debug.Log("행동력이 부족합니다.");
            return;
        }

        // 완료된 1회용 이벤트처럼 실행할 수 없는 이벤트가 선택된 경우에는
        // UI도 열리지 않으므로 행동력을 소비하지 않는다.
        if (!TryOpenNodeEvent(currentNode, false))
        {
            Debug.Log(
                $"{currentNode.DisplayName}: " +
                "현재 실행 가능한 재방문 이벤트가 없습니다.");

            return;
        }

        // 재방문 이벤트 또는 이벤트 후보 UI가 실제로 열린 뒤에만 소비한다.
        if (!TryUseActionPoint(currentPlayer, 1))
        {
            Debug.LogWarning(
                "재방문 이벤트가 열린 뒤 행동력 소비에 실패했습니다.");
        }
    }

    private bool OpenFieldEvent(
        FieldEventData eventData,
        FieldNode node,
        bool ignoreCompletionHistory)
    {
        if (eventData == null || node == null || currentPlayer == null || eventRunner == null)
        {
            return false;
        }

        FieldEventContext context = new FieldEventContext(currentPlayer, node, this);

        bool opened = eventRunner.OpenEvent(
            eventData,
            context,
            ignoreCompletionHistory);

        if (opened)
        {
            TurnState = FieldTurnState.Event;
        }

        return opened;
    }

    private void HandleEventClosed()
    {
        if (!IsFieldActive)
            return;

        if (TurnState != FieldTurnState.Event)
            return;

        if (TryCompleteConditionalEnding())
            return;

        if (TryOpenPendingHandCoreEvent())
            return;

        if (pendingRedLine != null)
        {
            bool opened =
                pendingRedLineResult == FieldRedLineResult.Open;

            if (!pendingRedLineDestroyed &&
                pendingRedLineResult == FieldRedLineResult.None)
            {
                Debug.LogWarning(
                    "적색 라인 이벤트 결과가 지정되지 않아 Locked로 처리합니다.");
            }

            StartCoroutine(
                CompleteRedLineAfterEventClosed(opened));
            return;
        }

        CompleteFieldAction();
    }

    private bool TryStartRedLineEvent(FieldLine line, FieldNode targetNode)
    {
        if (line == null || targetNode == null)
        {
            return false;
        }

        if (eventRunner == null)
        {
            Debug.LogWarning("적색 라인 접근 이벤트를 실행할 FieldEventRunner가 없습니다.");
            return false;
        }

        // 접근 이벤트는 실제로 클릭한 Red 라인에 등록된 후보를 사용합니다.
        FieldEventContext context =
            new FieldEventContext(currentPlayer, targetNode, this);

        FieldEventData redLineEvent =
            SelectRedLineAccessEvent(line, context);

        if (redLineEvent == null)
        {
            Debug.LogWarning(
                $"{line.LineId}: 조건을 만족하는 적색 라인 이벤트가 없습니다.");

            return false;
        }

        if (!TryUseActionPoint(currentPlayer, 1))
        {
            Debug.Log("행동력이 부족합니다.");
            return false;
        }

        pendingRedLine = line;
        pendingTargetNode = targetNode;
        pendingRedLineResult = FieldRedLineResult.None;
        pendingRedLineDestroyed = false;
        isPlayerMarkerMoving = false;

        bool opened = eventRunner.OpenEvent(
            redLineEvent,
            context,
            false);

        if (!opened)
        {
            pendingRedLine = null;
            pendingTargetNode = null;
            pendingRedLineResult = FieldRedLineResult.None;
            pendingRedLineDestroyed = false;

            Debug.LogWarning(
                $"{targetNode.DisplayName}: 적색 라인 접근 이벤트를 열지 못했습니다.");

            return false;
        }

        TurnState = FieldTurnState.Event;

        return true;
    }

    private FieldEventData SelectRedLineAccessEvent(
        FieldLine line,
        FieldEventContext context)
    {
        if (line == null || eventRunner == null)
            return null;

        IReadOnlyList<FieldLine.RedLineAccessEventEntry> entries =
            line.RedLineAccessEvents;

        if (entries == null)
            return null;

        FieldEventData selectedEvent = null;
        int selectedPriority = int.MinValue;

        // 같은 우선순위에서는 먼저 등록된 항목을 유지합니다.
        foreach (FieldLine.RedLineAccessEventEntry entry in entries)
        {
            if (entry == null || entry.EventData == null)
                continue;

            if (!eventRunner.CanOpenEvent(
                    entry.EventData,
                    context,
                    false))
            {
                continue;
            }

            if (selectedEvent != null &&
                entry.Priority <= selectedPriority)
            {
                continue;
            }

            selectedEvent = entry.EventData;
            selectedPriority = entry.Priority;
        }

        return selectedEvent;
    }

    /// <summary>
    /// 선택지 결과가 현재 처리 중인 적색 라인의 상태를 결정합니다.
    /// 실제 라인 변경과 이동은 결과창을 닫은 뒤 실행합니다.
    /// </summary>
    public void SetPendingRedLineResult(FieldRedLineResult result)
    {
        if (pendingRedLine == null || result == FieldRedLineResult.None)
            return;

        // 파괴된 라인은 이번 행동에서 이동하지 않는 규칙이 우선합니다.
        if (pendingRedLineDestroyed)
            return;

        pendingRedLineResult = result;
    }

    /// <summary>
    /// 현재 진행 중인 적색 라인 이벤트의 라인에 피해를 줍니다.
    /// 체력이 0이 되면 라인은 Normal이 되지만 이번 이동은 완료하지 않습니다.
    /// </summary>
    public bool TryDamagePendingRedLine(
        int amount,
        out FieldLine damagedLine,
        out int appliedDamage)
    {
        damagedLine = pendingRedLine;
        appliedDamage = 0;

        if (damagedLine == null || amount <= 0 || !damagedLine.IsBlocked)
            return false;

        appliedDamage = damagedLine.TakeDamage(amount);

        if (appliedDamage <= 0)
            return false;

        if (!damagedLine.IsBlocked)
        {
            pendingRedLineDestroyed = true;
            pendingRedLineResult = FieldRedLineResult.None;
        }

        return true;
    }

    private IEnumerator CompleteRedLineAfterEventClosed(bool opened)
    {
        // 모든 이벤트 UI의 닫기 콜백이 끝난 다음 프레임에 이동합니다.
        yield return null;

        if (!IsFieldActive || pendingRedLine == null)
            yield break;

        CompleteRedLineEvent(opened);
    }

    /// <summary>
    /// 적색 라인 이벤트 처리 완료 시 호출
    /// </summary>
    public void CompleteRedLineEvent(bool passed)
    {
        if (pendingRedLine == null)
            return;

        FieldLine resolvedLine = pendingRedLine;

        FieldNode targetNode = pendingTargetNode;

        pendingRedLine = null;
        pendingTargetNode = null;
        pendingRedLineResult = FieldRedLineResult.None;
        bool wasDestroyed = pendingRedLineDestroyed;
        pendingRedLineDestroyed = false;
        isPlayerMarkerMoving = false;

        // 적색 라인 이벤트가 끝났으므로
        // 일반 행동 상태로 먼저 복귀
        TurnState = FieldTurnState.PlayerAction;

        if (passed && !wasDestroyed)
        {
            resolvedLine.ClearBlock();

            // 이동 후 해당 노드의 이벤트 후보가 공개됨
            MoveCurrentPlayerTo(targetNode);

            return;
        }
        // 적색 라인 해제 실패
        CompleteFieldAction();
    }

    public bool TryUseActionPoint(CharacterBase player, int amount = 1)
    {
        if (!IsFieldActive)
            return false;

        if (player == null || player != currentPlayer)
        {
            return false;
        }

        ActionPointModule actionPoint = player.GetModule<ActionPointModule>();

        if (actionPoint == null)
            return false;

        return actionPoint.TryUse(amount);
    }

    /// <summary>
    /// 일반 필드 카드 사용이 완전히 끝난 뒤 호출
    /// </summary>
    public void CompleteCardAction()
    {
        if (!IsFieldActive)
            return;

        if (TurnState != FieldTurnState.PlayerAction)
            return;

        CompleteFieldAction();
    }

    private void CompleteFieldAction()
    {
        if (!IsFieldActive || currentPlayer == null)
        {
            return;
        }

        TurnState = FieldTurnState.PlayerAction;

        ActionPointModule actionPoint = currentPlayer.GetModule<ActionPointModule>();

        if (actionPoint == null)
            return;

        // 행동력 0은 추가 이벤트 조건이 아님
        // 즉시 턴 종료 절차로 이동
        if (actionPoint.IsEmpty)
        {
            EndFieldTurn();
        }
    }

    public bool TryEndFieldTurn()
    {
        if (!IsFieldActive ||
            TurnState != FieldTurnState.PlayerAction)
        {
            return false;
        }

        EndFieldTurn();
        return true;
    }

    public void EndFieldTurn()
    {
        if (!IsFieldActive)
            return;

        if (TurnState != FieldTurnState.PlayerAction)
        {
            return;
        }

        TurnState = FieldTurnState.TurnEnd;

        totalFieldTurn++;

        Debug.Log($"필드 턴 종료 / 누적 턴:{totalFieldTurn}");

        WriteFieldLog("턴 종료");

        int interval = Mathf.Max(1, mythTurnInterval);

        if (totalFieldTurn % interval != 0)
        {
            int remaining = interval - totalFieldTurn % interval;
            WriteFieldLog($"신화 턴까지 {remaining}턴 남았습니다.");
        }

        if (totalFieldTurn % interval == 0)
        {
            StartMythTurn();
            return;
        }

        CheckPlayersAndStartNextTurn();
    }

    private void StartMythTurn()
    {
        StartMythTurn(totalFieldTurn);
    }

    private void StartMythTurn(int mythTurnNumber)
    {
        TurnState = FieldTurnState.MythTurn;

        Debug.Log($"신화 턴 발생: {mythTurnNumber}");
        WriteFieldLog("신화 턴 시작");

        if (OnMythTurnRequested != null)
        {
            OnMythTurnRequested.Invoke(mythTurnNumber);

            return;
        }

        // 아직 신화 이벤트 시스템이 없다면
        // 바로 사망 확인으로 이동
        CompleteMythTurn();
    }

    /// <summary>
    /// 신화 이벤트와 연출이 모두 끝났을 때 호출
    /// </summary>
    public void CompleteMythTurn()
    {
        if (TurnState != FieldTurnState.MythTurn)
        {
            return;
        }

        if (pendingBattleMythTurns.Count > 0)
        {
            StartMythTurn(pendingBattleMythTurns.Dequeue());
            return;
        }

        CheckPlayersAndStartNextTurn();
    }

    /// <summary>
    /// 필드에서 시작된 전투가 끝났을 때 전투 라운드만큼
    /// 필드 누적 턴을 진행시킵니다.
    /// 전투 중 통과한 신화 턴 경계는 필드 복귀 후 순서대로 처리합니다.
    /// </summary>
    public void ApplyBattleElapsedRounds(int elapsedRounds)
    {
        if (!IsFieldActive || elapsedRounds <= 0)
            return;

        int previousTurn = totalFieldTurn;
        int interval = Mathf.Max(1, mythTurnInterval);

        totalFieldTurn += elapsedRounds;

        Debug.Log(
            $"전투 경과 반영: {elapsedRounds}라운드 / " +
            $"필드 누적 턴 {previousTurn} -> {totalFieldTurn}");

        // 현재 필드 턴을 포함해 전투 라운드 수만큼 참가 순서를 넘깁니다.
        // 마지막 한 번은 CheckPlayersAndStartNextTurn에서 처리하므로
        // 여기서는 elapsedRounds - 1회만 미리 이동합니다.
        for (int i = 1; i < elapsedRounds; i++)
        {
            MoveToNextPlayer();
        }

        int firstMythTurn =
            ((previousTurn / interval) + 1) * interval;

        for (int mythTurn = firstMythTurn;
             mythTurn <= totalFieldTurn;
             mythTurn += interval)
        {
            pendingBattleMythTurns.Enqueue(mythTurn);
        }

        if (pendingBattleMythTurns.Count > 0)
        {
            StartMythTurn(pendingBattleMythTurns.Dequeue());
            return;
        }

        CheckPlayersAndStartNextTurn();
    }

    private void CheckPlayersAndStartNextTurn()
    {
        if (HasDeadPlayer())
        {
            EndFieldByGameOver();
            return;
        }

        MoveToNextPlayer();
        StartFieldTurn();
    }

    private bool HasDeadPlayer()
    {
        foreach (CharacterBase player in participants)
        {
            if (player == null)
                return true;

            HitpointModules hp = player.GetModule<HitpointModules>();

            if (hp != null && hp.IsEmpty)
                return true;

            SanityModule sanity = player.GetModule<SanityModule>();

            if (sanity != null && sanity.IsEmpty)
                return true;
        }

        return false;
    }

    private void MoveToNextPlayer()
    {
        if (participants.Count == 0)
            return;

        currentPlayerIndex++;

        if (currentPlayerIndex >= participants.Count)
        {
            currentPlayerIndex = 0;
        }
    }

    private void EndFieldByGameOver()
    {
        if (!IsFieldActive || TurnState == FieldTurnState.GameOver)
        {
            return;
        }

        CompleteSession(true);
    }

    public void EndField()
    {
        // 이벤트 콜백이 다시 진행되지 않도록
        // 가장 먼저 필드를 비활성화
        IsFieldActive = false;
        TurnState = FieldTurnState.Inactive;

        UnregisterAllPlayerDefeatEvents();
        UnregisterNodes();

        pendingRedLine = null;
        pendingTargetNode = null;
        pendingRedLineResult = FieldRedLineResult.None;
        pendingRedLineDestroyed = false;
        isPlayerMarkerMoving = false;

        if (characterMarkers != null)
        {
            characterMarkers.ClearMarkers();
            characterMarkers.Unbind();
            characterMarkers = null;
        }

        eventRunner?.CloseEvent();

        foreach (FieldNode node in nodes)
        {
            if (node == null)
                continue;

            node.ResetNode();
        }

        currentPlayer = null;
        currentNode = null;

        participants.Clear();
        nodes.Clear();

        startingNode = null;
        isSelectingStartingNode = false;

        coreEventReservations.Clear();
        forcedCoreEventReservations.Clear();
        pendingHandCoreEvent = null;
        missionProgress.Clear();
        sessionScore = 0;
        currentSessionResult = null;

        ReleaseCurrentFieldObject();

        currentMission = null;
    }

    public bool SetStartingNode(FieldNode node)
    {
        if (IsFieldActive)
        {
            Debug.LogWarning("필드가 시작된 뒤에는 시작 노드를 변경할 수 없습니다.");

            return false;
        }

        if (node == null)
            return false;

        if (!nodes.Contains(node))
        {
            Debug.LogWarning("필드에 등록되지 않은 노드입니다.");

            return false;
        }

        startingNode = node;

        return true;
    }

    public void SetStartingNode(MissionFieldRoot fieldRoot)
    {

        startingNode = null;

        if (fieldRoot.FixedStartingNode != null)
        {
            startingNode = fieldRoot.FixedStartingNode;

            return;
        }

        IReadOnlyList<FieldNode> candidates = fieldRoot.StartingNodeCandidates;

        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning("FieldManager: 시작 가능한 노드가 없습니다.");

            return;
        }

        if (candidates.Count == 1)
        {
            startingNode = candidates[0];
            return;
        }

        // 시작 후보가 여러 개라면
        // 플레이어가 UI에서 선택
        OnStartingNodeSelectionRequested?.Invoke(candidates);
    }

    private bool isSelectingStartingNode;

    public void BeginStartingNodeSelection()
    {
        if (IsFieldActive)
            return;

        startingNode = null;
        isSelectingStartingNode = true;
    }

    private void HandleMissionSelected(FieldMissionData mission)
    {
        if (mission == null)
            return;

        LoadMissionField(mission);
    }

    public bool LoadMissionField(FieldMissionData mission)
    {
        if (mission == null)
        {
            Debug.LogWarning("FieldManager: 미션 데이터가 없습니다.");

            return false;
        }

        if (fieldCore == null)
        {
            Debug.LogWarning("FieldManager: FieldCore가 등록되지 않았습니다.");

            return false;
        }

        if (string.IsNullOrWhiteSpace(mission.FieldObjectName))
        {
            Debug.LogWarning($"{mission.MissionName}: " + "필드 오브젝트 이름이 없습니다.");

            return false;
        }

        if (currentFieldObject != null)
        {
            Debug.LogWarning("FieldManager: 이미 불러온 필드가 있습니다.");

            return false;
        }

        GameObject fieldObject = ObjectManager.CreateObject(mission.FieldObjectName, fieldCore);

        if (fieldObject == null)
        {
            Debug.LogWarning($"필드를 불러오지 못했습니다: " + $"{mission.FieldObjectName}");

            return false;
        }

        MissionFieldRoot fieldRoot = fieldObject.GetComponent<MissionFieldRoot>();

        if (fieldRoot == null)
        {
            Debug.LogWarning($"{mission.FieldObjectName}: " + "MissionFieldRoot가 없습니다.");

            return false;
        }

        currentFieldObject = fieldObject;
        currentFieldRoot = fieldRoot;
        ownsCurrentFieldObject = true;

        currentMission = mission;
        missionProgress.Clear();
        sessionScore = 0;
        currentSessionResult = null;

        RegisterMissionField(fieldRoot);

        OnMissionFieldLoaded?.Invoke(fieldRoot);

        return true;
    }


    private void RegisterLoadedField(MissionFieldRoot fieldRoot)
    {
        if (fieldRoot == null)
            return;

        UnregisterNodes();

        nodes.Clear();

        fieldRoot.DetectFieldObjects();


        foreach (FieldNode node in fieldRoot.Nodes)
        {
            if (node == null)
                continue;

            nodes.Add(node);

            node.OnClicked -= HandleNodeClicked;

            node.OnClicked += HandleNodeClicked;
        }

        ResolveStartingNode(fieldRoot);
    }

    private void FindStartingNode()
    {
        startingNode = null;

        foreach (FieldNode node in nodes)
        {
            if (node == null)
                continue;

            if (node.CanBeStartingNode)
            {
                startingNode = node;
                return;
            }
        }
    }

    public void SetFieldCore(Transform newFieldCore)
    {
        fieldCore = newFieldCore;
    }

    private void RegisterMissionField(MissionFieldRoot fieldRoot)
    {
        if (fieldRoot == null)
            return;

        UnregisterNodes();

        nodes.Clear();

        fieldRoot.DetectFieldObjects();

        foreach (FieldNode node in fieldRoot.Nodes)
        {
            if (node == null)
                continue;

            nodes.Add(node);

            node.OnClicked -= HandleNodeClicked;

            node.OnClicked += HandleNodeClicked;
        }

        ResolveStartingNode(fieldRoot);
    }


    public bool ConfirmStartingNode(FieldNode selectedNode)
    {
        if (selectedNode == null)
            return false;

        if (!selectedNode.CanBeStartingNode)
            return false;

        if (!nodes.Contains(selectedNode))
            return false;

        startingNode = selectedNode;
        isSelectingStartingNode = false;

        Debug.Log($"시작 노드 선택: " + $"{selectedNode.DisplayName}");

        OnStartingNodeConfirmed?.Invoke(startingNode);

        return true;
    }

    /// <summary>
    /// 해당 캐릭터의 다음 이벤트 후보에
    /// 핵심 이벤트가 포함되도록 예약한다.
    /// </summary>
    public void ReserveCoreEventForNextSelection(CharacterBase character)
    {
        if (character == null)
            return;

        coreEventReservations.Add(character);

        Debug.Log($"{character.name}: 다음 이벤트 후보에 핵심 이벤트 포함 예약");
    }

    /// <summary>
    /// 핵심 이벤트 예약 여부 확인.
    /// 예약을 제거하지 않는다.
    /// </summary>
    public bool HasCoreEventReservation(CharacterBase character)
    {
        if (character == null)
            return false;

        return coreEventReservations.Contains(character);
    }

    /// <summary>
    /// 다음 이벤트 후보를 만들 때 호출한다.
    /// 예약이 있다면 제거하고 true를 반환한다.
    /// </summary>
    public bool ConsumeCoreEventReservation(CharacterBase character)
    {
        if (character == null)
            return false;

        return coreEventReservations.Remove(character);
    }

    /// <summary>
    /// 다음 이벤트가 핵심 이벤트로 예약되어 있다면 우선 실행한다.
    /// 예약이 없다면 최초 방문 이벤트, 재방문 이벤트,
    /// 공통 이벤트 순서로 처리한다.
    /// </summary>
    /// <param name="node">이벤트가 발생할 노드</param>
    /// <param name="isFirstVisit">최초 방문 여부</param>
    /// <returns>이벤트를 정상적으로 열었으면 true</returns>
    public bool TryOpenNodeEvent(FieldNode node, bool isFirstVisit)
    {
        if (!IsFieldActive)
            return false;

        if (node == null || currentPlayer == null)
        {
            return false;
        }

        if (TurnState == FieldTurnState.Event)
            return false;

        // 핵심 이벤트 확정 예약이 있다면
        // 최초 방문 이벤트보다 먼저 실행한다.
        if (TryOpenForcedCoreEvent(node))
        {
            return true;
        }

        FieldEventData nodeEvent = isFirstVisit ? GetRandomAvailableFirstVisitEvent(node) : GetRandomAvailableRepeatEvent(node);

        if (nodeEvent != null)
        {
            if (OpenFieldEvent(nodeEvent, node, isFirstVisit))
            {
                Debug.Log(
                    $"노드 이벤트 실행: " +
                    $"{node.DisplayName} / " +
                    $"{nodeEvent.EventName}");

                return true;
            }
        }

        if (fieldEventSelectionController == null)
        {
            Debug.LogWarning(
                "FieldManager: " +
                "FieldEventSelectionController가 없습니다.");

            return false;
        }

        if (fieldEventSelectionController.IsSelecting)
            return false;

        FieldEventContext context =
            new FieldEventContext(currentPlayer, node, this);

        bool opened = fieldEventSelectionController.OpenNextEventSelection(context);

        if (!opened)
            return false;

        TurnState = FieldTurnState.Event;

        Debug.Log(
            $"노드 이벤트 후보 공개: " +
            $"{node.DisplayName}");

        return true;
    }

    public void WriteFieldLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        OnFieldLog?.Invoke(message);
    }

    /// <summary>
    /// 최초 진입 이벤트 중 발동 조건을 만족하고 아직 사용할 수 있는 이벤트만
    /// 추린 뒤 하나를 무작위로 반환합니다.
    /// </summary>
    private FieldEventData GetRandomAvailableFirstVisitEvent(FieldNode node)
    {
        if (node == null)
            return null;

        return GetRandomAvailableNodeEvent(
            node,
            node.FirstVisitEvents);
    }

    /// <summary>
    /// 재방문 이벤트 중 발동 조건을 만족하고 아직 사용할 수 있는 이벤트만
    /// 추린 뒤 하나를 무작위로 반환합니다. 완료된 Once Per Field 이벤트는
    /// 재방문 배열에 남아 있어도 다시 추첨되지 않습니다.
    /// </summary>
    private FieldEventData GetRandomAvailableRepeatEvent(FieldNode node)
    {
        if (node == null)
            return null;

        return GetRandomAvailableNodeEvent(
            node,
            node.RepeatEvents);
    }

    private FieldEventData GetRandomAvailableNodeEvent(
        FieldNode node,
        IReadOnlyList<FieldEventData> eventCandidates)
    {
        if (node == null || currentPlayer == null || eventRunner == null)
            return null;

        if (eventCandidates == null || eventCandidates.Count == 0)
            return null;

        FieldEventContext context =
            new FieldEventContext(currentPlayer, node, this);

        List<FieldEventData> availableEvents = new List<FieldEventData>();

        foreach (FieldEventData eventData in eventCandidates)
        {
            if (eventData == null || availableEvents.Contains(eventData))
                continue;

            if (!eventRunner.CanOpenEvent(eventData, context))
                continue;

            availableEvents.Add(eventData);
        }

        if (availableEvents.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, availableEvents.Count);

        return availableEvents[randomIndex];
    }

    private void RegisterPlayerDefeatEvents(CharacterBase player)
    {
        if (player == null)
            return;

        HitpointModules hp = player.GetModule<HitpointModules>();

        if (hp != null)
        {
            hp.OnEmpty -= HandlePlayerDefeated;
            hp.OnEmpty += HandlePlayerDefeated;
        }

        SanityModule sanity = player.GetModule<SanityModule>();

        if (sanity != null)
        {
            sanity.OnEmpty -= HandlePlayerDefeated;
            sanity.OnEmpty += HandlePlayerDefeated;
        }
    }

    private void UnregisterPlayerDefeatEvents(CharacterBase player)
    {
        if (player == null)
            return;

        HitpointModules hp = player.GetModule<HitpointModules>();

        if (hp != null)
        {
            hp.OnEmpty -= HandlePlayerDefeated;
        }

        SanityModule sanity = player.GetModule<SanityModule>();

        if (sanity != null)
            sanity.OnEmpty -= HandlePlayerDefeated;

    }

    private void UnregisterAllPlayerDefeatEvents()
    {
        foreach (CharacterBase player in participants)
        {
            UnregisterPlayerDefeatEvents(player);
        }
    }

    private void HandlePlayerDefeated()
    {
        if (!IsFieldActive)
            return;

        if (TurnState == FieldTurnState.GameOver)
            return;

        if (HasDeadPlayer())
        {
            EndFieldByGameOver();
        }
    }

    public bool AddMissionProgress(string objectiveId, int amount = 1)
    {
        if (!IsFieldActive)
            return false;

        if (currentMission == null)
            return false;

        if (string.IsNullOrWhiteSpace(objectiveId))
            return false;

        FieldMissionObjectiveRequirement requirement = FindMissionObjective(objectiveId);

        if (requirement == null)
        {
            Debug.LogWarning($"현재 미션에 존재하지 않는 목표입니다: {objectiveId}");

            return false;
        }

        missionProgress.TryGetValue(objectiveId, out int currentAmount);

        int newAmount = Mathf.Clamp(currentAmount + Mathf.Max(0, amount), 0, requirement.RequiredAmount);

        missionProgress[objectiveId] = newAmount;

        OnMissionProgressChanged?.Invoke(objectiveId, newAmount, requirement.RequiredAmount);

        Debug.Log($"미션 진행: {objectiveId} / " + $"{newAmount}/{requirement.RequiredAmount}");

        return true;
    }

    public void AddSessionScore(int amount)
    {
        if (!IsFieldActive || amount == 0)
            return;

        sessionScore += amount;
        OnSessionScoreChanged?.Invoke(sessionScore);
    }

    public bool RequestSessionEnd()
    {
        if (!IsFieldActive)
            return false;

        CompleteSession(false);
        return true;
    }

    private void CompleteSession(
        bool isDeath,
        FieldSessionEndingData forcedEnding = null)
    {
        if (!IsFieldActive)
            return;

        FieldMissionData completedMission = currentMission;
        FieldSessionEndingData ending = forcedEnding != null
            ? forcedEnding
            : ResolveSessionEnding(isDeath);

        IsFieldActive = false;
        TurnState = isDeath
            ? FieldTurnState.GameOver
            : FieldTurnState.MissionClear;

        UnregisterAllPlayerDefeatEvents();

        FieldSessionResult result = new FieldSessionResult(
            completedMission,
            ending,
            sessionScore,
            isDeath);

        currentSessionResult = result;

        OnSessionEnded?.Invoke(result);

        // 기존 연결을 깨지 않기 위한 호환 이벤트입니다.
        if (isDeath)
            OnFieldGameOver?.Invoke();
        else
            OnMissionCleared?.Invoke(completedMission);
    }

    private bool TryCompleteConditionalEnding()
    {
        if (currentMission == null || currentPlayer == null)
            return false;

        IReadOnlyList<FieldSessionEndingRequirement> requirements =
            currentMission.ConditionalEndings;

        if (requirements == null)
            return false;

        foreach (FieldSessionEndingRequirement requirement in requirements)
        {
            if (requirement == null || !requirement.IsSatisfied(currentPlayer))
                continue;

            CompleteSession(false, requirement.Ending);
            return true;
        }

        return false;
    }

    private FieldSessionEndingData ResolveSessionEnding(bool isDeath)
    {
        if (currentMission == null)
            return null;

        if (isDeath)
            return currentMission.DeathEnding;

        IReadOnlyList<FieldSessionEndingData> endings =
            currentMission.SessionEndings;

        if (endings == null)
            return null;

        FieldSessionEndingData selectedEnding = null;

        foreach (FieldSessionEndingData ending in endings)
        {
            if (ending == null || !ending.ContainsScore(sessionScore))
                continue;

            if (selectedEnding == null ||
                ending.MinimumScore > selectedEnding.MinimumScore)
            {
                selectedEnding = ending;
            }
        }

        if (selectedEnding != null)
            return selectedEnding;

        Debug.LogWarning(
            $"현재 점수 {sessionScore}에 해당하는 세션 엔딩이 없습니다.");

        return null;
    }

    private FieldMissionObjectiveRequirement FindMissionObjective(string objectiveId)
    {
        if (currentMission == null || currentMission.Objectives == null)
        {
            return null;
        }

        foreach (FieldMissionObjectiveRequirement objective in currentMission.Objectives)
        {
            if (objective == null)
                continue;

            if (objective.ObjectiveId == objectiveId)
                return objective;
        }

        return null;
    }

    private bool IsCurrentMissionClear()
    {
        if (currentMission == null)
            return false;

        IReadOnlyList<FieldMissionObjectiveRequirement> objectives =
            currentMission.Objectives;

        // 목표가 없는 미션은 자동 클리어하지 않음
        if (objectives == null || objectives.Count == 0)
            return false;

        foreach (FieldMissionObjectiveRequirement objective
                 in objectives)
        {
            if (objective == null)
                continue;

            missionProgress.TryGetValue(
                objective.ObjectiveId,
                out int currentAmount);

            if (currentAmount < objective.RequiredAmount)
                return false;
        }

        return true;
    }

    private bool TryCompleteCurrentMission()
    {
        if (!IsFieldActive)
            return false;

        if (!IsCurrentMissionClear())
            return false;

        FieldMissionData completedMission = currentMission;

        IsFieldActive = false;
        TurnState = FieldTurnState.MissionClear;

        UnregisterAllPlayerDefeatEvents();

        Debug.Log(
            $"미션 클리어: {completedMission.MissionName}"
        );

        OnMissionCleared?.Invoke(completedMission);

        return true;
    }

    private void ReleaseCurrentFieldObject()
    {
        if (currentFieldObject == null)
        {
            currentFieldRoot = null;
            ownsCurrentFieldObject = false;
            return;
        }

        if (ownsCurrentFieldObject)
        {
            PooledObject pooled =
                currentFieldObject.GetComponent<PooledObject>();

            if (pooled != null)
            {
                pooled.OnEnqueue();
            }
            else
            {
                Destroy(currentFieldObject);
            }
        }

        currentFieldObject = null;
        currentFieldRoot = null;
        ownsCurrentFieldObject = false;
    }

    /// <summary>
    /// 지정한 캐릭터의 다음 이벤트가
    /// 핵심 이벤트로 실행되도록 예약한다.
    /// </summary>
    /// <param name="character">핵심 이벤트를 예약할 캐릭터</param>
    public void ReserveCoreEventForNextEvent(CharacterBase character)
    {
        if (character == null)
            return;

        forcedCoreEventReservations.Add(character);

        Debug.Log(
            $"{character.name}: 다음 이벤트를 핵심 이벤트로 예약");
    }

    /// <summary>
    /// 지정한 캐릭터에게 핵심 이벤트 확정 예약이 있는지 확인한다.
    /// </summary>
    /// <param name="character">확인할 캐릭터</param>
    /// <returns>다음 이벤트가 핵심 이벤트로 예약되어 있으면 true</returns>
    public bool HasForcedCoreEventReservation(CharacterBase character)
    {
        if (character == null)
            return false;

        return forcedCoreEventReservations.Contains(character);
    }

    /// <summary>
    /// 현재 필드 이벤트 목록에서 실행 가능한 핵심 이벤트를 찾아
    /// 일반 노드 이벤트보다 먼저 실행한다.
    /// </summary>
    /// <param name="node">이벤트를 실행할 현재 노드</param>
    /// <returns>핵심 이벤트를 실행했으면 true</returns>
    private bool TryOpenForcedCoreEvent(FieldNode node)
    {
        if (node == null || currentPlayer == null || currentFieldRoot == null)
        {
            return false;
        }

        if (!forcedCoreEventReservations.Contains(currentPlayer))
        {
            return false;
        }

        IReadOnlyList<FieldEventData> eventPool = currentFieldRoot.EventPool;

        if (eventPool == null || eventPool.Count == 0)
        {
            Debug.LogWarning(
                "핵심 이벤트 확정 실패: 필드 이벤트 목록이 비어 있습니다.");

            return false;
        }

        List<FieldEventData> coreEvents = new List<FieldEventData>();

        foreach (FieldEventData eventData in eventPool)
        {
            if (eventData == null)
                continue;

            if (eventData.EventType != FieldEventType.Core)
                continue;

            if (coreEvents.Contains(eventData))
                continue;

            coreEvents.Add(eventData);
        }

        if (coreEvents.Count == 0)
        {
            Debug.LogWarning(
                "핵심 이벤트 확정 실패: " +
                "Event Pool에 Core 이벤트가 없습니다.");

            return false;
        }

        while (coreEvents.Count > 0)
        {
            int randomIndex =
                UnityEngine.Random.Range(0, coreEvents.Count);

            FieldEventData selectedEvent = coreEvents[randomIndex];

            coreEvents.RemoveAt(randomIndex);

            if (!OpenFieldEvent(
                selectedEvent,
                node,
                false))
            {
                continue;
            }

            // 실제로 핵심 이벤트가 열렸을 때만 예약을 소비한다.
            forcedCoreEventReservations.Remove(currentPlayer);

            Debug.Log(
                $"핵심 이벤트 확정 실행: " +
                $"{selectedEvent.EventName}");

            return true;
        }

        Debug.LogWarning(
            "핵심 이벤트 확정 실패: " +
            "현재 실행할 수 있는 Core 이벤트가 없습니다.");

        return false;
    }

    /// <summary>
    /// 필드에서 현재 플레이어의 턴이 시작될 때
    /// 활성 광원·점화 카드의 내구도를 감소시킵니다.
    /// </summary>
    private void ProcessFieldHandDurability(CharacterBase player)
    {
        if (player == null)
            return;

        DeckModule deck = player.GetModule<DeckModule>();

        if (deck == null)
            return;

        int removedCount = deck.ProcessHandTurnDurability();


        if (removedCount <= 0)
            return;

        if (handUI == null)
        {
            Canvas fieldCanvas = currentFieldRoot != null
                ? currentFieldRoot.GetComponentInParent<Canvas>(true)
                : null;

            if (fieldCanvas != null)
            {
                handUI = fieldCanvas.GetComponentInChildren<UI_Hand>(true);
            }
        }

        if (handUI != null)
        {
            handUI.RefreshFromDeck(deck);
        }
    }

}
