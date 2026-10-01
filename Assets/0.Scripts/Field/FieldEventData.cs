using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 이벤트 자체가 한 필드에서 반복해서 등장할 수 있는지 구분합니다.
/// 기존 repeatable bool 값과 호환되도록 숫자 순서를 유지합니다.
/// </summary>


[CreateAssetMenu(fileName = "FieldEvent", menuName = "Facing Madness/Field/Event")]
public class FieldEventData : ScriptableObject
{
    [Header("이벤트 정보")]
    [FormerlySerializedAs("pageId")]
    [SerializeField]
    private string eventId;

    [SerializeField]
    private string eventName;

    [SerializeField]
    private FieldEventType eventType;

    [TextArea(4, 12)]
    [SerializeField]
    private string description;

    [Header("이벤트 표시")]
    [FormerlySerializedAs("resultImage")]
    [SerializeField]
    private Sprite eventImage;

    [Header("선택지 표시 방식")]
    [FormerlySerializedAs("directChoiceDisplayType")]
    [SerializeField]
    private FieldEventPageDisplayType displayType = FieldEventPageDisplayType.Fixed;

    [FormerlySerializedAs("maximumVisibleDirectChoices")]
    [SerializeField, Range(1, 5)]
    private int maximumVisibleChoices = 5;

    [Header("선택지")]
    [FormerlySerializedAs("directChoices")]
    [SerializeField]
    private FieldEventChoice[] choices;

    [Header("보유 메모 열람")]
    [Tooltip("활성화하면 고정 선택지 대신 현재 캐릭터가 보유한 메모를 자동으로 표시합니다.")]
    [SerializeField]
    private bool showOwnedMemos;

    [Header("이벤트 반복 설정")]
    [FormerlySerializedAs("repeatable")]
    [SerializeField]
    private FieldEventUsageType usageType = FieldEventUsageType.Repeatable;

    [Header("이벤트 발동 조건")]
    [SerializeField]
    private FieldEventCondition[] openingConditions;

    [Header("시작 노드 제한")]
    [SerializeField]
    private string startingNodeId;

    // 02 이전 FieldEventData의 Root Page 연결을 읽기 위한 호환 필드입니다.
    // 새 이벤트에서는 사용하지 않으며 모든 내용을 이 데이터에 직접 작성합니다.
    [FormerlySerializedAs("rootPage")]
    [SerializeField, HideInInspector]
    private FieldEventData legacyEntryData;

    public string EventId => eventId;
    public string EventName => eventName;
    public FieldEventType EventType => eventType;
    public string Description => description;
    public Sprite EventImage => eventImage;
    public FieldEventPageDisplayType DisplayType => displayType;
    public int MaximumVisibleChoices => maximumVisibleChoices;
    public FieldEventChoice[] Choices => choices;
    public bool ShowOwnedMemos => showOwnedMemos;
    public string StartingNodeId => startingNodeId;
    public FieldEventUsageType UsageType => usageType;
    public bool IsOneTime => usageType == FieldEventUsageType.OncePerField;

    /// <summary>
    /// 기존 Root Page 에셋이 연결된 이벤트는 해당 데이터를 시작 화면으로 사용합니다.
    /// 새로 만드는 이벤트는 자기 자신이 곧 시작 화면입니다.
    /// </summary>
    public FieldEventData EntryData => legacyEntryData != null && legacyEntryData != this ? legacyEntryData : this;

    public bool HasChoices => showOwnedMemos || (choices != null && choices.Length > 0);

    public bool HasPlayableContent => EntryData != null && EntryData.HasChoices;

    public bool CanOpen(FieldEventContext context)
    {
        if (context == null)
            return false;

        if (!AreConditionsSatisfied(openingConditions, context))
            return false;

        FieldEventData entry = EntryData;

        return entry == this || AreConditionsSatisfied(entry.openingConditions, context);
    }

    public string GetOpeningFailMessage(FieldEventContext context)
    {
        string message = GetConditionFailMessage(openingConditions, context);

        if (!string.IsNullOrWhiteSpace(message))
            return message;

        FieldEventData entry = EntryData;

        if (entry != this)
        {
            message = GetConditionFailMessage(entry.openingConditions, context);
        }

        return string.IsNullOrWhiteSpace(message) ? "이 이벤트의 조건을 만족하지 못했습니다." : message;
    }

    private static bool AreConditionsSatisfied(FieldEventCondition[] conditions, FieldEventContext context)
    {
        if (conditions == null)
            return true;

        foreach (FieldEventCondition condition in conditions)
        {
            if (condition != null &&
                !condition.IsSatisfied(context))
            {
                return false;
            }
        }

        return true;
    }

    private static string GetConditionFailMessage(FieldEventCondition[] conditions, FieldEventContext context)
    {
        if (conditions == null || context == null)
            return string.Empty;

        foreach (FieldEventCondition condition in conditions)
        {
            if (condition == null || condition.IsSatisfied(context))
                continue;

            return condition.GetFailMessage();
        }

        return string.Empty;
    }
}
