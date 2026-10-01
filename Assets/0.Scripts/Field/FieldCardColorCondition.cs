using UnityEngine;

/// <summary>
/// 현재 이벤트 캐릭터의 손패에 지정 색상 카드가 있는지 확인합니다.
/// </summary>
[CreateAssetMenu(
    fileName = "NewFieldCardColorCondition",
    menuName = "Field/Event Condition/Hand Has Card Color")]
public class FieldCardColorCondition : FieldEventCondition
{
    [SerializeField]
    private CardColorType requiredColor;

    public override bool IsSatisfied(FieldEventContext context)
    {
        if (context == null ||
            context.Character == null ||
            requiredColor == CardColorType.None ||
            requiredColor == CardColorType._Length)
        {
            return false;
        }

        DeckModule deck =
            context.Character.GetModule<DeckModule>();

        if (deck == null || deck.HandInstances == null)
            return false;

        foreach (CardInstance card in deck.HandInstances)
        {
            if (card != null && card.Color == requiredColor)
                return true;
        }

        return false;
    }

    public override string GetFailMessage()
    {
        return $"{GetColorName(requiredColor)} 카드가 손패에 없습니다.";
    }

    private static string GetColorName(CardColorType color)
    {
        switch (color)
        {
            case CardColorType.Red:
                return "적색";

            case CardColorType.Yellow:
                return "황색";

            case CardColorType.Green:
                return "녹색";

            case CardColorType.Blue:
                return "청색";

            case CardColorType.Purple:
                return "자색";

            case CardColorType.Colorless:
                return "무색";

            case CardColorType.Black:
                return "흑색";

            default:
                return "필요한 색상";
        }
    }
}
