using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "NewFieldInformationCondition", menuName = "Field/Event Condition/Information Knowledge")]
public class FieldInformationCondition : FieldEventCondition
{
    [SerializeField]
    private FieldInformationData requiredInformation;

    [FormerlySerializedAs("mustHave")]
    [SerializeField]
    private FieldInformationKnowledgeRequirement requirement =
        FieldInformationKnowledgeRequirement.Knows;

    public override bool IsSatisfied(FieldEventContext context)
    {
        if (context == null || context.Character == null || requiredInformation == null)
            return false;

        FieldInformationInventory inventory =
            FieldInformationInventory.Get(context.Character);

        bool hasInformation =
            inventory != null && inventory.Contains(requiredInformation);

        return requirement == FieldInformationKnowledgeRequirement.Knows
            ? hasInformation
            : !hasInformation;
    }

    public override string GetFailMessage()
    {
        return requirement == FieldInformationKnowledgeRequirement.Knows
            ? "필요한 정보가 없습니다."
            : "이미 알고 있는 정보입니다.";
    }
}
