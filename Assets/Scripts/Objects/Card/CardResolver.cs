using UnityEngine;

public class CardResolver
{
    //public bool Use(CardData card, CharacterBase user, CharacterBase target)
    //{
    //    Debug.Log("카드 사용 시도");
    //
    //    if (!CanUse(card, user))
    //    {
    //        Debug.Log("코스트 부족 또는 CostModule 없음");
    //        return false;
    //    }
    //
    //    Debug.Log("코스트 통과");
    //
    //    CostModule costModule = user.GetModule<CostModule>();
    //
    //    foreach (CardCostData cost in card.costs)
    //    {
    //        costModule.Use(cost.costType, cost.amount);
    //    }
    //
    //    if (HasTag(card, CardTagType.Attack))
    //    {
    //        Debug.Log("공격 카드 확인");
    //
    //        DamageStruct damageInfo = new DamageStruct
    //        {
    //            from = user.gameObject,
    //            instigator = user,
    //            damageAmount = card.damage,
    //            critical = false,
    //            damageType = DamageType.Physical
    //        };
    //
    //        CombatModule targetCombat = target.GetModule<CombatModule>();
    //
    //        if (targetCombat == null)
    //        {
    //            Debug.LogError("대상 CombatModule 없음");
    //            return false;
    //        }
    //
    //        Debug.Log($"대미지 적용 시도: {damageInfo.damageAmount}");
    //
    //        targetCombat.OnHit(damageInfo);
    //    }
    //
    //    return true;
    //}
    
    
    public bool CanUse(CardData card, ControllerBase user)
    {
        CostModule costModule = user.GetModule<CostModule>();

        if (costModule == null)
            return false;

        foreach (CardCostData cost in card.costs)
        {
            if (!costModule.CanUse(cost.costType, cost.amount))
                return false;
        }

        return true;
    }

    public bool Use(CardData card, ControllerBase user, ControllerBase target)
    {
        if (!CanUse(card, user))
            return false;

        CostModule costModule = user.GetModule<CostModule>();

        foreach (CardCostData cost in card.costs)
        {
            costModule.Use(cost.costType, cost.amount);
        }

        if (HasTag(card, CardTagType.Attack))
        {
            DamageStruct damageInfo = new DamageStruct
            {
                from = user.gameObject,
                instigator = user,
                damageAmount = card.damage,
                critical = false,
                damageType = DamageType.Physical
            };

            CombatModule targetCombat = target.GetModule<CombatModule>();
            targetCombat.OnHit(damageInfo);
        }

        if (HasTag(card, CardTagType.Restore))
        {
            RestoreStruct restoreInfo = new RestoreStruct
            {
                from = user.gameObject,
                instigator = user,
                restoreAmount = card.restoreAmount
            };

            HitpointModules targetHp = target.GetModule<HitpointModules>();
            targetHp.TakeRestore(restoreInfo);
        }

        return true;
    }

    private bool HasTag(CardData card, CardTagType tag)
    {
        foreach (CardTagType cardTag in card.tags)
        {
            if (cardTag == tag)
                return true;
        }

        return false;
    }
}
