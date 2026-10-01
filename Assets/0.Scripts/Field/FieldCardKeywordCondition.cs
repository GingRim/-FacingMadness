using UnityEngine;

/// <summary>
/// 현재 이벤트 캐릭터의 손패에 지정 키워드 카드가 있는지 확인합니다.
/// CardInstance를 검사하므로 런타임에 추가되거나 변경된 키워드도 반영합니다.
/// </summary>
[CreateAssetMenu(
    fileName = "NewFieldCardKeywordCondition",
    menuName = "Field/Event Condition/Hand Has Card Keyword")]
public class FieldCardKeywordCondition : FieldEventCondition
{
    [SerializeField]
    private CardKeywordType requiredKeyword;

    public override bool IsSatisfied(FieldEventContext context)
    {
        if (context == null ||
            context.Character == null ||
            requiredKeyword == CardKeywordType.None ||
            requiredKeyword == CardKeywordType._Length)
        {
            return false;
        }

        DeckModule deck =
            context.Character.GetModule<DeckModule>();

        if (deck == null || deck.HandInstances == null)
            return false;

        foreach (CardInstance card in deck.HandInstances)
        {
            if (card != null && card.HasKeyword(requiredKeyword))
                return true;
        }

        return false;
    }

    public override string GetFailMessage()
    {
        string keywordName =
            CardKeywordRules.GetDisplayName(requiredKeyword);

        return string.IsNullOrWhiteSpace(keywordName)
            ? "필요한 키워드 카드가 손패에 없습니다."
            : $"{keywordName} 키워드 카드가 손패에 없습니다.";
    }
}
