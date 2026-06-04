using UnityEngine;


/// <summary>
/// 카드 효과 처리기
/// 카드의 색상과 사용 코스트에 따라 실제 효과를 실행한다.
/// </summary>
public class CardResolver
{
    /// <summary>
    /// 카드 사용
    /// </summary>
    /// <param name="card">사용할 카드</param>
    /// <param name="user">카드를 사용하는 캐릭터</param>
    /// <param name="target">카드 효과를 받을 대상</param>
    /// <param name="useCost">선택한 사용 코스트</param>
    /// <returns>사용 성공 여부</returns>
    public bool Use(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        // 카드 또는 사용자가 없으면 실패
        if (card == null || user == null)
            return false;

        // 코스트 지불 시도
        if (!TryPayCost(user, useCost))
            return false;

        // 카드 색상에 따라 효과 실행
        switch (card.color)
        {
            // 적색 카드
            case CardColorType.Red:
                ResolveRed(card, user, target, useCost);
                break;

            // 황색 카드
            case CardColorType.Yellow:
                //ResolveYellow(card, user, target, useMode);
                break;

            // 녹색 카드
            case CardColorType.Green:
                //ResolveGreen(card, user, target, useMode);
                break;

            // 청색 카드
            case CardColorType.Blue:
                //ResolveBlue(card, user, target, useMode);
                break;

            // 자색 카드
            case CardColorType.Purple:
                //ResolvePurple(card, user, target, useMode);
                break;

            // 무색 카드
            case CardColorType.Colorless:
                // ResolveColorless(card, user, target, useMode);
                break;

            // 검은색 카드
            case CardColorType.Black:
                //ResolveBlack(card, user, target, useMode);
                break;
        }

        return true;
    }

    /// <summary>
    /// 카드 사용 전 코스트 지불
    /// </summary>
    /// <param name="user">카드 사용자</param>
    /// <param name="useCost">선택한 사용 방식</param>
    /// <returns>지불 성공 여부</returns>
    private bool TryPayCost(CharacterBase user, CardUseCost useCost)
    {
        // 사용자의 코스트 모듈 가져오기
        CostModule cost = user.GetModule<CostModule>();

        // 코스트 모듈이 없으면 실패
        if (cost == null)
            return false;

        switch (useCost)
        {
            // 행동 코스트 사용
            case CardUseCost.Action:
                return cost.Use(CostType.Action, 1);

            // 보조 행동 코스트 사용
            case CardUseCost.Auxiliary:
                return cost.Use(CostType.Auxiliary, 1);

            // 행동 + 보조 행동 동시 사용
            case CardUseCost.ActionAndAuxiliary:

                // 행동 코스트 부족
                if (!cost.CanUse(CostType.Action, 1))
                    return false;

                // 보조 행동 코스트 부족
                if (!cost.CanUse(CostType.Auxiliary, 1))
                    return false;

                // 실제 차감
                cost.Use(CostType.Action, 1);
                cost.Use(CostType.Auxiliary, 1);

                return true;
        }

        // 정의되지 않은 사용 방식
        return false;
    }

    /// <summary>
    /// 적색 카드 효과.
    /// 행동: 1D10 피해
    /// 보조 행동: 1D8 피해
    /// </summary>
    private void ResolveRed(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (target == null)
            return;

        int damage = 0;

        switch (useCost)
        {
            case CardUseCost.Action:
                damage = Dice.RollD10();
                break;

            case CardUseCost.Auxiliary:
                damage = Dice.RollD8();
                break;
        }

        DamageStruct damageInfo = new DamageStruct
        {
            from = user.gameObject,
            instigator = user.Controller,
            damageAmount = damage,
            critical = false,
            damageType = DamageType.Physical
        };

        CombatModule combat = target.GetModule<CombatModule>();

        if (combat == null)
            return;

        combat.OnHit(damageInfo);
    } 
}

