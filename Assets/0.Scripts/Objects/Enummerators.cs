using NUnit.Framework.Constraints;
using UnityEngine;

public enum UIType
{
    None, Loading, Title, Movable, Menu, Info, Battle, Reward, Pause, Creation, Quit, SavePopUp, InComplete, CostHoverInfo,
    TargeHoverInfp, ExperimentHoverInfp, Cards, ResolverPopUp, ActionPopUp, GameOver, Field, _Length

}

/// <summary>
/// 대미지 타입
/// </summary>
public enum DamageType
{
    None, Hand_to_hand_combat, Long_range_combat, Magic, Physical, _Length
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
    None, Self, Ally, Enemy, _Length
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
/// <summary>
/// 상태 이상
/// </summary>
public enum StatusEffectType
{
    None, Haste, Bind, Motivation, Lethargy, Blessing, Curse, Vulnerable, Stun, Doom, DrawBlock, _Length
}

public enum BattleTurnState
{
    None, BattleStart, RoundStart, TurnStart, WaitingAction, WaitingReaction, TurnEnd, RoundEnd, BattleEnd, _Length
}


public enum CardDropResult
{
    Invalid,
    OpenPopup,
    UseDirect
}

/// <summary>
/// 현제 상테에 따른 카드 효과 변경점
/// </summary>
public enum CardUseContext
{
    Field,
    Battle
}

public enum FieldLineType
{
    Normal,
    Red,
    Hidden
}

public enum FieldEventType
{
    Core,       // 미션 진행에 필요한 핵심 이벤트
    Stat,       // 능력치·카드 판정 이벤트
    MacGuffin   // 진행 없이 행동력을 소비하는 일반 사건
}

public enum FieldTurnState
{
    Inactive,
    TurnStart,
    PlayerAction,
    Event,
    TurnEnd,
    MythTurn,
    GameOver,
    MissionClear,
    MissionFailed,
    _Length
}

public enum FieldCardCheckResult
{
    Success,
    Failure,
    Fumble
}

public enum FieldEffectDiceType
{
    None,
    D4,
    D6,
    D8,
    D10
}

public enum FieldHitpointEffectType
{
    Damage,
    Restore
}

public enum FieldSanityEffectType
{
    Damage,
    Restore
}

public enum FieldItemEffectType
{
    Add,
    Remove
}


public enum FieldLineStateEffectType
{
    RevealHidden,
    UnlockRed,
    ChangeToNormal,
    ChangeToRed,
    ChangeToHidden
}

public enum MythEventType
{
    None,
    Hallucination,
    Obstacle,
    Pollution,
    Oblivion,
    _Length
}


/// <summary>
/// 선택지가 필드 진행 중 반복 사용 가능한지 구분한다.
/// </summary>
public enum FieldChoiceUsageType
{
    Repeatable,     // 계속 등장 가능
    OncePerField    // 이번 필드에서 한 번 선택하면 제거
}

/// <summary>
/// 이벤트 페이지의 선택지 표시 방식을 구분한다.
/// </summary>
public enum FieldEventPageDisplayType
{
    Fixed,  // 등록된 선택지를 순서대로 표시
    Random  // 등록된 선택지 중 최대 5개를 무작위 표시
}

/// <summary>
/// 선택지가 하위 페이지로 이동하는지,
/// 실제 효과를 실행하는지 구분한다.
/// </summary>
public enum FieldChoiceActionType
{
    Navigate,   // 하위 선택지 페이지로 이동
    Resolve     // 효과 적용 후 이벤트 결과 표시
}

/// <summary>
/// 필드 이벤트 선택지의 성격을 구분한다.
/// </summary>
public enum FieldChoiceType
{
    /// <summary>
    /// 미션 진행에 영향을 주는 핵심 선택지.
    /// </summary>
    Core,

    /// <summary>
    /// 능력치를 활용하는 일반 선택지.
    /// </summary>
    Stat,

    /// <summary>
    /// 설명, 작은 보상, 상황 연출 등을 담당하는 선택지.
    /// </summary>
    MacGuffin
}

/// <summary>
/// 필드 이벤트 선택지의 실행 방식을 구분한다.
/// </summary>
public enum FieldChoiceExecutionType
{
    /// <summary>
    /// 별도의 능력치 판정 없이 결과를 실행한다.
    /// </summary>
    Direct,

    /// <summary>
    /// 지정된 능력치 판정의 성공 또는 실패에 따라 결과를 실행한다.
    /// 대응 색상 카드를 사용하면 판정을 확정 성공시킬 수 있다.
    /// </summary>
    StatCheck
}

/// <summary>
/// 카드에 부여할 수 있는 키워드 종류입니다.
/// 키워드는 이벤트 선택 조건, 판정 보조 및 내구도 소비에 사용됩니다.
/// </summary>
public enum CardKeywordType
{
    None,

    Light,     // 광원
    Unignited, // 비점화
    Ignition,  // 점화

    Blade,     // 날붙이
    Blunt,     // 둔기
    Tool,      // 도구
    Medicine,  // 약품
    HolyRelic, // 성물
    Binding,   // 결박
    
    Key,       // 열쇠
    Record,    // 기록

    _Length
}