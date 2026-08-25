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

    [Header("이벤트 시작 페이지")]
    [SerializeField]
    private FieldEventPageData rootPage;

    /// <summary>
    /// 이벤트가 시작될 때 처음 표시하는 메인 페이지다.
    /// </summary>
    public FieldEventPageData RootPage => rootPage;

    [TextArea(4, 12)]
    [SerializeField] private string description;

    [Header("설정")]
    [SerializeField] private bool repeatable;

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

}
