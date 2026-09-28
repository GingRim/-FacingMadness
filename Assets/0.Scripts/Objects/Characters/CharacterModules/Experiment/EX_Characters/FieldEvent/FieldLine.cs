using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FieldLine : MonoBehaviour
{
    [Serializable]
    public class RedLineAccessEventEntry
    {
        [SerializeField]
        private FieldEventData eventData;

        [SerializeField]
        private int priority;

        public FieldEventData EventData => eventData;
        public int Priority => priority;
    }

    [Header("연결 노드")]
    [SerializeField] private FieldNode nodeA;
    [SerializeField] private FieldNode nodeB;

    [Header("라인 상태")]
    [SerializeField]
    private FieldLineType lineType = FieldLineType.Normal;

    [Header("적색 라인 이벤트")]
    [Tooltip("이 Red 라인의 이동을 시도할 때 검사할 이벤트입니다. 조건 충족 항목 중 Priority가 가장 높은 이벤트를 사용합니다.")]
    [SerializeField]
    private RedLineAccessEventEntry[] redLineAccessEvents;

    [Header("적색 라인 체력")]
    [Tooltip("Red 상태일 때 사용하는 최대 체력입니다.")]
    [SerializeField, Min(1)]
    private int maximumHitpoint = 1;

    private int currentHitpoint;

    [Header("표시 오브젝트")]
    [SerializeField] private GameObject visualObject;

    [Header("라인 상태별 이미지")]
    [Tooltip("상태별 스프라이트를 표시할 UI Image입니다. 비워 두면 표시 오브젝트 또는 이 오브젝트에서 찾습니다.")]
    [SerializeField] private Image lineImage;

    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite redSprite;

    [Tooltip("Hidden 상태의 스프라이트입니다. Hidden 상태에서는 이미지를 숨깁니다.")]
    [SerializeField] private Sprite hiddenSprite;

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
    public int MaximumHitpoint => Mathf.Max(1, maximumHitpoint);
    public int CurrentHitpoint => currentHitpoint;
    public IReadOnlyList<RedLineAccessEventEntry> RedLineAccessEvents =>
        redLineAccessEvents;

    public event Action<FieldLine> OnLineStateChanged;
    public event Action<FieldLine, int, int> OnLineHitpointChanged;


    private void Awake()
    {
        initialLineType = lineType;

        ResetHitpointForCurrentType();

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

        FieldLineType previousType = lineType;

        lineType = newType;

        if (newType == FieldLineType.Red &&
            previousType != FieldLineType.Red)
        {
            currentHitpoint = MaximumHitpoint;
            NotifyHitpointChanged();
        }
        else if (newType != FieldLineType.Red)
        {
            currentHitpoint = 0;
            NotifyHitpointChanged();
        }

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
    /// Red 상태의 라인에 피해를 적용합니다.
    /// 체력이 0이 되면 즉시 Normal 라인으로 변경합니다.
    /// </summary>
    /// <returns>실제로 감소한 체력</returns>
    public int TakeDamage(int amount)
    {
        if (lineType != FieldLineType.Red || amount <= 0)
            return 0;

        int previousHitpoint = currentHitpoint;

        currentHitpoint = Mathf.Max(0, currentHitpoint - amount);

        int appliedDamage = previousHitpoint - currentHitpoint;

        NotifyHitpointChanged();

        if (currentHitpoint <= 0)
        {
            ChangeType(FieldLineType.Normal);
        }

        return appliedDamage;
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
    /// 정보로 경로를 공개하거나 새로운 경로 상태를 반영합니다.
    /// 현재 상태와 관계없이 Red 또는 Normal 상태를 적용할 수 있습니다.
    /// </summary>
    public bool RevealAs(FieldLineType revealedType)
    {
        if (revealedType == FieldLineType.Hidden)
        {
            Debug.LogWarning($"{name}: 공개 상태로 Hidden을 사용할 수 없습니다.");
            return false;
        }

        if (lineType == revealedType)
            return true;

        ChangeType(revealedType);
        return true;
    }

    public void Hide()
    {
        ChangeType(FieldLineType.Hidden);
    }

    private void RefreshVisual()
    {
        Image image = ResolveVisualImage();

        if (image != null)
        {
            Sprite stateSprite = GetStateSprite();

            if (stateSprite != null)
                image.sprite = stateSprite;
        }

        bool visible = lineType != FieldLineType.Hidden;

        // FieldLine 루트 자체를 비활성화하면 이후 상태 변경을 받을 수 없으므로
        // 별도의 표시 오브젝트일 때만 GameObject 활성 상태를 변경합니다.
        if (visualObject != null && visualObject != gameObject)
        {
            visualObject.SetActive(visible);
        }

        if (image != null)
        {
            image.enabled = visible;
        }
    }

    private Image ResolveVisualImage()
    {
        if (lineImage != null)
            return lineImage;

        if (visualObject != null)
        {
            lineImage = visualObject.GetComponent<Image>();

            if (lineImage == null)
                lineImage = visualObject.GetComponentInChildren<Image>(true);
        }

        if (lineImage == null)
        {
            lineImage = GetComponent<Image>();

            if (lineImage == null)
                lineImage = GetComponentInChildren<Image>(true);
        }

        return lineImage;
    }

    private Sprite GetStateSprite()
    {
        switch (lineType)
        {
            case FieldLineType.Normal:
                return normalSprite;

            case FieldLineType.Red:
                return redSprite;

            case FieldLineType.Hidden:
                return hiddenSprite;

            default:
                return null;
        }
    }

    public void ResetRuntimeState()
    {
        lineType = initialLineType;

        ResetHitpointForCurrentType();

        RefreshVisual();
        OnLineStateChanged?.Invoke(this);
    }

    private void ResetHitpointForCurrentType()
    {
        currentHitpoint =
            lineType == FieldLineType.Red
                ? MaximumHitpoint
                : 0;

        NotifyHitpointChanged();
    }

    private void NotifyHitpointChanged()
    {
        OnLineHitpointChanged?.Invoke(
            this,
            currentHitpoint,
            MaximumHitpoint);
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        maximumHitpoint = Mathf.Max(1, maximumHitpoint);

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
