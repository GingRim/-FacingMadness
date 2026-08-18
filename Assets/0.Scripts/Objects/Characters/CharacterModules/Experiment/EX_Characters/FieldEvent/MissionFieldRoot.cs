using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 자식 노드와 라인을 감지해서 FieldManager에 전달
/// </summary>
public class MissionFieldRoot : MonoBehaviour
{
    [Header("필드 이벤트 풀")]
    [SerializeField]
    private List<FieldEventData> eventPool = new();

    public IReadOnlyList<FieldEventData> EventPool => eventPool;

    [Header("고정 시작 노드")]
    [SerializeField]
    private FieldNode fixedStartingNode;

    private readonly List<FieldNode> nodes = new();
    private readonly List<FieldLine> lines = new();
    private readonly List<FieldNode> startingNodeCandidates = new();

    public FieldNode FixedStartingNode => fixedStartingNode;

    public IReadOnlyList<FieldNode> Nodes => nodes;

    public IReadOnlyList<FieldLine> Lines => lines;

    public IReadOnlyList<FieldNode> StartingNodeCandidates => startingNodeCandidates;

    private void Awake()
    {
        DetectFieldObjects();
    }

    public void DetectFieldObjects()
    {
        nodes.Clear();
        lines.Clear();
        startingNodeCandidates.Clear();

        FieldNode[] foundNodes = GetComponentsInChildren<FieldNode>(true);

        foreach (FieldNode node in foundNodes)
        {
            if (node == null)
                continue;

            nodes.Add(node);

            if (node.CanBeStartingNode)
            {
                startingNodeCandidates.Add(node);
            }
        }

        FieldLine[] foundLines = GetComponentsInChildren<FieldLine>(true);

        foreach (FieldLine line in foundLines)
        {
            if (line != null)
            {
                lines.Add(line);
            }
        }

        if (fixedStartingNode == null && startingNodeCandidates.Count == 1)
        {
            fixedStartingNode = startingNodeCandidates[0];
        }
    }

    public FieldLine FindLine(string lineId)
    {
        if (string.IsNullOrWhiteSpace(lineId))
            return null;

        foreach (FieldLine line in lines)
        {
            if (line == null)
                continue;

            if (line.LineId == lineId)
                return line;
        }

        return null;
    }
}
