using System.Collections.Generic;
using UnityEngine;

public class MythEffectResolver : MonoBehaviour
{

    private readonly List<FieldNode> pollutableNodes = new();

    private readonly List<FieldLine> blockableLines = new();

    public void Execute(MythEventType eventType, MythTurnContext context)
    {
        if (context == null)
            return;

        switch (eventType)
        {
            case MythEventType.Hallucination:
                ResolveHallucination(context);
                break;

            case MythEventType.Obstacle:
                ResolveObstacle(context);
                break;

            case MythEventType.Pollution:
                ResolvePollution(context);
                break;

            case MythEventType.Oblivion:
                ResolveOblivion(context);
                break;

            case MythEventType.None:
            default:
                Debug.LogWarning($"처리할 수 없는 신화 이벤트: {eventType}");
                break;
        }
    }

    /// <summary>
    /// 환각:
    /// 모든 플레이어가 각각 1D10만큼 정신력 감소.
    /// </summary>
    private void ResolveHallucination(MythTurnContext context)
    {
        foreach (CharacterBase character
                 in context.Participants)
        {
            if (character == null)
                continue;

            SanityModule sanity = character.GetModule<SanityModule>();

            if (sanity == null)
            {
                Debug.LogWarning($"{character.name}: SanityModule이 없습니다.");

                continue;
            }

            int damageAmount = Dice.RollD10();

            sanity.TakeSanityDamage(damageAmount);

            Debug.Log($"환각: {character.DisplayName} / " + $"정신력 {damageAmount} 감소");
        }
    }

    /// <summary>
    /// 장해물:
    /// 현재 플레이어의 노드와 연결된
    /// 일반 라인 중 무작위 하나를 적색으로 변경.
    /// </summary>
    private void ResolveObstacle(MythTurnContext context)
    {
        FieldNode currentNode = context.CurrentNode;

        if (currentNode == null)
        {
            Debug.LogWarning("장해물 실패: 현재 노드가 없습니다.");

            return;
        }

        blockableLines.Clear();

        foreach (FieldLine line in currentNode.ConnectedLines)
        {
            if (line == null)
                continue;

            // 비밀 라인과 이미 막힌 적색 라인은 제외
            if (line.LineType != FieldLineType.Normal)
                continue;

            blockableLines.Add(line);
        }

        if (blockableLines.Count == 0)
        {
            Debug.Log("장해물: 적색으로 변경할 일반 라인이 없습니다.");

            return;
        }

        FieldLine selectedLine = blockableLines[Random.Range(0, blockableLines.Count)];

        selectedLine.Block();

        FieldNode otherNode = selectedLine.GetOtherNode(currentNode);

        string targetName = otherNode != null ? otherNode.DisplayName : "알 수 없는 구역";

        Debug.Log($"장해물: {currentNode.DisplayName} ↔ " + $"{targetName} 경로가 적색 라인으로 변경됨");
    }

    private void ResolvePollution(MythTurnContext context)
    {
        if (context == null || context.FieldManager == null)
        {
            return;
        }

        pollutableNodes.Clear();

        foreach (FieldNode node
                 in context.FieldManager.Nodes)
        {
            if (node == null)
                continue;

            // 이미 오염된 노드에는 중복 부여하지 않음
            if (node.IsPolluted)
                continue;

            pollutableNodes.Add(node);
        }

        if (pollutableNodes.Count == 0)
        {
            Debug.Log("오염: 오염시킬 수 있는 노드가 없습니다.");

            return;
        }

        FieldNode selectedNode = pollutableNodes[Random.Range(0, pollutableNodes.Count)];

        selectedNode.ApplyPollution();

        Debug.Log($"오염 발생 지역: {selectedNode.DisplayName}");
    }

    private void ResolveOblivion(MythTurnContext context)
    {
        // 망각 규칙을 연결할 예정
    }
}
