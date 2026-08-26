using UnityEngine;


/// <summary>
/// 카드 키워드의 공통 규칙과 표시 이름을 제공합니다.
/// </summary>
public class CardKeywordRules
{
    /// <summary>
    /// 해당 키워드가 카드 내구도를 사용하는지 확인합니다.
    /// 비점화는 아직 감소하지 않지만 점화될 내구도를 보유합니다.
    /// </summary>
    public static bool UsesDurability(CardKeywordType keyword)
    {
        switch (keyword)
        {
            case CardKeywordType.Light:
            case CardKeywordType.Unignited:
            case CardKeywordType.Ignition:
            case CardKeywordType.Blade:
            case CardKeywordType.Blunt:
            case CardKeywordType.Tool:
            case CardKeywordType.Medicine:
            case CardKeywordType.HolyRelic:
            case CardKeywordType.Binding:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 손패에서 턴이 지날 때 내구도가 감소하는 키워드인지 확인합니다.
    /// </summary>
    public static bool LosesDurabilityEachTurn(CardKeywordType keyword)
    {
        return keyword == CardKeywordType.Light || keyword == CardKeywordType.Ignition;
    }

    /// <summary>
    /// 키워드의 한글 표시 이름을 반환합니다.
    /// </summary>
    public static string GetDisplayName(CardKeywordType keyword)
    {
        switch (keyword)
        {
            case CardKeywordType.Light:
                return "광원";

            case CardKeywordType.Unignited:
                return "비점화";

            case CardKeywordType.Ignition:
                return "점화";

            case CardKeywordType.Blade:
                return "날붙이";

            case CardKeywordType.Blunt:
                return "둔기";

            case CardKeywordType.Tool:
                return "도구";

            case CardKeywordType.Medicine:
                return "약품";

            case CardKeywordType.HolyRelic:
                return "성물";

            case CardKeywordType.Binding:
                return "결박";

            case CardKeywordType.Key:
                return "열쇠";

            case CardKeywordType.Record:
                return "기록";

            default:
                return string.Empty;
        }
    }
}