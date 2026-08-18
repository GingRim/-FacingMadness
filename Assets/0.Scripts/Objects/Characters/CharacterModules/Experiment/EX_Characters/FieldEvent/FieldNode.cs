using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldNode : MonoBehaviour
{

    [Header("시작 지점")]
    [SerializeField]
    private bool canBeStartingNode;

    public bool CanBeStartingNode => canBeStartingNode;

    [Header("노드 정보")]
    [SerializeField] private string nodeId;
    [SerializeField] private string displayName;

    [TextArea(3, 8)]
    [SerializeField] private string description;

    [Header("연결 정보")]
    [SerializeField] private List<FieldLine> connectedLines = new();

    [Header("최초 진입 이벤트")]
    [SerializeField]
    private FieldEventData firstVisitEvent;

    [Header("재진입 이벤트")]
    [SerializeField]
    private FieldEventData[] repeatEvents;

    [Header("캐릭터 표시 위치")]
    [SerializeField]
    private Transform markerRoot;

    [Header("비밀 구역")]
    [SerializeField]
    private bool isHiddenArea;

    [Header("오염")]
    [SerializeField]
    private GameObject pollutionVisual;

    private bool isPolluted;

    public bool IsPolluted => isPolluted;

    public event Action<FieldNode, bool> OnPollutionChanged;

    [SerializeField]
    private bool startsDiscovered;

    [SerializeField]
    private GameObject hiddenAreaVisual;

    private bool isHiddenAreaDiscovered;

    public Transform MarkerRoot => markerRoot != null ? markerRoot : transform;

    public FieldEventData FirstVisitEvent => firstVisitEvent;

    private readonly List<CharacterBase> characters = new();

    public event Action<FieldNode> OnHiddenAreaDiscovered;

    private bool isVisited;
    public bool IsHiddenArea => isHiddenArea;
    public bool IsHiddenAreaDiscovered => !isHiddenArea || isHiddenAreaDiscovered;

    public string NodeId => nodeId;
    public string DisplayName => displayName;
    public string Description => description;

    public bool IsVisited => isVisited;

    public IReadOnlyList<FieldLine> ConnectedLines => connectedLines;

    public IReadOnlyList<CharacterBase> Characters => characters;

    public event Action<FieldNode> OnClicked;
    public event Action<FieldNode, CharacterBase> OnCharacterEntered;
    public event Action<FieldNode, CharacterBase> OnCharacterExited;
    public event Action<FieldNode, CharacterBase> OnFirstEntered;

    private void Awake()
    {
        isHiddenAreaDiscovered = !isHiddenArea || startsDiscovered;

        RefreshHiddenAreaVisual();
    }

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

        // 노드 진입 즉시 오염 처리
        ResolvePollutionOnEnter(character);

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

        isHiddenAreaDiscovered = !isHiddenArea || startsDiscovered;

        isPolluted = false;

        RefreshHiddenAreaVisual();
        RefreshPollutionVisual();
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

    public FieldLine GetLineTo(FieldNode target)
    {
        if (target == null)
            return null;

        foreach (FieldLine line in connectedLines)
        {
            if (line == null)
                continue;

            if (line.Connects(this, target))
                return line;
        }

        return null;
    }

    public FieldEventData GetRandomRepeatEvent()
    {
        if (repeatEvents == null || repeatEvents.Length == 0)
        {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, repeatEvents.Length);

        return repeatEvents[randomIndex];
    }


    public bool DiscoverHiddenArea()
    {
        if (!isHiddenArea)
            return false;

        if (isHiddenAreaDiscovered)
            return false;

        isHiddenAreaDiscovered = true;

        RefreshHiddenAreaVisual();

        OnHiddenAreaDiscovered?.Invoke(this);

        Debug.Log($"비밀 구역 확인: {DisplayName}");

        return true;
    }

    private void RefreshHiddenAreaVisual()
    {
        if (hiddenAreaVisual == null)
            return;

        bool shouldShow = !isHiddenArea || isHiddenAreaDiscovered;

        hiddenAreaVisual.SetActive(shouldShow);
    }

    public bool ApplyPollution()
    {
        if (isPolluted)
            return false;

        isPolluted = true;

        RefreshPollutionVisual();

        OnPollutionChanged?.Invoke(this, true);

        Debug.Log($"노드 오염: {DisplayName}");

        return true;
    }

    private void ResolvePollutionOnEnter(CharacterBase character)
    {
        if (!isPolluted || character == null)
            return;

        SanityModule sanity = character.GetModule<SanityModule>();

        if (sanity == null)
        {
            Debug.LogWarning($"{character.name}: SanityModule이 없어 오염 피해를 적용하지 못했습니다.");
        }
        else
        {
            sanity.TakeSanityDamage(5);

            Debug.Log($"오염: {character.DisplayName} " + $"정신력 5 감소");
        }

        // 피해 적용 후 오염은 즉시 해제
        isPolluted = false;

        RefreshPollutionVisual();

        OnPollutionChanged?.Invoke(this, false);
    }

    private void RefreshPollutionVisual()
    {
        if (pollutionVisual == null)
            return;

        pollutionVisual.SetActive(isPolluted);
    }

}