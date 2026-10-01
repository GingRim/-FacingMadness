using System;
using UnityEngine;

public class ArmorModule : CharacterModule
{
    public sealed override System.Type RegistrationType => typeof(ArmorModule);

    [SerializeField] private int baseArmor;
    [SerializeField] private int temporaryArmor;

    public int BaseArmor => baseArmor;
    public int TemporaryArmor => temporaryArmor;
    public int TotalArmor => baseArmor + temporaryArmor;

    public void SetBaseArmor(int value)
    {
        baseArmor = Mathf.Max(0, value);
    }

    public void AddBaseArmor(int value)
    {
        baseArmor = Mathf.Max(0, baseArmor + value);
    }

    public void AddTemporaryArmor(int value)
    {
        if (value <= 0)
            return;

        temporaryArmor += value;

        Debug.Log($"임시 장갑 {value} 획득 / 현재 임시 장갑: {temporaryArmor}");
    }

    public void ReduceTemporaryArmorAtRoundEnd()
    {
        if (temporaryArmor <= 0)
            return;

        int reduceAmount = temporaryArmor;

        temporaryArmor = Mathf.Max(0, temporaryArmor - reduceAmount);

        Debug.Log($"라운드 종료: 임시 장갑 {reduceAmount} 감소 / 현재 임시 장갑: {temporaryArmor}");
    }

    public int GetReducedDamage(
        int damage,
        DamageType damageType,
        DamageFormType damageForm)
    {
        if (damage <= 0)
            return 0;

        int armor = TotalArmor;

        // 근접·원거리 여부와 관계없이 장갑 전체를 적용합니다.
        // 관통 피해만 장갑 감소량을 절반으로 계산합니다.
        int reduceAmount = damageForm == DamageFormType.Pierce
            ? armor / 2
            : armor;

        return Mathf.Max(0, damage - reduceAmount);
    }

    /// <summary>
    /// 기존 호출부 호환용. 피해 형식이 없는 공격은 타격으로 처리합니다.
    /// </summary>
    public int GetReducedDamage(int damage, DamageType damageType)
    {
        return GetReducedDamage(damage, damageType, DamageFormType.Blunt);
    }

    public void ClearTemporaryArmor()
    {
        if (temporaryArmor <= 0)
            return;

        Debug.Log($"임시 장갑 제거: {temporaryArmor}");

        temporaryArmor = 0;
    }
}
