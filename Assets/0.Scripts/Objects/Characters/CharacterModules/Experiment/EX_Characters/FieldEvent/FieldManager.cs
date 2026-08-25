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

    public int MythTurnInterval => mythTurnInterval;

    private readonly List<CharacterBase> participants = new();

    private CharacterBase currentPlayer;
    private FieldNode currentNode;

    private int currentPlayerIndex;
    private int totalFieldTurn;

    private FieldLine pendingRedLine;
    private FieldNode pendingTargetNode;

    private Transform fieldCore;
    private GameObject currentFieldObject;
    private MissionFieldRoot currentFieldRoot;

    private readonly List<FieldNode> startingNodeCandidates = new();
    private readonly HashSet<CharacterBase> coreEventReservations = new();
    /// <summary>
    /// 다음 이벤트를 핵심 이벤트로 확정한 캐릭터 목록이다.
    /// </summary>
    private readonly HashSet<CharacterBase> forcedCoreEventReservations = new();

    public IReadOnlyList<FieldNode> StartingNodeCandidates => startingNodeCandidates;

    private FieldMissionData currentMission;

    private readonly Dictionary<string, int> missionProgress = new();

    public FieldMissionData CurrentMission => currentMission;
    public GameObject CurrentFieldObject => currentFieldObject;
    public CharacterBase CurrentPlayer => currentPlayer;
    public FieldNode CurrentNode => currentNode;
    public MissionFieldRoot CurrentFieldRoot => currentFieldRoot;
    public bool HasStartingNode => startingNode != null;

    public IReadOnlyList<FieldNode> Nodes => nodes;

    public bool IsSelectingStartingNode => isSelectingStartingNode;


    public IReadOnlyList<CharacterBase> Participants => participants;

    public int TotalFieldTurn => totalFieldTurn;

    public bool IsFieldActive { get; private set; }

    public FieldTurnState TurnState { get; private set; } = FieldTurnState.Inactive;

    public event Action<CharacterBase> OnCurrentPlayerChanged;
    public event Action<FieldNode> OnNodeChanged;
    public event Action<FieldLine, FieldNode> OnRedLineEventRequested;
    public event Action<int> OnMythTurnRequested;
    public event Action OnFieldGameOver;
    public event Action<MissionFieldRoot> OnMissionFieldLoaded;
    public event Action<IReadOnlyList<FieldNode>> OnStartingNodeSelectionRequested;
    public event Action OnMadnessEntered;
    public event Action<string, int, int> OnMissionProgressChanged;
    public event Action<FieldMissionData> OnMissionCleared;
    public event Action<FieldNode> OnStartingNodeConfirmed;
    public event Action<CharacterBase, int> OnFieldTurnStarted;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
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

        IsFieldActive = true;
        currentPlayerIndex = 0;
        totalFieldTurn = 0;

        StartFieldTurn();

        // 필드 초기화 직후이므로 시작 노드는 최초 방문으로 처리한다.
        if (!TryOpenNodeEvent(startingNode, true))
        {
            CompleteFieldAction();
        }
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

        eventRunner?.ResetCompletedEvents();
        coreEventReservations.Clear();
        forcedCoreEventReservations.Clear();
        IsFieldActive = false;
        TurnState = FieldTurnState.Inactive;
    }

    private void StartFieldTurn()
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

        TurnState = FieldTurnState.PlayerAction;

        Debug.Log($"필드 턴 시작: {currentPlayer.DisplayName}");

        OnFieldTurnStarted?.Invoke(currentPlayer, totalFieldTurn);

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

        int levelDice;

        if (level.Level >= 10)
        {
            levelDice = Dice.RollD4() + Dice.RollD4();
        }
        else if (level.Level >= 5)
        {
            levelDice = Dice.RollD6();
        }
        else
        {
            levelDice = Dice.RollD4();
        }

        int maximum = derived.GetAgilityModifier() + levelDice;

        actionPoint.Initialize(Mathf.Max(1, maximum));
    }

    private void HandleNodeClicked(FieldNode clickedNode)
    {
        if (clickedNode == null)
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

        return currentNode.IsConnectedTo(targetNode);
    }

    private void MoveCurrentPlayerTo(FieldNode targetNode)
    {
        if (targetNode == null || currentPlayer == null)
        {
            CompleteFieldAction();
            return;
        }

        if (currentNode != null)
        {
            currentNode.Exit(currentPlayer);
        }

        currentNode = targetNode;
        currentNode.Enter(currentPlayer);

        OnNodeChanged?.Invoke(currentNode);

        // 이벤트 후보 3~5개 공개
        if (TryOpenNodeEvent(currentNode))
        {
            return;
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

        if (!TryUseActionPoint(currentPlayer, 1))
        {
            Debug.Log("행동력이 부족합니다.");
            return;
        }

        if (TryOpenNodeEvent(currentNode))
        {
            return;
        }

        CompleteFieldAction();
    }

    private bool OpenFieldEvent(FieldEventData eventData, FieldNode node)
    {
        if (eventData == null || node == null || currentPlayer == null || eventRunner == null)
        {
            return false;
        }

        FieldEventContext context = new FieldEventContext(currentPlayer, node, this);

        bool opened = eventRunner.OpenEvent(eventData, context);

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

        if (pendingRedLine != null)
            return;

        // 방금 끝난 이벤트 결과로
        // 미션 목표를 달성했는지 확인
        if (TryCompleteCurrentMission())
            return;

        CompleteFieldAction();
    }

    private bool TryStartRedLineEvent(FieldLine line, FieldNode targetNode)
    {
        if (line == null || targetNode == null)
        {
            return false;
        }

        // 적색 라인 이벤트를 받을 UI가 없으면
        // 행동력을 소비하지 않음
        if (OnRedLineEventRequested == null)
        {
            Debug.LogWarning("FieldManager: 적색 라인 이벤트가 연결되지 않았습니다.");

            return false;
        }

        if (!TryUseActionPoint(currentPlayer, 1))
        {
            Debug.Log("행동력이 부족합니다.");
            return false;
        }

        pendingRedLine = line;
        pendingTargetNode = targetNode;

        TurnState = FieldTurnState.Event;

        OnRedLineEventRequested.Invoke(pendingRedLine, pendingTargetNode);

        return true;
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

        // 적색 라인 이벤트가 끝났으므로
        // 일반 행동 상태로 먼저 복귀
        TurnState = FieldTurnState.PlayerAction;

        if (passed)
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

    public void EndFieldTurn()
    {
        if (!IsFieldActive)
            return;

        if (TurnState == FieldTurnState.Event || TurnState == FieldTurnState.MythTurn || TurnState == FieldTurnState.GameOver)
        {
            return;
        }

        TurnState = FieldTurnState.TurnEnd;

        totalFieldTurn++;

        Debug.Log($"필드 턴 종료 / 누적 턴:{totalFieldTurn}");

        if (totalFieldTurn % 10 == 0)
        {
            StartMythTurn();
            return;
        }

        CheckPlayersAndStartNextTurn();
    }

    private void StartMythTurn()
    {
        TurnState = FieldTurnState.MythTurn;

        Debug.Log($"신화 턴 발생: {totalFieldTurn}");

        if (OnMythTurnRequested != null)
        {
            OnMythTurnRequested.Invoke(totalFieldTurn);

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

        TurnState = FieldTurnState.GameOver;

        IsFieldActive = false;

        Debug.Log("필드 게임 오버");

        OnFieldGameOver?.Invoke();
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
        missionProgress.Clear();

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

        currentMission = mission;
        missionProgress.Clear();

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
    public bool TryOpenNodeEvent(FieldNode node, bool isFirstVisit = false)
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

        FieldEventData nodeEvent;

        if (isFirstVisit)
        {
            nodeEvent = node.FirstVisitEvent;
        }
        else
        {
            nodeEvent = node.GetRandomRepeatEvent();
        }

        if (nodeEvent != null)
        {
            if (OpenFieldEvent(nodeEvent, node))
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

        FieldEventContext context = new FieldEventContext(this, node);

        bool opened = fieldEventSelectionController.OpenNextEventSelection(context);

        if (!opened)
            return false;

        TurnState = FieldTurnState.Event;

        Debug.Log(
            $"노드 이벤트 후보 공개: " +
            $"{node.DisplayName}");

        return true;
    }

    private void RegisterPlayerDefeatEvents(CharacterBase player)
    {
        if (player == null)
            return;

        HitpointModules hp = player.GetModule<HitpointModules>();

        if (hp == null)
            return;

        if (hp != null)
        {
            hp.OnEmpty -= HandlePlayerDefeated;
            hp.OnEmpty += HandlePlayerDefeated;
        }

        SanityModule sanity = player.GetModule<SanityModule>();

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
            return;
        }

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

        currentFieldObject = null;
        currentFieldRoot = null;
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

            if (!OpenFieldEvent(selectedEvent, node))
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

}