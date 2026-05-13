using UnityEngine;

/// <summary>
/// 대미지 타입
/// </summary>
public enum DamageType
{
    nomur , Physical, _Length
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
/// 팀 타입
/// 1.아군 2.적군
/// </summary>
 public enum TeamType
 {
    None, Ally, Enemy, _Length
 }
/// <summary>
/// 카드 컬러 타입
/// 1.적색 2.황색 3.녹색 4.청색 5.자색 6.무색(중립)
/// </summary>
public enum CardColorType
{
    None, Red, Yellow, Green, Blue, Purple, Colorless, _Length
}
/// <summary>
/// 카드 태그
/// 1.공격 2.회복 3.버프 4.디버프 5.마법
/// </summary>
public enum CardTagType
{
    Attack, Restore, Buff, Debuff, Magic, _Length   
}
/// <summary>
/// 능력치 타입
/// 1. 힘 2. 민첩 3. 건강 4. 지능 5. 의지
/// </summary>
public enum StatType
{
    Strength, Agility, Health, Intelligence, Will, _Length
}
/// <summary>
/// 파생 통계(새부 능력치)
/// </summary>
public enum DerivedStatType
{ // 최대 체력, 최대 정신력, 우선권, 근접 보정, 원거리 보정, 마법 보정, 회복 보정, 가드 보정, 회피 보정, 반격 보정
    MaxHP, MaxSan, Initiative, MeleeBonus, RangedBonus, MagicBonus, RestoreBonus, GuardBonus, EvadeBonus, 
    CounterattackBonus, 
    _Length
}

/// <summary>
/// 키워드
/// </summary>
/// 1. 다이스 2. 판정, 3.보정 4. 축복 5. 저주 6.대 성공 7. 펌블
public enum KeywordType
{
    D, Adjudgment, Bonus, Blessing, Cursed, GreatSuccess, Fumble, _Length
}