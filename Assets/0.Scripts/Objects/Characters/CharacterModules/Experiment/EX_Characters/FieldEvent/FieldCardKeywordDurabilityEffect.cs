using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFieldCardKeywordDurabilityEffect",
    menuName = "Field/Event Effect/Card Keyword Durability")]
public class FieldCardKeywordDurabilityEffect : FieldEventEffect
{
    [SerializeField]
    private CardKeywordType requiredKeyword;

    [SerializeField, Min(1)]
    private int durabilityCost = 1;

    public override void Execute(FieldEventContext context)
    {
        if (context == null ||
            context.Character == null ||
            requiredKeyword == CardKeywordType.None ||
            requiredKeyword == CardKeywordType._Length)
        {
            return;
        }

        DeckModule deck = context.Character.GetModule<DeckModule>();

        if (deck == null || deck.HandInstances == null)
            return;

        CardInstance targetCard = FindFirstUsableCard(deck);

        if (targetCard == null)
        {
            Debug.LogWarning(
                $"{CardKeywordRules.GetDisplayName(requiredKeyword)} " +
                "키워드의 사용 가능한 카드가 손패에 없습니다.");
            return;
        }

        if (!targetCard.UseKeyword(
                requiredKeyword,
                Mathf.Max(1, durabilityCost)))
        {
            return;
        }

        if (targetCard.IsDepleted)
        {
            deck.MoveCard(
                targetCard,
                CardZoneType.Hand,
                CardZoneType.Remove);
        }
    }

    private CardInstance FindFirstUsableCard(DeckModule deck)
    {
        foreach (CardInstance card in deck.HandInstances)
        {
            if (card == null || card.Data == null || card.IsDepleted)
                continue;

            if (card.HasKeyword(requiredKeyword))
                return card;
        }

        return null;
    }
}
