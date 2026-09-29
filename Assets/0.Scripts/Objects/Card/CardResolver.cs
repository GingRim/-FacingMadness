using System;
using System.Collections.Generic;
using UnityEngine;
using static Dice;


/// <summary>
/// 카드 효과 처리기
/// 카드의 색상과 사용 코스트에 따라 실제 효과를 실행한다.
/// </summary>
public class CardResolver
{
    /// <summary>
    /// 전투 카드 효과
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
    /// 이전 호출부 호환용입니다. 카드 자체의 필드 사용은 지원하지 않습니다.
    /// 행동력 소비와 효과 적용 없이 실패를 반환합니다.
    /// </summary>
    public bool UseField(CardData card, CharacterBase user, FieldEventContext context)
    {
        return false;
    }

    /// <summary>
    /// 카드 사용 전 코스트 지불
    /// </summary>
    /// <param name="user">카드 사용자</param>
    /// <param name="useCost">선택한 사용 방식</param>
    /// <returns>지불 성공 여부</returns>
    private bool TryPayCost(CharacterBase user, CardUseCost useCost)
    {
        ActionPointModule actionPoint = user.GetModule<ActionPointModule>();

        if (actionPoint == null)
            return false;

        return actionPoint.TryUse(GetActionPointCost(useCost));
    }

    private int GetActionPointCost(CardUseCost useCost)
    {
        return useCost == CardUseCost.ActionAndAuxiliary ? 2 : 1;
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

        if (derived == null || lv == null)
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

                    RestoreStruct restoreInfo = new RestoreStruct
                    {
                        from = user.gameObject,
                        instigator = user.Controller,
                        restoreAmount = restore
                    };

                    CombatModule combat = restreTarget.GetModule<CombatModule>();

                    if (combat == null)
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
        DerivedStatModule derived = user.GetModule<DerivedStatModule>();

        LVModules lv = user.GetModule<LVModules>();

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
                            deck.DrawInstance();
                            deck.DrawInstance();
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
    /// 무색 카드의 기본 및 키워드 전투 효과.
    /// 키워드가 없는 카드는 레벨별 기본 주사위 피해 또는 임시 장갑을 사용합니다.
    /// 키워드 카드는 행동 코스트만 사용하며 키워드 조합으로 효과를 결정합니다.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="user"></param>
    /// <param name="target"></param>
    /// <param name="useCost"></param>
    private bool ResolveColorless(
        CardData card,
        CardInstance cardInstance,
        CharacterBase user,
        CharacterBase target,
        CardUseCost useCost)
    {
        bool hasKeywords =
            cardInstance != null
                ? cardInstance.HasKeywords
                : card.Keywords != null && card.Keywords.Count > 0;

        if (!hasKeywords)
        {
            if (useCost == CardUseCost.Action)
                return ApplyColorlessDamage(user, target, card.RollColorlessDamage());

            if (useCost == CardUseCost.Auxiliary)
                return ApplyColorlessArmor(user, card.RollColorlessArmor());

            return false;
        }

        // 키워드 무색 카드는 보조 행동 효과를 사용할 수 없습니다.
        if (useCost != CardUseCost.Action)
            return false;

        if (HasKeyword(card, cardInstance, CardKeywordType.Tool) ||
            HasKeyword(card, cardInstance, CardKeywordType.Key) ||
            HasKeyword(card, cardInstance, CardKeywordType.Record))
        {
            return false;
        }

        if (HasKeyword(card, cardInstance, CardKeywordType.Medicine))
        {
            return ApplyColorlessHealing(user, target, Dice.RollD8() + 4);
        }

        if (target == null)
            return false;

        int damage;

        if (HasKeyword(card, cardInstance, CardKeywordType.Blade))
        {
            damage = Dice.RollD4() + 3;
        }
        else if (HasKeyword(card, cardInstance, CardKeywordType.Blunt))
        {
            damage = Dice.RollD6() + 2;
        }
        else if (HasKeyword(card, cardInstance, CardKeywordType.HolyRelic))
        {
            damage = 4;
        }
        else
        {
            // 광원·비점화·점화·결박은 기본 1D4 공격에서 시작합니다.
            damage = Dice.RollD4();
        }

        if (HasKeyword(card, cardInstance, CardKeywordType.Ignition))
        {
            damage += Dice.RollD4();
        }

        if (!ApplyColorlessDamage(user, target, damage))
            return false;

        if (HasKeyword(card, cardInstance, CardKeywordType.Binding))
        {
            StatusEffectModule status =
                target.GetModule<StatusEffectModule>();

            if (status != null)
            {
                int bind = Dice.RollD4();
                status.AddStatus(StatusEffectType.Bind, bind);
                BattleManager.ClaimBattleLog($"속박 {bind} 부여");
            }
            else
            {
                Debug.LogWarning("결박 효과 실패: 대상에게 StatusEffectModule이 없습니다.");
            }
        }

        return true;
    }

    private bool ApplyColorlessDamage(
        CharacterBase user,
        CharacterBase target,
        int damage)
    {
        if (user == null || target == null)
            return false;

        CombatModule combat = target.GetModule<CombatModule>();

        if (combat == null)
            return false;

        DamageStruct damageInfo = new DamageStruct
        {
            from = user.gameObject,
            instigator = user.Controller,
            damageAmount = damage,
            critical = false,
            damageType = DamageType.Hand_to_hand_combat
        };

        combat.OnHit(damageInfo);
        BattleManager.ClaimBattleLog($"{damage} 피해");
        return true;
    }

    private bool ApplyColorlessArmor(CharacterBase user, int armor)
    {
        if (user == null)
            return false;

        ArmorModule armorModule = user.GetModule<ArmorModule>();

        if (armorModule == null)
            return false;

        armorModule.AddTemporaryArmor(armor);
        BattleManager.ClaimBattleLog($"임시 장갑 {armor} 획득");
        return true;
    }

    private bool ApplyColorlessHealing(
        CharacterBase user,
        CharacterBase target,
        int restore)
    {
        if (user == null || target == null)
            return false;

        CombatModule combat = target.GetModule<CombatModule>();

        if (combat == null)
            return false;

        RestoreStruct restoreInfo = new RestoreStruct
        {
            from = user.gameObject,
            instigator = user.Controller,
            restoreAmount = restore
        };

        combat.OnRestore(restoreInfo);
        BattleManager.ClaimBattleLog($"생명력 {restore} 회복");
        return true;
    }

    private bool HasKeyword(
        CardData card,
        CardInstance cardInstance,
        CardKeywordType keyword)
    {
        return cardInstance != null
            ? cardInstance.HasKeyword(keyword)
            : card != null && card.HasKeyword(keyword);
    }

    /// <summary>
    /// 코스트 사용 가능 여부만 확인
    /// </summary>
    public bool CanUse(CardData selectedCard, CharacterBase user, CardUseCost useCost)
    {
        ActionPointModule actionPoint = user.GetModule<ActionPointModule>();

        if (actionPoint == null)
            return false;

        return actionPoint.CanUse(GetActionPointCost(useCost));
    }

    /// <summary>
    /// 런타임 키워드와 내구도를 포함해 카드 사용 가능 여부를 확인합니다.
    /// </summary>
    public bool CanUse(
        CardInstance selectedCard,
        CharacterBase user,
        CardUseCost useCost)
    {
        if (selectedCard == null || selectedCard.Data == null || selectedCard.IsDepleted)
            return false;

        if (selectedCard.Color == CardColorType.Colorless &&
            selectedCard.HasKeywords &&
            useCost != CardUseCost.Action)
        {
            return false;
        }

        if (selectedCard.Color == CardColorType.Colorless &&
            (selectedCard.HasKeyword(CardKeywordType.Tool) ||
             selectedCard.HasKeyword(CardKeywordType.Key) ||
             selectedCard.HasKeyword(CardKeywordType.Record)))
        {
            return false;
        }

        return CanUse(selectedCard.Data, user, useCost);
    }

    /// <summary>
    /// CardInstance의 런타임 키워드로 무색 카드 효과를 결정합니다.
    /// </summary>
    public bool UseWithoutCostCheck(
        CardInstance cardInstance,
        CharacterBase user,
        CharacterBase target,
        CardUseCost useCost)
    {
        if (cardInstance == null || cardInstance.Data == null || user == null)
            return false;

        if (!CanUse(cardInstance, user, useCost))
            return false;

        if (!TryPayCost(user, useCost))
            return false;

        CardData card = cardInstance.Data;

        if (card.magicCardType != MagicCardType.None)
        {
            ResolveMagicCard(card, user, target, useCost);
            return true;
        }

        if (cardInstance.Color == CardColorType.Colorless)
        {
            return ResolveColorless(
                card,
                cardInstance,
                user,
                target,
                useCost);
        }

        if (card.IsAbilityCard)
        {
            return ResolveAbilityCard(
                card,
                cardInstance.CurrentGrade,
                cardInstance.GradeBonus,
                user,
                target,
                useCost);
        }

        return ResolveNonColorlessCard(card, user, target, useCost);
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

        if (card.color == CardColorType.Colorless)
        {
            return ResolveColorless(card, null, user, target, useCost);
        }

        if (card.IsAbilityCard)
        {
            return ResolveAbilityCard(
                card,
                card.CardGrade,
                card.GradeBonus,
                user,
                target,
                useCost);
        }

        return ResolveNonColorlessCard(card, user, target, useCost);
    }

    private bool ResolveAbilityCard(
        CardData card,
        int cardGrade,
        int gradeBonus,
        CharacterBase user,
        CharacterBase target,
        CardUseCost useCost)
    {
        StatType statType = GetAbilityCardStat(card.color);

        if (statType == StatType.None)
            return false;

        StatModules stats = user.GetModule<StatModules>();
        int statModifier = stats != null ? stats.GetModifier(statType) : 0;
        int amount = Mathf.Max(0, statModifier + gradeBonus);

        if (useCost == CardUseCost.Auxiliary)
        {
            return ApplyAbilityCardSelfEffect(
                card,
                cardGrade,
                user,
                amount);
        }

        if (target == null)
            return false;

        int dice = RollAbilityBaseDice(user);
        int damage = Mathf.Max(0, dice + statModifier + gradeBonus);

        return ApplyColorlessDamage(user, target, damage);
    }

    private bool ApplyAbilityCardSelfEffect(
        CardData card,
        int cardGrade,
        CharacterBase user,
        int amount)
    {
        if (card == null)
            return false;

        CardColorType color = card.color;

        switch (color)
        {
            case CardColorType.Red:
                return ApplyColorlessArmor(user, amount);

            case CardColorType.Yellow:
            {
                StatusEffectModule status = user.GetModule<StatusEffectModule>();
                if (status == null)
                    return false;

                status.AddStatus(StatusEffectType.Haste, amount);
                BattleManager.ClaimBattleLog($"가속 {amount} 획득");
                return true;
            }

            case CardColorType.Green:
                return ApplyColorlessHealing(user, user, amount);

            case CardColorType.Blue:
            {
                StatusEffectModule status = user.GetModule<StatusEffectModule>();
                if (status == null)
                    return false;

                status.AddStatus(StatusEffectType.Motivation, amount);
                BattleManager.ClaimBattleLog($"의욕 {amount} 획득");
                return true;
            }

            case CardColorType.Purple:
            {
                SanityModule sanity = user.GetModule<SanityModule>();
                StatusEffectModule status = user.GetModule<StatusEffectModule>();

                if (sanity == null || status == null)
                    return false;

                int grade = Mathf.Clamp(cardGrade, 1, 5);
                int sanityPenaltyBase = 20 + grade * 5;
                int sanityDamage = Mathf.Max(0, sanityPenaltyBase - amount);

                sanity.TakeSanityDamage(sanityDamage);
                status.AddStatus(StatusEffectType.Blessing, 1);

                int generatedCount = AddWillGradeMagicCards(card, user, grade);

                if (generatedCount > 0)
                {
                    BattleManager.ClaimBattleLog(
                        $"정신력 {sanityDamage} 감소 / 축복 획득 / 마법 {generatedCount}장 추가");
                }
                else
                {
                    BattleManager.ClaimBattleLog(
                        $"정신력 {sanityDamage} 감소 / 축복 획득");
                }

                return true;
            }

            default:
                return false;
        }
    }

    /// <summary>
    /// 의지 능력치 카드의 현재 등급에 따라 마법 카드를 덱에 추가합니다.
    /// 1등급: 추가 없음
    /// 2등급: 종언을 제외한 공격·방어·축복 중 무작위 1장
    /// 3등급: 종언을 포함한 전체 목록 중 무작위 1장
    /// 4등급: 종언을 포함한 전체 목록 중 무작위 2장
    /// 5등급: 전체 목록 중 무작위 1장 + 공격·방어·축복 각 1장
    /// 연결되지 않은 카드 참조는 안전하게 제외합니다.
    /// </summary>
    private int AddWillGradeMagicCards(
        CardData sourceCard,
        CharacterBase user,
        int cardGrade)
    {
        if (sourceCard == null || user == null || cardGrade <= 1)
            return 0;

        DeckModule deck = GetDeckModule(user);

        if (deck == null)
            return 0;

        int addedCount = 0;

        switch (cardGrade)
        {
            case 2:
                addedCount += AddRandomMagicCard(
                    deck,
                    sourceCard.attackMagicCard,
                    sourceCard.defenseMagicCard,
                    sourceCard.buffMagicCard);
                break;

            case 3:
                addedCount += AddRandomMagicCard(
                    deck,
                    sourceCard.forbiddenMagicCard,
                    sourceCard.attackMagicCard,
                    sourceCard.defenseMagicCard,
                    sourceCard.buffMagicCard);
                break;

            case 4:
                for (int i = 0; i < 2; i++)
                {
                    addedCount += AddRandomMagicCard(
                        deck,
                        sourceCard.forbiddenMagicCard,
                        sourceCard.attackMagicCard,
                        sourceCard.defenseMagicCard,
                        sourceCard.buffMagicCard);
                }
                break;

            default:
                addedCount += AddRandomMagicCard(
                    deck,
                    sourceCard.forbiddenMagicCard,
                    sourceCard.attackMagicCard,
                    sourceCard.defenseMagicCard,
                    sourceCard.buffMagicCard);

                addedCount += AddMagicCard(deck, sourceCard.attackMagicCard);
                addedCount += AddMagicCard(deck, sourceCard.defenseMagicCard);
                addedCount += AddMagicCard(deck, sourceCard.buffMagicCard);
                break;
        }

        return addedCount;
    }

    /// <summary>
    /// 유효한 후보 중 하나를 무작위로 골라 덱에 추가합니다.
    /// 후보가 모두 비어 있으면 아무 작업도 하지 않습니다.
    /// </summary>
    private int AddRandomMagicCard(DeckModule deck, params CardData[] candidates)
    {
        if (deck == null || candidates == null || candidates.Length == 0)
            return 0;

        List<CardData> validCards = new();

        foreach (CardData candidate in candidates)
        {
            if (candidate != null)
                validCards.Add(candidate);
        }

        if (validCards.Count == 0)
            return 0;

        CardData selected = validCards[UnityEngine.Random.Range(0, validCards.Count)];

        return AddMagicCard(deck, selected);
    }

    /// <summary>
    /// 지정한 마법 카드를 덱에 추가하고 섞습니다.
    /// </summary>
    private int AddMagicCard(DeckModule deck, CardData magicCard)
    {
        if (deck == null || magicCard == null)
            return 0;

        return deck.AddCardToDeckAndShuffle(magicCard) != null ? 1 : 0;
    }

    private StatType GetAbilityCardStat(CardColorType color)
    {
        switch (color)
        {
            case CardColorType.Red: return StatType.Strength;
            case CardColorType.Yellow: return StatType.Agility;
            case CardColorType.Green: return StatType.Health;
            case CardColorType.Blue: return StatType.Intelligence;
            case CardColorType.Purple: return StatType.Will;
            default: return StatType.None;
        }
    }

    private int RollAbilityBaseDice(CharacterBase user)
    {
        LVModules level = user.GetModule<LVModules>();
        int currentLevel = level != null ? level.Level : 1;

        if (currentLevel >= 10)
            return Dice.RollD8();

        if (currentLevel >= 5)
            return Dice.RollD6();

        return Dice.RollD4();
    }

    private bool ResolveNonColorlessCard(
        CardData card,
        CharacterBase user,
        CharacterBase target,
        CardUseCost useCost)
    {
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
