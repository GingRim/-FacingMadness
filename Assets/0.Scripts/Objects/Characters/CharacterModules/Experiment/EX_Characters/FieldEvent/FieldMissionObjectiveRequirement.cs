using UnityEngine;

public class FieldMissionObjectiveRequirement
{
    [SerializeField]
    private string objectiveId;

    [SerializeField, Min(1)]
    private int requiredAmount = 1;

    public string ObjectiveId => objectiveId;
    public int RequiredAmount => requiredAmount;
}
