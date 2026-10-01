using UnityEngine;


[CreateAssetMenu(fileName = "NewFieldItemEffect", menuName = "Field/Event Effect/Item")]
public class FieldItemEffect : FieldEventEffect
{ 
    [SerializeField]
    private FieldItemEffectType effectType;

    [SerializeField]
    private ItemContainer item;

    [SerializeField, Min(1)]
    private int amount = 1;

    public override void Execute(FieldEventContext context)
    {
        if (context == null || context.Inventory == null || item == null)
        {
            return;
        }

        switch (effectType)
        {
            case FieldItemEffectType.Add:
                AddItem(context.Inventory);
                break;

            case FieldItemEffectType.Remove:
                RemoveItem(context.Inventory);
                break;
        }
    }

    private void AddItem(Inventory inventory)
    {
        int remaining = inventory.AddItem(item, amount);

        int addedAmount = amount - remaining;

        Debug.Log($"필드 아이템 획득: {item.name} " + $"{addedAmount}개");

        if (remaining > 0)
        {
            Debug.LogWarning($"인벤토리 공간 부족: " + $"{item.name} {remaining}개를 넣지 못했습니다.");
        }
    }

    private void RemoveItem(Inventory inventory)
    {
        if (!inventory.HasItem(item, amount))
        {
            Debug.LogWarning($"필드 아이템 소모 실패: " + $"{item.name}이 부족합니다.");

            return;
        }

        int remaining = inventory.RemoveItem(item, amount);

        if (remaining > 0)
        {
            Debug.LogWarning($"아이템 제거 실패: " + $"{item.name} {remaining}개가 남았습니다.");

            return;
        }

        Debug.Log($"필드 아이템 소모: {item.name} " + $"{amount}개");
    }
}
