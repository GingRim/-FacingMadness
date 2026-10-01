using System;
using UnityEngine;

/// <summary>
/// 필드 이벤트 선택지에 카드 요구 조건
/// </summary>

[Serializable]
public class FieldCardRequirement
{
    [SerializeField]
    private bool requiresCard;

    [Tooltip("사용 가능한 카드 색상을 등록합니다. 무색 대체를 허용하려면 무색도 목록에 추가합니다.")]
    [SerializeField]
    private CardColorType[] allowedColors;

    [Header("필드 판정")]
    [SerializeField]
    private StatType checkStat;

    public StatType CheckStat => checkStat;

    public bool RequiresCard => requiresCard;

    public bool IsSatisfiedBy(CardData card)
    {
        if (!requiresCard)
            return true;

        if (card == null)
            return false;

        if (allowedColors == null || allowedColors.Length == 0)
        {
            return false;
        }

        foreach (CardColorType allowedColor in allowedColors)
        {
            if (card.color == allowedColor)
                return true;
        }

        return false;
    }
}
