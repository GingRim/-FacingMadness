using UnityEngine;

[CreateAssetMenu(fileName = "NewFieldInformationCondition", menuName = "Field/Event Condition/Has Information")]
public class FieldInformationCondition : FieldEventCondition
{
    [SerializeField]
    private FieldInformationData requiredInformation;

    [SerializeField]
    private bool mustHave = true;

    public override bool IsSatisfied(FieldEventContext context)
    {
        if (context == null || context.Character == null || requiredInformation == null)
            return false;

        FieldInformationInventory inventory =
            FieldInformationInventory.Get(context.Character);

        bool hasInformation =
            inventory != null && inventory.Contains(requiredInformation);

        return mustHave ? hasInformation : !hasInformation;
    }

    public override string GetFailMessage()
    {
        return mustHave
            ? "필요한 정보가 없습니다."
            : "이미 알고 있는 정보입니다.";
    }
}
