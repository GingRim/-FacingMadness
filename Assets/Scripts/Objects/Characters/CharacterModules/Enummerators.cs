using UnityEngine;

public enum UIType
{
    None, Loading, Title, Movable, Menu, Info, Battle, Reward, Pause, Creation, Quit, SavePopUp, InComplete, CostHoverInfo,
    TargeHoverInfp, experimentHoverInfp, Cards, ResolverPopUp, _Length

}

/// <summary>
/// 대미지 타입
/// </summary>
public enum DamageType
{
    None, Hand_to_hand_combat, Long_range_combat, Magic, _Length
}


/// <summary>
/// 대응 타입
/// 1.방어 2.회피 3.반격
/// </summary>
 public enum ActionType
 {
    None, Guard, Evade, Counterattack, _Length
 }


/// <summary>
/// 코스트 타입
/// 1.행동 2.보조행동 3.대응
/// </summary>
 public enum CostType
 {
    None, Action, Auxiliary, Reaction, _Length  
 }
/// <summary>
/// 카드가 진짜로 사용하는 카드 코스트
/// </summary>
public enum CardUseCost
{
    None, Action, Auxiliary, ActionAndAuxiliary, HP, San, _Length
}


/// <summary>
/// 팀 타입
/// 1.아군 2.적군
/// </summary>
 public enum TeamType
 {
    None, Ally, Enemy, _Length
 }


/// <summary>
/// 카드 컬러 타입
/// 1.적색 2.황색 3.녹색 4.청색 5.자색 6.무색(중립)7.흑(디버프)
/// </summary>
public enum CardColorType
{
    None, Red, Yellow, Green, Blue, Purple, Colorless, Black, _Length
}


/// <summary>
/// 능력치 타입
/// 1. 힘 2. 민첩 3. 건강 4. 지능 5. 의지
/// </summary>
public enum StatType
{
    None, Strength, Agility, Health, Intelligence, Will, _Length
}


/// <summary>
/// 파생 통계(새부 능력치)
/// </summary>
public enum DerivedStatType
{ // 최대 체력, 최대 정신력, 우선권, 근접 보정, 원거리 보정, 마법 보정, 회복 보정, 가드 보정, 회피 보정, 반격 보정
    None, MaxHP, MaxSan, Initiative, MeleeBonus, RangedBonus, MagicBonus, RestoreBonus, GuardBonus, EvadeBonus, 
    CounterattackBonus, 
    _Length
}


/// <summary>
/// 키워드
/// </summary>
/// 1. 다이스 2. 판정, 3.보정 4. 축복 5. 저주 6.대 성공 7. 펌블
public enum KeywordType
{
    None, D, Adjudgment, Bonus, Blessing, Cursed, GreatSuccess, Fumble, T, _Length
}


/// <summary>
/// 카드가 존재할 수 있는 영역
/// </summary>
/// 1.덱 2.손패 3.묘지 4.소멸 5.제거
public enum CardZoneType
{
    None, Deck, Hand, Graveyard, Exhaust, Remove, _Length
}

/// <summary>
/// 1.라운드 시작 처리 2.캐릭터 행동 턴 3.카드/대상 선언 4.대응 처리 5.대미지 계산 및 적용 6.캐릭터 턴 종료
/// </summary>
public enum BattlePhaseType
{
    None, StandbyPhase, MainPhase, DeclarePhase, ChainPhase, DamageStep, EndPhase, _Length
}

/// <summary>
/// 크리티컬 타입 1.크리티컬 2. 상위 크리티컬
/// </summary>
public enum CriticalType
{
    None, Critical, GreatCritical, _Length
}
/// <summary>
/// 마법 카드 타입 1.금지 2.공격 3.방어 4.버프
/// </summary>
public enum MagicCardType
{
    None, Forbidden, Attack, Defense, Buff, _Length
}
