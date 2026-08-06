using UnityEngine;

/// <summary>
/// 이벤트 실행 정보
/// </summary>
public class FieldEventContext
{
    public CharacterBase Player { get; }
    public FieldNode Node { get; }
    public FieldManager FieldManager { get; }

    public FieldEventContext(CharacterBase player, FieldNode node, FieldManager fieldManager)
    {
        Player = player;
        Node = node;
        FieldManager = fieldManager;
    }
}
