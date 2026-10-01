using UnityEngine;

public enum FieldCardDataRequirement
{
    [InspectorName("덱에 카드가 있어야 함")]
    HasCard,

    [InspectorName("덱에 카드가 없어야 함")]
    DoesNotHaveCard
}

/// <summary>
/// 현재 이벤트 캐릭터의 덱 영역에 지정한 CardData가
/// 필요한 수량 이상 들어 있는지 확인합니다.
/// 핸드, 묘지, 제외, 소멸 영역은 검사하지 않습니다.
/// </summary>
[CreateAssetMenu(
    fileName = "NewFieldCardDataCondition",
    menuName = "Field/Event Condition/Deck Has Card Data")]
public class FieldCardDataCondition : FieldEventCondition
{
    [Header("필요한 카드")]
    [SerializeField]
    private CardData requiredCard;

    [Header("조건 방식")]
    [SerializeField]
    private FieldCardDataRequirement requirement =
        FieldCardDataRequirement.HasCard;

    [Header("필요한 수량")]
    [SerializeField, Min(1)]
    private int requiredAmount = 1;

    public override bool IsSatisfied(FieldEventContext context)
    {
        if (context == null ||
            context.Character == null ||
            requiredCard == null)
        {
            return false;
        }

        DeckModule deck =
            context.Character.GetModule<DeckModule>();

        if (deck == null || deck.DeckInstances == null)
            return false;

        int neededAmount = Mathf.Max(1, requiredAmount);
        int foundAmount = 0;

        foreach (CardInstance card in deck.DeckInstances)
        {
            if (card == null || card.Data != requiredCard)
                continue;

            foundAmount++;

            if (foundAmount >= neededAmount)
                break;
        }

        bool hasRequiredAmount = foundAmount >= neededAmount;

        return requirement == FieldCardDataRequirement.DoesNotHaveCard
            ? !hasRequiredAmount
            : hasRequiredAmount;
    }

    public override string GetFailMessage()
    {
        string cardName =
            requiredCard != null &&
            !string.IsNullOrWhiteSpace(requiredCard.cardName)
                ? requiredCard.cardName.Trim()
                : "필요한 카드";

        int neededAmount = Mathf.Max(1, requiredAmount);

        if (requirement == FieldCardDataRequirement.DoesNotHaveCard)
        {
            return neededAmount == 1
                ? $"{cardName} 카드가 덱에 있어 실행할 수 없습니다."
                : $"{cardName} 카드가 덱에 {neededAmount}장 이상 있어 실행할 수 없습니다.";
        }

        return neededAmount == 1
            ? $"{cardName} 카드가 덱에 없습니다."
            : $"{cardName} 카드가 덱에 {neededAmount}장 필요합니다.";
    }
}
