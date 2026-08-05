using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldNode : MonoBehaviour
{

    [Header("노드 정보")]
    [SerializeField] private string nodeId;
    [SerializeField] private string displayName;

    [TextArea(3, 8)]
    [SerializeField] private string description;

    [Header("연결 정보")]
    [SerializeField] private List<FieldLine> connectedLines = new();

    [Header("노드 이벤트")]
    [SerializeField] private FieldEvent nodeEvent;

    private readonly List<CharacterBase> characters = new();

    private bool isVisited;

    public string NodeId => nodeId;
    public string DisplayName => displayName;
    public string Description => description;

    public bool IsVisited => isVisited;

    public FieldEvent NodeEvent => nodeEvent;

    public IReadOnlyList<FieldLine> ConnectedLines =>
        connectedLines;

    public IReadOnlyList<CharacterBase> Characters =>
        characters;

    public event Action<FieldNode> OnClicked;
    public event Action<FieldNode, CharacterBase> OnCharacterEntered;
    public event Action<FieldNode, CharacterBase> OnCharacterExited;
    public event Action<FieldNode, CharacterBase> OnFirstEntered;

    /// <summary>
    /// Button의 OnClick이나 별도의 노드 클릭 스크립트에서 호출
    /// </summary>
    public void ClickNode()
    {
        OnClicked?.Invoke(this);
    }

    public void Enter(CharacterBase character)
    {
        if (character == null)
            return;

        if (characters.Contains(character))
            return;

        bool firstVisit = !isVisited;

        characters.Add(character);
        isVisited = true;

        if (firstVisit)
        {
            OnFirstEntered?.Invoke(this, character);
        }

        OnCharacterEntered?.Invoke(this, character);
    }

    public void Exit(CharacterBase character)
    {
        if (character == null)
            return;

        if (!characters.Remove(character))
            return;

        OnCharacterExited?.Invoke(this, character);
    }

    public bool ContainsCharacter(CharacterBase character)
    {
        if (character == null)
            return false;

        return characters.Contains(character);
    }

    public bool HasAnyCharacter()
    {
        return characters.Count > 0;
    }

    public void AddLine(FieldLine line)
    {
        if (line == null)
            return;

        if (connectedLines.Contains(line))
            return;

        connectedLines.Add(line);
    }

    public void RemoveLine(FieldLine line)
    {
        if (line == null)
            return;

        connectedLines.Remove(line);
    }

    public bool IsConnectedTo(FieldNode target)
    {
        if (target == null)
            return false;

        foreach (FieldLine line in connectedLines)
        {
            if (line == null)
                continue;

            if (line.Connects(this, target))
                return true;
        }

        return false;
    }

    public void ResetNode()
    {
        characters.Clear();
        isVisited = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            nodeId = gameObject.name;
        }

        connectedLines.RemoveAll(line => line == null);
    }
#endif
}

