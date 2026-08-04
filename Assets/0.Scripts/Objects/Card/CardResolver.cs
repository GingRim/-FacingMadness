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
        if (criticalType == CriticalType.GreatCritical)
        {
            BattleManager.ClaimBattleLog($"상위 크리티컬<br>{damage}피해");
        }
        else if (criticalType == CriticalType.Critical)
        {
            BattleManager.ClaimBattleLog($"크리티컬<br>{damage}피해");
        }
        else
        {
            BattleManager.ClaimBattleLog($"{damage}피해");
        }
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
                    if (criticalType == CriticalType.GreatCritical)
                    {
                        BattleManager.ClaimBattleLog($"상위 크리티컬<br>{damage}피해");
                    }
                    else if (criticalType == CriticalType.Critical)
                    {
                        BattleManager.ClaimBattleLog($"크리티컬<br>{damage}피해");
                    }
                    else
                    {
                        BattleManager.ClaimBattleLog($"{damage}피해");
                    }
                    break;
            }


            case CardUseCost.Auxiliary:
            {
                    StatusEffectModule status = user.GetModule<StatusEffectModule>();
                    int q = RollD4();
                    if (status == null)
                    {
                        Debug.Log("상태 이상 모듈 없음");
                        return;
                    }

                    status.AddStatus(StatusEffectType.Haste, q);


                    BattleManager.ClaimBattleLog($"가속{q} 증가");
                    break;
            }
        }
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

                int restore = RollD10();

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
                  
                    if (result.criticalType == CriticalType.GreatCritical)
                    {
                        BattleManager.ClaimBattleLog($"상위 크리티컬<br>{restore}생명력 회복");
                    }
                    else if (result.criticalType == CriticalType.Critical)
                    {
                        BattleManager.ClaimBattleLog($"크리티컬<br>{restore}생명력 회복");
                    }
                    else
                    {
                        BattleManager.ClaimBattleLog($"{restore}생명력 회복");
                    }

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

                Debug.Log($"크리티컬:{result.criticalType}");

                 ArmorModule armorModule = user.GetModule<ArmorModule>();

                    if (armorModule == null)
                    {
                        Debug.Log("임시 장갑 실패: ArmorModule 없음");
                        return;
                    }

                    armorModule.AddTemporaryArmor(armor);
                    if (result.criticalType == CriticalType.GreatCritical)
                    {
                        BattleManager.ClaimBattleLog($"상위 크리티컬<br>임시 장갑{armor} 획득");
                    }
                    else if (result.criticalType == CriticalType.Critical)
                    {
                        BattleManager.ClaimBattleLog($"크리티컬<br>임시 장갑{armor} 획득");
                    }
                    else
                    {
                        BattleManager.ClaimBattleLog($"임시 장갑{armor} 획득");
                    }
                    

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
        DerivedStatModule derived =
            user.GetModule<DerivedStatModule>();

        LVModules lv =
            user.GetModule<LVModules>();

        if (derived == null || lv == null)
            return;

        switch (useCost)
        {
            case CardUseCost.Action:
                {
                    if (target == null)
                        return;

                    DiceResult result = Dice.RollD10WithCritical(derived.GetIntelligenceModifier(), lv.Level);

                    int damage = RollD10();

                    if (result.criticalType == CriticalType.Critical)
                    {
                        damage += derived.GetIntelligenceModifier();
                    }
                    else if (result.criticalType == CriticalType.GreatCritical)
                    {
                        damage += derived.GetIntelligenceModifier();
                        damage += Dice.RollD10();
                    }

                    DamageStruct damageInfo = new DamageStruct
                        {
                            from = user.gameObject,
                            instigator = user.Controller,
                            damageAmount = damage,
                            critical = result.criticalType != CriticalType.None,
                            damageType = DamageType.Long_range_combat
                        };

                    CombatModule combat = target.GetModule<CombatModule>();

                    if (combat == null)
                        return;

                    combat.OnHit(damageInfo);

                    if (result.criticalType == CriticalType.GreatCritical)
                    {
                        BattleManager.ClaimBattleLog($"상위 크리티컬<br>{damage}피해");
                    }
                    else if (result.criticalType == CriticalType.Critical)
                    {
                        BattleManager.ClaimBattleLog($"크리티컬<br>{damage}피해");
                    }
                    else
                    {
                        BattleManager.ClaimBattleLog($"{damage}피해");
                    }

                    break;
                }

            case CardUseCost.Auxiliary:
                {
                    int roll = Dice.RollD10();

                    if (roll % 2 == 0)
                    {
                        DeckModule deck = user.GetModule<DeckModule>();

                        if (deck != null)
                        {
                            deck.Draw();
                            deck.Draw();
                        }

                        BattleManager.ClaimBattleLog("청색 보조: 2 드로우");
                    }
                    else
                    {
                        StatusEffectModule status = user.GetModule<StatusEffectModule>();

                        int gat = RollD4();

                        if (status == null)
                        {
                            Debug.Log("의욕 부여 실패: StatusEffectModule 없음");
                            return;
                        }

                        status.AddStatus(StatusEffectType.Motivation, gat);
                        BattleManager.ClaimBattleLog($"의욕{gat} 증가");
                    }

                    break;
                }
        }
    }


    /// <summary>
    /// 자색 카드 효과.
    /// 행동 + 보조 행동 코스트를 사용한다.
    /// 크리티컬 없음.
    /// 1D10 결과에 따라 마법 카드를 생성해 핸드에 추가한다.
    /// </summary>
    private bool ResolvePurple(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (card == null || user == null)
            return false;

        if (useCost != CardUseCost.ActionAndAuxiliary)
            return false;

        DeckModule deck = GetDeckModule(user);

        if (deck == null)
            return false;

        int result = Dice.RollD10();

        CardData generatedCard = null;

        if (result == 1)
        {
            generatedCard = card.forbiddenMagicCard;
        }
        else if (result >= 2 && result <= 4)
        {
            generatedCard = card.attackMagicCard;
        }
        else if (result >= 5 && result <= 7)
        {
            generatedCard = card.defenseMagicCard;
        }
        else if (result >= 8 && result <= 10)
        {
            generatedCard = card.buffMagicCard;
        }

        if (generatedCard == null)
        {
            Debug.LogWarning(
                $"자색 카드 생성 실패: 결과 {result}에 해당하는 마법 카드가 {card.cardName}에 연결되지 않았습니다.");

            return false;
        }

        deck.AddCardToDeckAndShuffle(generatedCard);

        BattleManager.ClaimBattleLog($"{generatedCard.cardName} 생성");

        return true;
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
        LVModules lv = user.GetModule<LVModules>();

        if (lv == null)
            return;

        // 무색은 보정치 없이 크리티컬 판정
        DiceResult criticalResult = Dice.RollD10WithCritical(0, lv.Level);

        switch (useCost)
        {
            case CardUseCost.Action:
                {
                    if (target == null)
                        return;

                    int damage = RollColorlessDice(user);

                    if (criticalResult.criticalType == CriticalType.Critical)
                    {
                        damage = RollColorlessDice(user) + RollColorlessDice(user);
                    }
                    else if (criticalResult.criticalType == CriticalType.GreatCritical)
                    {
                        damage = Dice.RollD10() + Dice.RollD10();

                    }

                    DamageStruct damageInfo =
                        new DamageStruct
                        {
                            from = user.gameObject,
                            instigator = user.Controller,
                            damageAmount = damage,
                            critical = criticalResult.criticalType != CriticalType.None,
                            damageType = DamageType.Hand_to_hand_combat
                        };

                    CombatModule combat =target.GetModule<CombatModule>();

                    if (combat == null)
                        return;

                    combat.OnHit(damageInfo);
                    if (criticalResult.criticalType == CriticalType.GreatCritical)
                    {
                        BattleManager.ClaimBattleLog($"상위 크리티컬<br>{damage} 피해");
                    }
                    else if (criticalResult.criticalType == CriticalType.Critical)
                    {
                        BattleManager.ClaimBattleLog($"크리티컬<br>{damage} 피해");
                    }
                    else
                    {
                        BattleManager.ClaimBattleLog($"{damage} 피해");
                    }
                    Debug.Log($"무색 카드 피해: {damage} / 크리티컬: {criticalResult.criticalType}");

                    break;
                }

            case CardUseCost.Auxiliary:
                {
                    int restore = RollColorlessDice(user);

                    if (criticalResult.criticalType == CriticalType.Critical)
                    {
                        restore = RollColorlessDice(user) + RollColorlessDice(user);
                    }
                    else if (criticalResult.criticalType == CriticalType.GreatCritical)
                    {
                        restore = Dice.RollD10() + Dice.RollD10() + 5;
                    }

                    RestoreStruct restoreInfo =
                        new RestoreStruct
                        {
                            from = user.gameObject,
                            instigator = user.Controller,
                            restoreAmount = restore
                        };

                    CombatModule combat = user.GetModule<CombatModule>();

                    if (combat == null)
                        return;

                    combat.OnRestore(restoreInfo);
                    if (criticalResult.criticalType == CriticalType.GreatCritical)
                    {
                        BattleManager.ClaimBattleLog($"상위 크리티컬<br>{restore}생명력 회복");
                    }
                    else if (criticalResult.criticalType == CriticalType.Critical)
                    {
                        BattleManager.ClaimBattleLog($"크리티컬<br>{restore}생명력 회복");
                    }
                    else
                    {
                        BattleManager.ClaimBattleLog($"{restore}생명력 회복");
                    }
                    Debug.Log(
                        $"무색 카드 회복: {restore} / 크리티컬: {criticalResult.criticalType}");

                    break;
                }
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

        if (card.magicCardType != MagicCardType.None)
        {
            ResolveMagicCard(card, user, target, useCost);
            return true;
        }

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
                return ResolvePurple(card, user, target, useCost);

            // 무색 카드
            case CardColorType.Colorless:
                ResolveColorless(card, user, target, useCost);
                break;

            // 검은색 카드
            case CardColorType.Black:
                ResolveBlack(card, user, target, useCost);
                break;
        }

        return true;
    }
   
    /// <summary>
    /// 마법 카드 처리 함수
    /// </summary>
    /// <param name="card"></param>
    /// <param name="user"></param>
    /// <param name="target"></param>
    /// <param name="useCost"></param>
    private void ResolveMagicCard(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (!TryPayMagicCost(user, card.magicCardType))
        {
            Debug.Log("마법 카드 사용 실패: 생명력 코스트 지불 불가");
            return;
        }

        switch (card.magicCardType)
        {
            case MagicCardType.Forbidden:
                ResolveForbiddenMagic(card, user, target, useCost);
                break;

            case MagicCardType.Attack:
                ResolveAttackMagic(card, user, target, useCost);
                break;

            case MagicCardType.Defense:
                ResolveDefenseMagic(card, user, target, useCost);
                break;

            case MagicCardType.Buff:
                ResolveBuffMagic(card, user, target, useCost);
                break;
        }
    }

    /// <summary>
    /// 금지된 마법 카드.
    /// 크리티컬 없음.
    /// 현재는 종언 주사위 결과만 처리.
    /// </summary>
    private void ResolveForbiddenMagic(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (target == null)
        {
            Debug.Log("금지된 마법 실패: 대상 없음");
            return;
        }

        StatusEffectModule status =
            target.GetModule<StatusEffectModule>();

        if (status == null)
        {
            Debug.Log("금지된 마법 실패: 대상에게 StatusEffectModule 없음");
            return;
        }

        int doomValue = RollD10();

        status.AddDoom(doomValue);

        BattleManager.ClaimBattleLog($"금지된 마법: 종언 {doomValue} 부여");
    }

    /// <summary>
    /// 공격 마법 카드.
    /// 행동 코스트로 사용.
    /// 마법 코스트는 ResolveMagicCard에서 먼저 처리한다.
    /// 대상에게 마법 피해를 준다.
    /// </summary>
    private void ResolveAttackMagic(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (useCost != CardUseCost.ActionAndAuxiliary)
        {
            Debug.Log("공격 마법은 행동 코스트로만 사용할 수 있습니다.");
            return;
        }

        if (target == null)
        {
            Debug.Log("공격 마법 실패: 대상 없음");
            return;
        }

        int damage = 10 + Dice.RollD10();

        DamageStruct damageInfo = new DamageStruct
        {
            from = user.gameObject,
            instigator = user.Controller,
            damageAmount = damage,
            critical = false,
            damageType = DamageType.Magic
        };

        CombatModule combat = target.GetModule<CombatModule>();

        if (combat == null)
        {
            Debug.Log("공격 마법 실패: 대상에게 CombatModule이 없습니다.");
            return;
        }

        combat.OnHit(damageInfo);

        BattleManager.ClaimBattleLog($"{damage}피해");
    }

    /// <summary>
    /// 방어 마법 카드.
    /// 행동 코스트로 사용.
    /// 마법 코스트는 ResolveMagicCard에서 먼저 처리한다.
    /// 사용자에게 보호막을 부여한다.
    /// </summary>
    private void ResolveDefenseMagic(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        ArmorModule armorModule = user.GetModule<ArmorModule>();

        if (useCost != CardUseCost.ActionAndAuxiliary)
        {
            Debug.Log("방어 마법은 행동 코스트로만 사용할 수 있습니다.");
            return;
        }

        int shieldAmount = 200;

        // 보호막/임시 장갑 시스템이 아직 없다면 일단 로그만 처리
        armorModule.AddTemporaryArmor(shieldAmount);

        BattleManager.ClaimBattleLog($"임시 장갑{shieldAmount} 획득");
    }

    /// <summary>
    /// 버프 마법 카드.
    /// 행동 코스트로 사용.
    /// 마법 코스트는 ResolveMagicCard에서 먼저 처리한다.
    /// 아군 전체에게 축복/의지를 부여한다.
    /// </summary>
    private void ResolveBuffMagic(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (useCost != CardUseCost.ActionAndAuxiliary)
        {
            Debug.Log("버프 마법은 행동 코스트로만 사용할 수 있습니다.");
            return;
        }

        Debug.Log("버프 마법 사용: 아군 전체에게 축복/의지 부여");



        StatusEffectModule status = user.GetModule<StatusEffectModule>();
        

        if (status == null)
        {
            Debug.Log("축복 및 의지 부여 실패: StatusEffectModule 없음");
            return;
        }

        status.AddStatus(StatusEffectType.Blessing, 1);
        status.AddStatus(StatusEffectType.Motivation, 4);

        BattleManager.ClaimBattleLog($"축복 획득");
        BattleManager.ClaimBattleLog($"의지4 획득");

    }

    /// <summary>
    /// 마법 카드 공통 코스트.
    /// 일반 마법: 정신력 1D10 감소
    /// 금지된 마법: 정신력 10 + 1D10 감소
    /// </summary>
    private bool TryPayMagicCost(CharacterBase user, MagicCardType magicCardType)
    {
        if (user == null)
            return false;

        CombatModule combat = user.GetModule<CombatModule>();

        if (combat == null)
            return false;

        int hpCost = Dice.RollD10();

        if (magicCardType == MagicCardType.Forbidden)
        {
            hpCost += 10;
        }

        DamageStruct costDamage = new DamageStruct
        {
            from = user.gameObject,
            instigator = user.Controller,
            damageAmount = hpCost,
            critical = false,
            damageType = DamageType.Magic
        };

        combat.OnHit(costDamage);

        Debug.Log($"마법 코스트: 생명력 {hpCost} 감소");

        return true;

    }


    /// <summary>
    /// 플레이어 덱 찾기
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private DeckModule GetDeckModule(CharacterBase user)
    {
        if (user == null)
            return null;

        DeckModule deck = user.GetModule<DeckModule>();

        if (deck != null)
            return deck;

        ControllerBase controller = user.Controller;

        if (controller == null)
        {
            Debug.LogWarning($"{user.name}에 Controller가 없습니다.");
            return null;
        }

        CharacterBase owner = controller.GetComponent<CharacterBase>();

        if (owner == null)
        {
            Debug.LogWarning($"{controller.name}에 CharacterBase가 없습니다.");
            return null;
        }

        deck = owner.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{owner.name}에 DeckModule이 없습니다.");
            return null;
        }

        return deck;
    }

    /// <summary>
    /// 검은 카드 효과
    /// </summary>
    /// <param name="card"></param>
    /// <param name="user"></param>
    /// <param name="target"></param>
    /// <param name="useCost"></param>
    private void ResolveBlack(CardData card, CharacterBase user, CharacterBase target, CardUseCost useCost)
    {
        if (useCost != CardUseCost.ActionAndAuxiliary)
        {
            Debug.Log("검은색 카드는 행동 + 보조 행동 코스트가 필요합니다.");
            return;
        }

        StatModules stat = user.GetModule<StatModules>();

        if (stat == null)
        {
            Debug.Log("검은색 카드 실패: StatModules 없음");
            return;
        }

        StatType designatedStatType = stat.GetDesignatedStatType();

        switch (designatedStatType)
        {
            case StatType.Strength:
                ResolveBlackStrength(card, user, target);
                break;

            case StatType.Agility:
                ResolveBlackAgility(card, user, target);
                break;

            case StatType.Health:
                ResolveBlackHealth(card, user, target);
                break;

            case StatType.Intelligence:
                ResolveBlackIntelligence(card, user, target);
                break;

            case StatType.Will:
                ResolveBlackWill(card, user, target);
                break;
        }
    }


    private void ResolveBlackStrength(CardData card, CharacterBase user, CharacterBase target)
    {
        {
            if (user == null)
            {
                Debug.Log("검은 근력 카드 실패: 사용자 없음");
                return;
            }

            CombatModule combat =
                user.GetModule<CombatModule>();

            if (combat == null)
            {
                Debug.Log("검은 근력 카드 실패: 사용자에게 CombatModule 없음");
                return;
            }

            int damage = Dice.RollD8();

            DamageStruct damageInfo = new DamageStruct
            {
                from = user.gameObject,
                instigator = user.Controller,
                damageAmount = damage,
                critical = false,
                damageType = DamageType.Magic
            };

            combat.OnHit(damageInfo);

            Debug.Log($"검은 근력 카드: 자신에게 {damage} 피해");
        }
    }

    private void ResolveBlackAgility(CardData card, CharacterBase user, CharacterBase target)
    {
        if (target == null)
        {
            Debug.Log("검은 민첩 카드 실패: 대상 없음");
            return;
        }

        StatusEffectModule status =
            target.GetModule<StatusEffectModule>();

        if (status == null)
        {
            Debug.Log("검은 민첩 카드 실패: 대상에게 StatusEffectModule 없음");
            return;
        }

        int bindStack = 1;

        status.AddStatus(StatusEffectType.Bind, bindStack);

        Debug.Log($"검은 민첩 카드: {target.name}에게 속박 {bindStack} 부여");
    }

    private void ResolveBlackHealth(CardData card, CharacterBase user, CharacterBase target)
    {
        if (target == null)
        {
            Debug.Log("검은 건강 카드 실패: 대상 없음");
            return;
        }

        StatusEffectModule status =
            target.GetModule<StatusEffectModule>();

        if (status == null)
        {
            Debug.Log("검은 건강 카드 실패: 대상에게 StatusEffectModule 없음");
            return;
        }

        int vulnerableStack = 1;

        status.AddStatus(StatusEffectType.Vulnerable, vulnerableStack);

        Debug.Log($"검은 건강 카드: {target.name}에게 취약 {vulnerableStack} 부여");
    }

    private void ResolveBlackIntelligence(CardData card, CharacterBase user, CharacterBase target)
    {
        if (target == null)
        {
            Debug.Log("검은 지능 카드 실패: 대상 없음");
            return;
        }

        StatusEffectModule status =
            target.GetModule<StatusEffectModule>();

        if (status == null)
        {
            Debug.Log("검은 지능 카드 실패: 대상에게 StatusEffectModule 없음");
            return;
        }

        status.AddStatus(StatusEffectType.DrawBlock, 1);

        Debug.Log($"검은 지능 카드: {target.name}에게 드로우 제한 부여");
    }

    private void ResolveBlackWill(CardData card, CharacterBase user, CharacterBase target)
    {
        if (user == null)
        {
            Debug.Log("검은 의지 카드 실패: 사용자 없음");
            return;
        }

        CombatModule combat =
            user.GetModule<CombatModule>();

        if (combat == null)
        {
            Debug.Log("검은 의지 카드 실패: 사용자에게 CombatModule 없음");
            return;
        }

        int damage = Dice.RollD4();

        DamageStruct damageInfo = new DamageStruct
        {
            from = user.gameObject,
            instigator = user.Controller,
            damageAmount = damage,
            critical = false,
            damageType = DamageType.Magic
        };

        combat.OnHit(damageInfo);

        Debug.Log($"검은 의지 카드: 자신에게 {damage} 피해");
    }

}