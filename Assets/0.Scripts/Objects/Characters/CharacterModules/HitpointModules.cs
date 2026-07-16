using System;
using UnityEngine;

// 공격, 대응, 지원, 이동, 광기, 행동(이벤트)?

public struct DamageStruct // 이벤트 데이터 팻킷
{
    public GameObject from; // 양식
    public ControllerBase instigator; // 선동가(명령자)
    public int damageAmount; // 대미지 양
    public bool critical;// 크리티컬
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

    public sealed override System.Type RegistrationType => typeof(HitpointModules);

    public int Max =>   fill.Max;
    public int Min => fill.Min;
    public int Current => fill.Current;
    public bool IsFullHealth => fill.IsMax;
    public bool IsEmpty => fill.IsEmpty;
    

    public int TakeDamage(in DamageStruct damageInfo)
    {
        fill.DecreaseCurrent(damageInfo.damageAmount);
        return damageInfo.damageAmount;
    }
    public int TakeRestore(in RestoreStruct restoreInfo)
    {
        fill.IncreaseCurrent(restoreInfo.restoreAmount);
        return restoreInfo.restoreAmount;
    }

    public void InitializeHP(int maxHp)
    {
        fill.SetMax(maxHp);
        fill.SetCurrent(maxHp);
    }


}
