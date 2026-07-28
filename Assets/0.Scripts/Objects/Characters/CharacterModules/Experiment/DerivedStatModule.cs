using UnityEngine;

public class DerivedStatModule : CharacterModule
{
    private StatModules stat;
    private LVModules LV;
    DerivedStatModule derivedStat;

    public DerivedStatModule DerivedStat => derivedStat;

    public sealed override System.Type RegistrationType => typeof(DerivedStatModule);

    public override void OnRegistration(CharacterBase owner)
    {
        base.OnRegistration(owner);
        stat = owner.GetModule<StatModules>();
        LV = owner.GetModule<LVModules>();
    }

    /// <summary>
    /// 최대 체력 = 기본값 + (건강 * 5 + 레벨 * 15)
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 최대 정신력 = 기본값 + (의지 * 5 + 레벨 * 15)
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 가드 피해 감소 = 1D10 + 근력 보정 + 근력 3당 추가 보정
    /// </summary>
    /// <returns></returns>
    public int GetGuardBonus()
    {
        return stat.GetModifier(StatType.Strength)
             + stat.GetStat(StatType.Strength) / 3;
    }

    /// <summary>
    /// 회피 판정 보정
    /// </summary>
    /// <returns></returns>
    public int GetEvadeBonus()
    {
        return stat.GetStat(StatType.Agility);
    }

    /// <summary>
    /// 반격 보정 = 건강 보정
    /// </summary>
    /// <returns></returns>
    public int GetCounterBonus()
    {
        return stat.GetModifier(StatType.Health);
    }

    /// <summary>
    /// 우선권
    /// </summary>
    /// <param name="level"></param>
    /// <param name="handSize"></param>
    /// <returns></returns>
    public int GetInitiative(int level, int handSize)
    {
        int agility = stat.GetStat(StatType.Agility);

        int initiative = ((level * 5) + (agility * 5 + 5)) - handSize;

        StatusEffectModule status = Owner.GetModule<StatusEffectModule>();

        if (status != null)
        {
            initiative += status.GetInitiativeBonus();
        }


        return initiative;
    }

    /// <summary>
    /// 이동 거리 증가 = 민첩 3당 +1
    /// </summary>
    /// <returns></returns>
    public int GetMoveBonus()
    {
        return stat.GetStat(StatType.Agility) / 3;
    }

    /// <summary>
    /// 대응 코스트 증가 = 민첩 3당 +1
    /// </summary>
    /// <returns></returns>
    public int GetReactionCostBonus()
    {
        return stat.GetStat(StatType.Agility) / 3;
    }

    /// <summary>
    /// 행동 코스트 증가 = 건강 3당 +1
    /// </summary>
    /// <returns></returns>
    public int GetActionCostBonus()
    {
        return stat.GetStat(StatType.Health) / 3;
    }
    
    
    /// <summary>
    /// LV별 최대 코스트 증가
    /// </summary>
    /// <returns></returns>
    public int GetDefaultCost()
    {
        if (LV.Level >= 10)
            return 2;
        else if (LV.Level >= 5)
            return  1;

        return 0;
    }

    /// <summary>
    /// 최대 행동 코스트
    /// </summary>
    public int GetMaxActionCost()
    {
        return 1 + GetDefaultCost() + GetActionCostBonus();
    }

    /// <summary>
    /// 최대 보조행동 코스트
    /// </summary>
    /// <returns></returns>
    public int GetMaxAuxiliaryCost()
    {
        return 1 + GetDefaultCost() + GetAuxiliaryCostBonus();
    }

    /// <summary>
    /// 최대 대응 코스트
    /// </summary>
    /// <returns></returns>
    public int GetMaxReactionCost()
    {
        return 1 + GetDefaultCost() + GetReactionCostBonus();
    }

    /// <summary>
    /// 보조 행동 코스트 증가 = 지능 3당 +1
    /// </summary>
    /// <returns></returns>
    public int GetAuxiliaryCostBonus()
    {
        return stat.GetStat(StatType.Intelligence) / 3;
    }

    /// <summary>
    /// 최대 핸드 = 지능
    /// </summary>
    /// <returns></returns>
    public int GetMaxHand()
    {
        return stat.GetStat(StatType.Intelligence);
    }

    /// <summary>
    /// 드로우 증가 = 지능 3당 +1
    /// </summary>
    /// <returns></returns>
    public int GetDrawBonus()
    {
        int intelligence = stat.GetStat(StatType.Intelligence);

        int bonus = intelligence / 3;

        if (intelligence >= 10)
        {
            bonus += 1;
        }

        return bonus;
    }

    /// <summary>
    /// 광기 지속 턴 = 1 + (10 - 의지)
    /// </summary>
    /// <returns></returns>
    public int GetMadnessDuration()
    {
        return 1 + (10 - stat.GetStat(StatType.Will));
    }

    /// <summary>
    ///최대 아이템 보유 수 = 근력 
    /// </summary>
    /// <returns></returns>
    public int GetMaxItem()
    {
        return stat.GetStat(StatType.Strength);
    }

    /// <summary>
    /// 최대 유물 수 = 의지
    /// </summary>
    /// <returns></returns>
    public int GetMaxRelicCount()
    {
        return stat.GetStat(StatType.Will);
    }

    /// <summary>
    /// 카드 색상별 최대 수
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 힘 보정치 불러오기
    /// </summary>
    /// <returns></returns>
    public int GetStrengthModifier()
    {
        return stat.GetModifier(StatType.Strength);
    }

    /// <summary>
    /// 민첩 보정치 불러오기
    /// </summary>
    /// <returns></returns>
    public int GetAgilityModifier()
    {
        return stat.GetModifier(StatType.Agility);
    }

    /// <summary>
    /// 건강 보정치 불러오기
    /// </summary>
    /// <returns></returns>
    public int GetHealthModifier()
    {
        return stat.GetModifier(StatType.Health);
    }

    /// <summary>
    /// 지능 보정치 불러오기
    /// </summary>
    /// <returns></returns>
    public int GetIntelligenceModifier()
    {
        return stat.GetModifier(StatType.Intelligence);
    }

    /// <summary>
    /// 의지 보정치 불러오기
    /// </summary>
    /// <returns></returns>
    public int GetWillModifier()
    {
        return stat.GetModifier(StatType.Will);
    }
}
