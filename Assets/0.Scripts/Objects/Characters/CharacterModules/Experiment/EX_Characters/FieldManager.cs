using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : ManagerBase
{
    
    [Header("필드 구성")]
    [SerializeField] private FieldNode startingNode;
    [SerializeField] private List<FieldNode> nodes = new();

    private CharacterBase currentPlayer;
    private FieldNode currentNode;

    public CharacterBase CurrentPlayer => currentPlayer;
    public FieldNode CurrentNode => currentNode;

    public bool IsFieldActive { get; private set; }

    public event Action<FieldNode> OnNodeChanged;
    public event Action<FieldNode> OnNodeEventRequested;
    public event Action<CharacterBase> OnCurrentPlayerChanged;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        RegisterNodes();

        yield return null;
    }

    protected override void OnDisconnected()
    {
        UnregisterNodes();
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

    public void StartField(CharacterBase player)
    {
        if (player == null)
        {
            Debug.LogWarning(
                "FieldManager: 현재 플레이어가 없습니다.");

            return;
        }

        if (startingNode == null)
        {
            Debug.LogWarning(
                "FieldManager: 시작 노드가 없습니다.");

            return;
        }

        IsFieldActive = true;

        SetCurrentPlayer(player);
        SetCurrentNode(startingNode);
    }

    public void EndField()
    {
        if (currentNode != null &&
            currentPlayer != null)
        {
            currentNode.Exit(currentPlayer);
        }

        currentNode = null;
        currentPlayer = null;

        IsFieldActive = false;
    }

    public void SetCurrentPlayer(CharacterBase player)
    {
        if (currentPlayer == player)
            return;

        currentPlayer = player;

        OnCurrentPlayerChanged?.Invoke(currentPlayer);
    }

    private void HandleNodeClicked(FieldNode clickedNode)
    {
        if (!IsFieldActive)
            return;

        if (clickedNode == null || currentPlayer == null)
        {
            return;
        }

        // 현재 플레이어가 있는 노드를 다시 클릭
        if (clickedNode == currentNode)
        {
            RequestCurrentNodeEvent();
            return;
        }

        TryMoveToNode(clickedNode);
    }

    public bool TryMoveToNode(FieldNode targetNode)
    {
        if (!CanMoveToNode(targetNode))
            return false;

        // 행동력 시스템이 만들어지면
        // 여기서 행동력 1을 차감한다.
        if (!TryUseActionPoint())
            return false;

        SetCurrentNode(targetNode);

        return true;
    }

    private bool CanMoveToNode(FieldNode targetNode)
    {
        if (!IsFieldActive)
            return false;

        if (currentPlayer == null || currentNode == null || targetNode == null)
        {
            return false;
        }

        if (targetNode == currentNode)
            return false;

        FieldLine line = currentNode.GetLineTo(targetNode);

        if (!currentNode.IsConnectedTo(targetNode))
        {
            Debug.Log(
                $"{currentNode.DisplayName}에서 " +
                $"{targetNode.DisplayName}(으)로 연결된 길이 없습니다.");

            return false;
        }

        // 다음 단계에서 FieldLine의 통행 가능 여부도 검사
        return true;
    }

    private void SetCurrentNode(FieldNode newNode)
    {
        if (newNode == null ||
            currentPlayer == null)
        {
            return;
        }

        if (currentNode != null)
        {
            currentNode.Exit(currentPlayer);
        }

        currentNode = newNode;
        currentNode.Enter(currentPlayer);

        OnNodeChanged?.Invoke(currentNode);

        // 최초 진입 및 재진입 이벤트 요청
        OnNodeEventRequested?.Invoke(currentNode);
    }

    private void RequestCurrentNodeEvent()
    {
        if (currentNode == null)
            return;

        // 현재 노드 추가 상호작용에도 행동력 소비
        if (!TryUseActionPoint())
            return;

        OnNodeEventRequested?.Invoke(currentNode);
    }

    private bool TryUseActionPoint()
    {
        // ActionPointModule을 연결하기 전의 임시 처리
        return true;
    }
}
