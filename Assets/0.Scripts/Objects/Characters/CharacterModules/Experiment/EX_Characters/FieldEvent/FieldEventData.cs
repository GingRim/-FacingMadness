using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FieldEvent", menuName = "Facing Madness/Field/Event")]


///이벤트 데이터
public class FieldEventData : ScriptableObject
{
    [Header("이벤트 정보")]
    [SerializeField] private string eventId;
    [SerializeField] private string eventName;

    [Header("이벤트 유형")]
    [SerializeField]
    private FieldEventType eventType;

    [Header("여러 페이지 이벤트 (선택)")]
    [Tooltip("페이지 이동이 필요한 복합 이벤트에서만 등록합니다.")]
    [SerializeField]
    private FieldEventPageData rootPage;

    /// <summary>
    /// 이벤트가 시작될 때 처음 표시하는 메인 페이지다.
    /// </summary>
    public FieldEventPageData RootPage => rootPage;

    [Header("단일 페이지 선택지")]
    [Tooltip("단순 이벤트는 별도 Page Data 없이 여기에 선택지를 직접 등록합니다.")]
    [SerializeField]
    private FieldEventChoice[] directChoices;

    [SerializeField]
    private FieldEventPageDisplayType directChoiceDisplayType =
        FieldEventPageDisplayType.Fixed;

    [SerializeField, Range(1, 5)]
    private int maximumVisibleDirectChoices = 5;

    [TextArea(4, 12)]
    [SerializeField] private string description;

    [Header("설정")]
    [SerializeField] private bool repeatable;

    [Header("이벤트 발동 조건")]
    [SerializeField]
    private FieldEventCondition[] openingConditions;

    [SerializeField]
    private string startingNodeId;

    [Header("이벤트 표시")]
    [SerializeField]
    private Sprite eventImage;

    /// <summary>
    /// 이벤트 화면에 표시할 분위기 이미지다.
    /// </summary>
    public Sprite EventImage => eventImage;

    public string StartingNodeId => startingNodeId;

    public string EventId => eventId;
    public string EventName => eventName;
    public FieldEventType EventType => eventType;
    public string Description => description;
    public bool Repeatable => repeatable;
    public FieldEventChoice[] DirectChoices => directChoices;
    public FieldEventPageDisplayType DirectChoiceDisplayType => directChoiceDisplayType;
    public int MaximumVisibleDirectChoices => maximumVisibleDirectChoices;

    public bool HasDirectChoices => directChoices != null && directChoices.Length > 0;

    public bool HasPlayableContent => rootPage != null || HasDirectChoices;

    public bool CanOpen(FieldEventContext context)
    {
        if (context == null)
            return false;

        if (openingConditions == null)
            return true;

        foreach (FieldEventCondition condition in openingConditions)
        {
            if (condition != null && !condition.IsSatisfied(context))
                return false;
        }

        return true;
    }

}
