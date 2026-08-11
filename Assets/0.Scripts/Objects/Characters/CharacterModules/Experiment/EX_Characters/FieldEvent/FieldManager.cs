using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FieldManager : ManagerBase
{
    [Header("필드 구성")]
    [SerializeField] private FieldNode startingNode;
    [SerializeField] private List<FieldNode> nodes = new();

    [Header("이벤트 실행기")]
    [SerializeField] private FieldEventRunner eventRunner;

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

    public IReadOnlyList<FieldNode> StartingNodeCandidates => startingNodeCandidates;
    public GameObject CurrentFieldObject => currentFieldObject;
    public CharacterBase CurrentPlayer => currentPlayer;
    public FieldNode CurrentNode => currentNode;
    public MissionFieldRoot CurrentFieldRoot => currentFieldRoot;
    public bool HasStartingNode => startingNode != null;


    public bool IsSelectingStartingNode => isSelectingStartingNode;

    public event Action<FieldNode> OnStartingNodeConfirmed;

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
            startingNode.Enter(player);
        }

        if (participants.Count == 0)
            return;

        IsFieldActive = true;
        currentPlayerIndex = 0;
        totalFieldTurn = 0;

        StartFieldTurn();

        // 필드 시작 노드의 최초 이벤트
        if (startingNode.FirstVisitEvent != null)
        {
            OpenFieldEvent(startingNode.FirstVisitEvent, startingNode);
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

        participants.Clear();

        currentPlayer = null;
        currentNode = null;

        pendingRedLine = null;
        pendingTargetNode = null;

        eventRunner?.ResetCompletedEvents();

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

        bool firstVisit = !targetNode.IsVisited;

        if (currentNode != null)
        {
            currentNode.Exit(currentPlayer);
        }

        currentNode = targetNode;
        currentNode.Enter(currentPlayer);

        OnNodeChanged?.Invoke(currentNode);

        FieldEventData eventData = firstVisit ? currentNode.FirstVisitEvent : currentNode.GetRandomRepeatEvent();

        if (eventData != null && OpenFieldEvent(eventData, currentNode))
        {
            return;
        }

        CompleteFieldAction();
    }

    private void TryOpenAdditionalEvent()
    {
        if (currentNode == null || currentPlayer == null)
        {
            return;
        }

        FieldEventData eventData = currentNode.GetRandomRepeatEvent();

        if (eventData == null)
        {
            Debug.Log($"{currentNode.DisplayName}: " + "추가 이벤트가 없습니다.");

            return;
        }

        if (!TryUseActionPoint(currentPlayer, 1))
        {
            Debug.Log("행동력이 부족합니다.");
            return;
        }

        if (!OpenFieldEvent(eventData, currentNode))
        {
            CompleteFieldAction();
        }
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

        // 적색 라인 이벤트는
        // CompleteRedLineEvent에서 처리
        if (pendingRedLine != null)
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

        if (passed)
        {
            resolvedLine.ClearBlock();

            MoveCurrentPlayerTo(targetNode);
            return;
        }

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

            HitpointModules hp =
                player.GetModule<HitpointModules>();

            if (hp != null && hp.IsEmpty)
                return true;

            // 최대 정신력 0 조건은
            // 정신력 모듈 확인 후 이곳에 추가
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
        TurnState = FieldTurnState.GameOver;

        IsFieldActive = false;

        Debug.Log("필드 게임 오버");

        OnFieldGameOver?.Invoke();
    }

    public void EndField()
    {
        IsFieldActive = false;

        TurnState = FieldTurnState.Inactive;

        pendingRedLine = null;
        pendingTargetNode = null;

        eventRunner?.CloseEvent();

        currentPlayer = null;
        currentNode = null;

        participants.Clear();
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

}


