using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NewFieldInformation", menuName = "Field/Information")]
public class FieldInformationData : ScriptableObject
{
    [SerializeField] private string informationId;
    [SerializeField] private string title;
    [SerializeField, TextArea(3, 12)] private string description;
    [SerializeField] private FieldInformationVisibility visibility = FieldInformationVisibility.Hidden;

    [Header("획득 시 공개할 필드 경로")]
    [Tooltip("FieldLine의 Line Id를 등록합니다.")]
    [SerializeField] private string[] revealLineIds;

    [Tooltip("정보 획득 직후 공개된 라인의 상태입니다. 문처럼 확인이 필요한 경로는 Red를 사용합니다.")]
    [SerializeField]
    private FieldInformationLineRevealState revealLineState = FieldInformationLineRevealState.Red;

    [Tooltip("FieldNode의 Node Id를 등록합니다.")]
    [SerializeField] private string[] revealNodeIds;

    public string InformationId => informationId;
    public string Title => title;
    public string Description => description;
    public FieldInformationVisibility Visibility => visibility;
    public bool IsMemo => visibility == FieldInformationVisibility.Memo;
    public IReadOnlyList<string> RevealLineIds => revealLineIds ?? System.Array.Empty<string>();

    public IReadOnlyList<string> RevealNodeIds => revealNodeIds ?? System.Array.Empty<string>();

    public FieldLineType RevealedLineType => revealLineState == FieldInformationLineRevealState.Normal ? FieldLineType.Normal : FieldLineType.Red;
}
