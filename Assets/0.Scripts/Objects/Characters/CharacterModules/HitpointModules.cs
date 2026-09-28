using System;
using UnityEngine;

// 공격, 대응, 지원, 이동, 광기, 행동(이벤트)?

public struct DamageStruct // 이벤트 데이터 팻킷
{
    public GameObject from;
    public ControllerBase instigator;

    /// <summary>
    /// 카드 효과에서 나온 주사위 결과.
    /// 주사위를 사용하지 않는 피해라면 0.
    /// </summary>
    public int diceValue;

    /// <summary>
    /// OnHit에 들어오기 전까지 계산된 피해량.
    /// 주사위와 카드 자체 보정치가 포함된 값.
    /// </summary>
    public int damageAmount;

    public bool critical;
    public bool highCritical;

    public DamageType damageType;

    public bool canCounter;
    public ActionType reactionType;

    internal int armorReduction;
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

    /// <summary>
    /// 현재 또는 최대 생명력이 변경되면 발생합니다.
    /// </summary>
    public event Action<int, int> OnHitpointChanged;

    /// <summary>
    /// 생명력이 남아 있던 대상이 피해로 0이 되었을 때
    /// 마지막 피해 정보를 함께 전달한다.
    /// 직접 처치자를 판정하는 미션 목표에서 사용한다.
    /// </summary>
    public event Action<DamageStruct> OnKilled;


    public int TakeDamage(in DamageStruct damageInfo)
    {
        int before = fill.Current;

        fill.DecreaseCurrent(damageInfo.damageAmount);

        int actualDamage = before - fill.Current;

        if (actualDamage > 0)
            OnHitpointChanged?.Invoke(fill.Current, fill.Max);

        if (before > fill.Min && fill.Current <= fill.Min)
        {
            OnKilled?.Invoke(damageInfo);
            OnEmpty?.Invoke();
        }

        return actualDamage;
    }

    public int TakeRestore(in RestoreStruct restoreInfo)
    {
        int before = fill.Current;

        fill.IncreaseCurrent(restoreInfo.restoreAmount);

        int actualRestore = fill.Current - before;

        if (actualRestore > 0)
            OnHitpointChanged?.Invoke(fill.Current, fill.Max);

        return actualRestore;
    }

    public void InitializeHP(int maxHp)
    {
        fill.SetMax(maxHp);
        fill.SetCurrent(maxHp);
        OnHitpointChanged?.Invoke(fill.Current, fill.Max);
    }
}
