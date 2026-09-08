using System;
using UnityEngine;

public class FieldLine : MonoBehaviour
{
    [Header("연결 노드")]
    [SerializeField] private FieldNode nodeA;
    [SerializeField] private FieldNode nodeB;

    [Header("라인 상태")]
    [SerializeField]
    private FieldLineType lineType = FieldLineType.Normal;

    [Header("적색 라인 고정 이벤트")]
    [Tooltip("이 라인이 Red일 때 이동을 시도하면 바로 실행할 이벤트입니다.")]
    [SerializeField]
    private FieldEventData redLineEvent;

    [Header("표시 오브젝트")]
    [SerializeField] private GameObject visualObject;

    [Header("라인 식별")]
    [SerializeField]
    private string lineId;

    private FieldLineType initialLineType;

    private bool isRegistered;

    public FieldNode NodeA => nodeA;
    public FieldNode NodeB => nodeB;

    public FieldLineType LineType => lineType;
    public bool IsHidden => lineType == FieldLineType.Hidden;
    public bool IsBlocked => lineType == FieldLineType.Red;
    public bool CanPass => lineType == FieldLineType.Normal;
    public string LineId => lineId;
    public FieldEventData RedLineEvent => redLineEvent;

    public event Action<FieldLine> OnLineStateChanged;


    private void Awake()
    {
        initialLineType = lineType;

        RegisterToNodes();
        RefreshVisual();
    }

    private void OnDestroy()
    {
        UnregisterFromNodes();
    }

    private void RegisterToNodes()
    {
        if (isRegistered)
            return;

        if (nodeA != null)
        {
            nodeA.AddLine(this);
        }

        if (nodeB != null)
        {
            nodeB.AddLine(this);
        }

        isRegistered = true;
    }

    private void UnregisterFromNodes()
    {
        if (!isRegistered)
            return;

        if (nodeA != null)
        {
            nodeA.RemoveLine(this);
        }

        if (nodeB != null)
        {
            nodeB.RemoveLine(this);
        }

        isRegistered = false;
    }

    public bool Contains(FieldNode node)
    {
        if (node == null)
            return false;

        return node == nodeA || node == nodeB;
    }

    public bool Connects(FieldNode first, FieldNode second)
    {
        if (first == null || second == null)
            return false;

        return
            (nodeA == first && nodeB == second) || (nodeA == second && nodeB == first);
    }

    public FieldNode GetOtherNode(FieldNode node)
    {
        if (node == nodeA)
            return nodeB;

        if (node == nodeB)
            return nodeA;

        return null;
    }

    public bool TryGetOtherNode(FieldNode node, out FieldNode otherNode)
    {
        otherNode = GetOtherNode(node);

        return otherNode != null;
    }

    public void ChangeType(FieldLineType newType)
    {
        if (lineType == newType)
            return;

        lineType = newType;

        RefreshVisual();
        OnLineStateChanged?.Invoke(this);
    }

    /// <summary>
    /// 일반 경로를 적색 경로로 봉쇄
    /// </summary>
    public void Block()
    {
        ChangeType(FieldLineType.Red);
    }

    /// <summary>
    /// 적색 경로 이벤트를 해결해 일반 경로로 변경
    /// </summary>
    public void ClearBlock()
    {
        if (lineType != FieldLineType.Red)
            return;

        ChangeType(FieldLineType.Normal);
    }

    /// <summary>
    /// 청색 카드 등으로 비밀 경로 발견
    /// </summary>
    public void Discover()
    {
        if (lineType != FieldLineType.Hidden)
            return;

        ChangeType(FieldLineType.Normal);
    }

    /// <summary>
    /// 정보로 숨겨진 경로를 공개하면서 지정된 상태로 변경합니다.
    /// 이미 공개된 경로의 현재 상태는 덮어쓰지 않습니다.
    /// </summary>
    public bool RevealAs(FieldLineType revealedType)
    {
        if (lineType != FieldLineType.Hidden)
            return false;

        if (revealedType == FieldLineType.Hidden)
        {
            Debug.LogWarning($"{name}: 공개 상태로 Hidden을 사용할 수 없습니다.");
            return false;
        }

        ChangeType(revealedType);
        return true;
    }

    public void Hide()
    {
        ChangeType(FieldLineType.Hidden);
    }

    private void RefreshVisual()
    {
        if (visualObject == null)
            return;

        // 비밀 라인은 발견 전까지 보이지 않음
        visualObject.SetActive(lineType != FieldLineType.Hidden);
    }

    public void ResetRuntimeState()
    {
        lineType = initialLineType;

        RefreshVisual();
        OnLineStateChanged?.Invoke(this);
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (nodeA == nodeB)
        {
            nodeB = null;
        }

        if (string.IsNullOrWhiteSpace(lineId))
        {
            lineId = gameObject.name;
        }

        RefreshVisual();
    }
#endif
}
