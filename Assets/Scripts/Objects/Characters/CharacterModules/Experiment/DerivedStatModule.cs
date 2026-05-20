using UnityEngine;

public class DerivedStatModule : CharacterModule
{
    private StatModules stat;
    private LVModules LV;

    public sealed override System.Type RegistrationType => typeof(DerivedStatModule);

    public override void OnRegistration(CharacterBase owner)
    {
        base.OnRegistration(owner);
        stat = owner.GetModule<StatModules>();
        LV = owner.GetModule<LVModules>();
    }

    // 최대 체력 = 기본값 + (건강 * 5 + 레벨 * 15)
    public int GetMaxHP()
    {
        if (stat == null)
        {
            Debug.LogError("DerivedStatModule: StatModules 없음");
            return 0;
        }

        int health = stat.GetStat(StatType.Health);

        int level = 1;

        if (LV != null)
            level = LV.Level;

        return 20 + (health * 5) + (level * 15);
    }

    // 최대 정신력 = 기본값 + (의지 * 5 + 레벨 * 15)
    public int GetMaxSanity()
    {
        if (stat == null)
        {
            Debug.LogError("DerivedStatModule : StatModules 없음");
                return 0;
        }
        
        int will = stat.GetStat(StatType.Will);

        int level = 1;

        if (LV != null) level = LV.Level;
        return 20 + (will * 5) + (level * 15);
    }

    // 가드 피해 감소 = 1D10 + 근력 보정 + 근력 3당 추가 보정
    public int GetGuardBonus()
    {
        return stat.GetModifier(StatType.Strength)
             + stat.GetStat(StatType.Strength) / 3;
    }

    // 회피 판정 보정
    public int GetEvadeBonus()
    {
        return stat.GetStat(StatType.Agility);
    }

    // 반격 보정 = 건강 보정
    public int GetCounterBonus()
    {
        return stat.GetModifier(StatType.Health);
    }

    // 우선권
    public int GetInitiative(int level, int handSize)
    {
        int agility = stat.GetStat(StatType.Agility);
        return ((level * 5) + (agility * 5 + 5)) - handSize;
    }

    // 이동 거리 증가 = 민첩 3당 +1
    public int GetMoveBonus()
    {
        return stat.GetStat(StatType.Agility) / 3;
    }

    // 대응 코스트 증가 = 민첩 3당 +1
    public int GetReactionCostBonus()
    {
        return stat.GetStat(StatType.Agility) / 3;
    }

    // 행동 코스트 증가 = 건강 3당 +1
    public int GetActionCostBonus()
    {
        return stat.GetStat(StatType.Health) / 3;
    }

    // 보조 행동 코스트 증가 = 지능 3당 +1
    public int GetAuxiliaryCostBonus()
    {
        return stat.GetStat(StatType.Intelligence) / 3;
    }

    // 최대 핸드 = 지능
    public int GetMaxHand()
    {
        return stat.GetStat(StatType.Intelligence);
    }

    // 드로우 증가 = 지능 3당 +1
    public int GetDrawBonus()
    {
        return stat.GetStat(StatType.Intelligence) / 3;
    }

    // 광기 지속 턴 = 1 + (10 - 의지)
    public int GetMadnessDuration()
    {
        return 1 + (10 - stat.GetStat(StatType.Will));
    }

    // 최대 유물 수 = 의지
    public int GetMaxRelicCount()
    {
        return stat.GetStat(StatType.Will);
    }

    // 카드 색상별 최대 수
    public int GetMaxCardCount(CardColorType color)
    {
        switch (color)
        {
            case CardColorType.Red:
                return stat.GetStat(StatType.Strength);

            case CardColorType.Yellow:
                return stat.GetStat(StatType.Agility);

            case CardColorType.Green:
                return stat.GetStat(StatType.Health);

            case CardColorType.Blue:
                return stat.GetStat(StatType.Intelligence);

            case CardColorType.Purple:
                return stat.GetStat(StatType.Will);

            default:
                return 0;
        }
    }
}
