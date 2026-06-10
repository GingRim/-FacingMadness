using System;
using UnityEngine;
using static Dice;


/// <summary>
/// 카드 효과 처리기
/// 카드의 색상과 사용 코스트에 따라 실제 효과를 실행한다.
/// </summary>
public class CardResolver
{
    /// <summary>
    /// 기존 통합 사용 함수
    /// </summary>
    public bool Use(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        
        if (card == null || user == null)
            return false;

        // 코스트 사용 가능 여부 확인
        if (!CanUse(card, user, useCost))
            return false;

        // 실제 사용
        return UseWithoutCostCheck(card, user, target, useCost);
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
        DerivedStatModule derived = user.GetModule<DerivedStatModule>();
        LVModules lv = user.GetModule<LVModules>();

        if (derived == null || lv == null)
            return;

        int damage = 0;

        CriticalType criticalType = CriticalType.None;

        switch (useCost)
        {    
            case CardUseCost.Action:
            {
                DiceResult result = Dice.RollD10WithCritical(derived.GetStrengthModifier(), lv.Level);

                damage = result.total;
                criticalType = result.criticalType;

                if (criticalType == CriticalType.Critical)
                {
                    damage += Dice.RollD10();
                }
                else if (criticalType == CriticalType.GreatCritical)
                {
                    damage += Dice.RollD10();
                    damage += derived.GetStrengthModifier() * 2;
                }

                break;
            }

                case CardUseCost.Auxiliary:
                {
                    DiceResult result = Dice.RollD10WithCritical(derived.GetStrengthModifier(), lv.Level);
                    
                    damage = Dice.RollD8();
                    
                    criticalType = result.criticalType;

                    if (criticalType == CriticalType.Critical)
                    {
                        damage += derived.GetStrengthModifier();
                    }
                    else if (criticalType == CriticalType.GreatCritical)
                    {
                        damage += derived.GetStrengthModifier();
                        damage += Dice.RollD8();
                    }

                    break;
                } 
        }

        DamageStruct damageInfo = new DamageStruct
        {
            from = user.gameObject, instigator = user.Controller, damageAmount = damage,
            critical = criticalType != CriticalType.None,
            damageType = DamageType.Hand_to_hand_combat
        };

        CombatModule combat = target.GetModule<CombatModule>();

        if (combat == null)
            return;

        combat.OnHit(damageInfo);
        Debug.Log($"적색 카드 피해: {damage} / 크리티컬: {criticalType}");
    }

    /// <summary>
    /// 황색 카드 효과.
    /// 행동: 1D10 피해
    /// 보조 행동: 가속 1D4 스택 획득
    /// </summary>
    private void ResolveYellow(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        DerivedStatModule derived = user.GetModule<DerivedStatModule>();
        LVModules lv = user.GetModule<LVModules>();

        if (derived == null || lv == null)
            return;

        int damage = 0;
        CriticalType criticalType = CriticalType.None;

        switch (useCost)
        {
            case CardUseCost.Action:
            {
                DiceResult result = Dice.RollD10WithCritical(0, lv.Level);

                damage = result.diceValue;
                criticalType = result.criticalType;

                if (criticalType == CriticalType.Critical)
                {
                        damage += Dice.RollD10() + Dice.RollD6();
                }
                else if (criticalType == CriticalType.GreatCritical)
                {
                    damage += Dice.RollD10() + Dice.RollD10() + Dice.RollD6() + Dice.RollD6();
                }

                break;
            }


            case CardUseCost.Auxiliary:
            {
                int hasteStack = Dice.RollD4();

                Debug.Log($"가속 {hasteStack} 스택 획득");

                // 나중에 EffectModule 생기면 여기서 적용
                // EffectModule effect = user.GetModule<EffectModule>();
                // effect.AddStack(EffectType.Haste, hasteStack);

                break;
            }
        }
        
        DamageStruct damageInfo = new DamageStruct
        {
            from = user.gameObject,
            instigator = user.Controller,
            damageAmount = damage,
            critical = criticalType != CriticalType.None,
            damageType = DamageType.Hand_to_hand_combat
        };

        CombatModule combat = target.GetModule<CombatModule>();

        if (combat == null)
            return;

        combat.OnHit(damageInfo);

        Debug.Log($"황색 카드 피해: {damage} / 크리티컬: {criticalType}");


    }

    /// <summary>
    /// 녹색 카드 효과.
    /// 행동: HP 1D10 회복
    /// 보조 행동: 임시 장갑 1D4 획득
    /// </summary>
    private void ResolveGreen(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        DerivedStatModule derived = user.GetModule<DerivedStatModule>();
        LVModules lv = user.GetModule<LVModules>();
        
        if(derived == null || lv == null)
            return;

        switch (useCost)
        {
            case CardUseCost.Action:
            {
                CharacterBase restreTarget = target != null ? target : user;

                DiceResult result = Dice.RollD10WithCritical(derived.GetHealthModifier(), lv.Level);

                int restore = result.total;

                if (result.criticalType == CriticalType.Critical)
                {
                    restore += 5;
                }
                else if (result.criticalType == CriticalType.GreatCritical)
                {
                    restore += 15;
                }

                RestoreStruct restoreInfo = new RestoreStruct{
                    from = user.gameObject,
                    instigator = user.Controller,
                    restoreAmount = restore};

                CombatModule combat = restreTarget.GetModule<CombatModule>();

                if(combat == null)
                    return;

                combat.OnRestore(restoreInfo);

                Debug.Log($"녹색 회복: {restore} / 크리티컬:{result.criticalType}");

                break;
            }


                

            case CardUseCost.Auxiliary:
            {
                DiceResult result = Dice.RollD10WithCritical(derived.GetHealthModifier(), lv.Level);

                int armor = Dice.RollD4();

                if (result.criticalType == CriticalType.Critical)
                {
                    armor += Dice.RollD4();
                }
                else if (result.criticalType == CriticalType.GreatCritical)
                {
                    armor = Dice.RollD8() + Dice.RollD8();
                }

                Debug.Log($"임시 장갑 획득: {armor} / 크리티컬:{result.criticalType}");

                // 나중에 ArmorModule / EffectModule 생기면 여기서 적용
                // ArmorModule armorModule = user.GetModule<ArmorModule>();
                // armorModule.AddTemporaryArmor(armor);
                // 1 ~ 2~9 ~10

                break;
            }
        }
    }

    /// <summary>
    /// 청색 카드 효과.
    /// 행동: 원거리 공격 1D10
    /// 보조: 짝수면 2드로우, 홀수면 집중
    /// </summary>
    private void ResolveBlue(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        DerivedStatModule derived = user.GetModule<DerivedStatModule>();
        switch (useCost)
        {
            case CardUseCost.Action:

                if (target == null)
                    return;

                int damage = Dice.RollD10();

                DamageStruct damageInfo = new DamageStruct
                {
                    from = user.gameObject,
                    instigator = user.Controller,
                    damageAmount = damage,
                    critical = false,
                    damageType = DamageType.Long_range_combat
                };

                CombatModule combat =
                    target.GetModule<CombatModule>();

                if (combat == null)
                    return;

                combat.OnHit(damageInfo);

                break;

            case CardUseCost.Auxiliary:

                int result = Dice.RollD10();

                if (result % 2 == 0)
                {
                    Debug.Log("청색 효과 : 2 드로우");

                    DeckModule deck =
                        user.GetModule<DeckModule>();

                    if (deck != null)
                    {
                        deck.Draw();
                        deck.Draw();
                    }
                }
                else
                {
                    Debug.Log("청색 효과 : 집중 획득");

                    // 나중에 집중 시스템 추가 예정
                    // EffectModule.Add(Concentration)
                }

                break;
        }
    }

    /// <summary>
    /// 자색 카드 효과.
    /// 행동 + 보조 행동 코스트를 사용하는 마법 카드 계열.
    /// 현재는 1D6으로 마법군만 결정한다.
    /// </summary>
    private void ResolvePurple(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (useCost != CardUseCost.ActionAndAuxiliary)
        {
            Debug.Log("자색 카드는 행동 + 보조 행동 코스트가 필요합니다.");
            return;
        }

        int magicType = Dice.RollD6();

        switch (magicType)
        {
            case 1:
                Debug.Log("자색 카드: 룬 마법 생성");
                break;

            case 2:
                Debug.Log("자색 카드: 원소 마법 생성");
                break;

            case 3:
                Debug.Log("자색 카드: 소환술 생성");
                break;

            case 4:
                Debug.Log("자색 카드: 연금술 생성");
                break;

            case 5:
                Debug.Log("자색 카드: 주문 생성");
                break;

            case 6:
                Debug.Log("자색 카드: 금지된 주술 생성");
                break;
        }
    }
   
    /// <summary>
    /// 무색 카드 효과.
    /// 행동 전용 다이스 만큼 대미지
    /// 보조 행동 전용 다이스 만큼 회복(생명력, 정신력)
    /// </summary>
    /// <param name="card"></param>
    /// <param name="user"></param>
    /// <param name="target"></param>
    /// <param name="useCost"></param>
    private void ResolveColorless(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        int value = RollColorlessDice(user);
        DerivedStatModule derived = user.GetModule<DerivedStatModule>();
        switch (useCost)
        {
            case CardUseCost.Action:

                if (target == null)
                    return;

                DamageStruct damageInfo = new DamageStruct
                {
                    from = user.gameObject,
                    instigator = user.Controller,
                    damageAmount = value,
                    critical = false,
                    damageType = DamageType.Hand_to_hand_combat
                };

                target.GetModule<CombatModule>()?.OnHit(damageInfo);

                break;

            case CardUseCost.Auxiliary:

                RestoreStruct restoreInfo = new RestoreStruct
                {
                    from = user.gameObject,
                    instigator = user.Controller,
                    restoreAmount = value
                };

                user.GetModule<CombatModule>()?.OnRestore(restoreInfo);

                break;
        }
    }

    private int RollColorlessDice(CharacterBase user)
    {
        LVModules lv = user.GetModule<LVModules>();

        if (lv == null)
            return Dice.RollD4();

        if (lv.Level >= 10)
            return Dice.RollD8();

        if (lv.Level >= 5)
            return Dice.RollD6();

        return Dice.RollD4();
    }

    /// <summary>
    /// 코스트 사용 가능 여부만 확인
    /// </summary>
    public bool CanUse(CardData selectedCard, CharacterBase user, CardUseCost useCost)
    {
        CostModule cost = user.GetModule<CostModule>();

        if (cost == null)
            return false;

        switch (useCost)
        {
            case CardUseCost.Action:
                return cost.CanUse(CostType.Action, 1);

            case CardUseCost.Auxiliary:
                return cost.CanUse(CostType.Auxiliary, 1);

            case CardUseCost.ActionAndAuxiliary:

                return cost.CanUse(CostType.Action, 1)
                    && cost.CanUse(CostType.Auxiliary, 1);
        }

        return false;
    }

    /// <summary>
    /// 코스트 검사는 하지 않고
    /// 실제 차감 + 효과만 실행
    /// </summary>
    public bool UseWithoutCostCheck(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
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
                ResolveYellow(card, user, target, useCost);

                break;

            // 녹색 카드
            case CardColorType.Green:
                ResolveGreen(card, user, target, useCost);
                break;

            // 청색 카드
            case CardColorType.Blue:
                ResolveBlue(card, user, target, useCost);
                break;

            // 자색 카드
            case CardColorType.Purple:
                ResolvePurple(card, user, target, useCost);
                break;

            // 무색 카드
            case CardColorType.Colorless:
                ResolveColorless(card, user, target, useCost);
                break;

            // 검은색 카드
            case CardColorType.Black:
                //ResolveBlack(card, user, target, useCost);
                break;
        }

        return true;
    }

}

