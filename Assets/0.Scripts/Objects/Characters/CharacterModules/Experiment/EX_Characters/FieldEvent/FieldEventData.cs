using UnityEngine;

[CreateAssetMenu(fileName = "FieldEvent", menuName = "Facing Madness/Field/Event")]


///이벤트 데이터
public class FieldEventData : ScriptableObject
{
    [Header("이벤트 정보")]
    [SerializeField] private string eventId;
    [SerializeField] private string eventName;

    [SerializeField]
    private FieldEventType eventType;

    [TextArea(4, 12)]
    [SerializeField] private string description;

    [Header("설정")]
    [SerializeField] private bool repeatable;

    [Header("선택지")]
    [SerializeField]
    private FieldEventChoice[] choices;

    [SerializeField]
    private string startingNodeId;

    public string StartingNodeId => startingNodeId;

    public string EventId => eventId;
    public string EventName => eventName;
    public FieldEventType EventType => eventType;
    public string Description => description;
    public bool Repeatable => repeatable;

    public FieldEventChoice[] Choices => choices;
}
