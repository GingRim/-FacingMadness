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

    public int GetReducedDamage(int damage, DamageType damageType)
    {
        if (damage <= 0)
            return 0;

        int reduceAmount = 0;
        int armor = TotalArmor;

        switch (damageType)
        {
            case DamageType.Hand_to_hand_combat:
                reduceAmount = armor;
                break;

            case DamageType.Long_range_combat:
                reduceAmount = armor / 2;
                break;

            case DamageType.Magic:
                reduceAmount = 0;
                break;
        }

        return Mathf.Max(0, damage - reduceAmount);
    }
}