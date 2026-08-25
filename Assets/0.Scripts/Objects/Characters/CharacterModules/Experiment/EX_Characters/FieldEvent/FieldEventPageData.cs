using UnityEngine;

/// <summary>
/// 하나의 이벤트 안에서 표시되는 개별 선택지 페이지다.
/// 메인 페이지나 능력치별 하위 페이지로 사용할 수 있다.
/// </summary>
[CreateAssetMenu(fileName = "NewFieldEventPage", menuName = "Field/Event Page")]
public class FieldEventPageData : ScriptableObject
{
    [Header("페이지 식별")]
    [SerializeField]
    private string pageId;

    [Header("페이지 설명")]
    [SerializeField, TextArea(3, 10)]
    private string description;

    [Header("선택지 표시 방식")]
    [SerializeField]
    private FieldEventPageDisplayType displayType = FieldEventPageDisplayType.Fixed;

    [Header("최대 표시 개수")]
    [SerializeField, Range(1, 5)]
    private int maximumVisibleChoices = 5;

    [Header("선택지")]
    [SerializeField]
    private FieldEventChoice[] choices;

    /// <summary>
    /// 페이지를 구분하는 고유 식별자다.
    /// </summary>
    public string PageId => pageId;

    /// <summary>
    /// 페이지에 표시할 설명 문장이다.
    /// </summary>
    public string Description => description;

    /// <summary>
    /// 선택지 표시 방식이다.
    /// </summary>
    public FieldEventPageDisplayType DisplayType => displayType;

    /// <summary>
    /// 화면에 동시에 표시할 수 있는 최대 선택지 개수다.
    /// </summary>
    public int MaximumVisibleChoices => maximumVisibleChoices;

    /// <summary>
    /// 현재 페이지에 등록된 전체 선택지 목록이다.
    /// </summary>
    public FieldEventChoice[] Choices => choices;
}