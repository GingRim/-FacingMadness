using UnityEngine;


/// <summary>
/// 카드 키워드의 공통 규칙과 표시 이름을 제공합니다.
/// </summary>
public class CardKeywordRules
{
    /// <summary>
    /// 해당 키워드가 카드 내구도를 사용하는지 확인합니다.
    /// 모든 키워드 카드는 무색 카드로 취급하며 기본 내구도 3을 사용합니다.
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
            case CardKeywordType.Key:
            case CardKeywordType.Record:
            case CardKeywordType.Core:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 손패에서 턴이 지날 때 내구도가 감소하는 키워드인지 확인합니다.
    /// 새 규칙에서는 내구도가 성공적인 전투 사용 때만 감소하므로 항상 false입니다.
    /// 기존 호출부와의 호환을 위해 메서드는 유지합니다.
    /// </summary>
    public static bool LosesDurabilityEachTurn(CardKeywordType keyword)
    {
        return false;
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

            case CardKeywordType.Core:
                return "핵심";

            default:
                return string.Empty;
        }
    }
}
