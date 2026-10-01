using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFieldInformationRemoveEffect",
    menuName = "Field/Event Effect/Information Remove")]
public class FieldInformationRemoveEffect : FieldEventEffect
{
    [Tooltip("제거할 정보입니다. 비어 있는 항목은 건너뜁니다.")]
    [SerializeField]
    private FieldInformationData[] informationToRemove;

    public override void Execute(FieldEventContext context)
    {
        if (context == null ||
            context.Character == null ||
            informationToRemove == null ||
            informationToRemove.Length == 0)
        {
            return;
        }

        FieldInformationInventory inventory =
            FieldInformationInventory.Get(context.Character);

        if (inventory == null)
            return;

        foreach (FieldInformationData information in informationToRemove)
        {
            if (information == null)
                continue;

            inventory.Remove(information);
        }
    }
}
