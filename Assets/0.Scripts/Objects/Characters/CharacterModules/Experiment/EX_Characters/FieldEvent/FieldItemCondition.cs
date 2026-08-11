using UnityEngine;


[CreateAssetMenu(fileName = "NewFieldItemCondition", menuName = "Field/Event Condition/Has Item")]
public class FieldItemCondition : FieldEventCondition
{
    [SerializeField]
    private ItemContainer requiredItem;

    [SerializeField, Min(1)]
    private int requiredAmount = 1;

    public override bool IsSatisfied(FieldEventContext context)
    {
        if (context == null || context.Inventory == null || requiredItem == null)
        {
            return false;
        }

        return context.Inventory.HasItem(requiredItem, requiredAmount);
    }

}
