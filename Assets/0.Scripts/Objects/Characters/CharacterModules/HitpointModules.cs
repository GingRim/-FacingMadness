using System;
using UnityEngine;

// 공격, 대응, 지원, 이동, 광기, 행동(이벤트)?

public struct DamageStruct // 이벤트 데이터 팻킷
{
    public GameObject from; // 양식
    public ControllerBase instigator; // 선동가(명령자)

    /// <summary>
    /// 모든 계산이 끝난 뒤 실제 적용할 최종 피해량
    /// </summary>
    public int damageAmount;

    /// <summary>
    /// 공격에서 나온 주사위 값.
    /// 여러 주사위라면 주사위 결과의 합계.
    /// </summary>
    public int diceValue;// 크리티컬

    /// <summary>
    /// 이 공격이 능력 보정치를 사용하는지 여부.
    /// 보정치가 0이어도 사용하는 공격인지 구분하기 위해 필요.
    /// </summary>
    public bool hasAbilityModifier;


    /// <summary>
    /// 근력·민첩·지능 등의 능력 보정치.
    /// </summary>
    public int abilityModifier;
    public bool highCritical; // 상위 크리티컬


    public DamageType damageType; // 대미지 유형


    /// <summary>
    /// 이 공격이 반격 가능한 공격인지.
    /// 근접 공격 또는 근접 사격이면 true.
    /// </summary>
    public bool canCounter;

    /// <summary>
    /// 선택된 대응.
    /// None이면 대응하지 않음.
    /// </summary>
    public ActionType reactionType;
}
    
public struct RestoreStruct
{
    public GameObject from;
    public ControllerBase instigator;
    public int restoreAmount;
}
public abstract class HitpointModules : CharacterModule
{
    protected FillValue fill = new FillValue();

    public sealed override Type RegistrationType => typeof(HitpointModules);

    public int Max => fill.Max;
    public int Min => fill.Min;
    public int Current => fill.Current;

    public bool IsFullHealth => fill.IsMax;

    // FillValue.IsEmpty에 의존하지 않고 0을 포함해 직접 판정
    public bool IsEmpty => fill.Current <= fill.Min;

    /// <summary>
    /// 생명력이 남아 있는 상태에서 0 이하가 된 순간 한 번 발생
    /// </summary>
    public event Action OnEmpty;


    public int TakeDamage(in DamageStruct damageInfo)
    {
        int before = fill.Current;

        fill.DecreaseCurrent(damageInfo.damageAmount);

        int actualDamage = before - fill.Current;

        if (before > fill.Min &&
            fill.Current <= fill.Min)
        {
            OnEmpty?.Invoke();
        }

        return actualDamage;
    }

    public int TakeRestore(in RestoreStruct restoreInfo)
    {
        int before = fill.Current;

        fill.IncreaseCurrent(restoreInfo.restoreAmount);

        return fill.Current - before;
    }

    public void InitializeHP(int maxHp)
    {
        fill.SetMax(maxHp);
        fill.SetCurrent(maxHp);
    }
}
