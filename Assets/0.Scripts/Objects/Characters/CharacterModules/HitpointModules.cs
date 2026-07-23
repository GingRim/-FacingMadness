using System;
using UnityEngine;

// 공격, 대응, 지원, 이동, 광기, 행동(이벤트)?

public struct DamageStruct // 이벤트 데이터 팻킷
{
    public GameObject from;
    public ControllerBase instigator;

    // 주사위 결과
    public int diceValue;

    // 계산 과정에서 불러온 보정치
    public int abilityModifier;
    public int statusModifier;

    // 대응 과정에서 감소한 값
    public int reactionReduction;

    // 실제 적용된 장갑 감소량
    public int armorReduction;

    // 최종 적용 피해
    public int damageAmount;

    public bool critical;
    public bool highCritical;

    public DamageType damageType;
    public bool canCounter;
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
