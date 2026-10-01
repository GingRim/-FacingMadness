using System;
using UnityEngine;


/// <summary>
/// 필드 미션을 완료하기 위해 달성해야 하는 개별 목표를 정의한다.
/// </summary>
[Serializable]
public class FieldMissionObjectiveRequirement
{
    [Header("목표 식별")]
    [SerializeField]
    private string objectiveId;

    [Header("필요 달성량")]
    [SerializeField, Min(1)]
    private int requiredAmount = 1;

    /// <summary>
    /// 이벤트 효과가 미션 진행도를 증가시킬 때 사용하는 목표 식별자다.
    /// </summary>
    public string ObjectiveId => objectiveId;

    /// <summary>
    /// 해당 목표를 완료하기 위해 필요한 진행량이다.
    /// </summary>
    public int RequiredAmount => requiredAmount;
}
