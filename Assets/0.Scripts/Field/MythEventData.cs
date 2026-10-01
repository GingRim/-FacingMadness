using UnityEngine;


[CreateAssetMenu(fileName = "NewMythEvent", menuName = "Field/Myth Event")]
public class MythEventData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField]
    private string eventName;

    [SerializeField, TextArea(3, 8)]
    private string description;

    [Header("이벤트 종류")]
    [SerializeField]
    private MythEventType eventType;

    public string EventName => eventName;
    public string Description => description;
    public MythEventType EventType => eventType;

}
