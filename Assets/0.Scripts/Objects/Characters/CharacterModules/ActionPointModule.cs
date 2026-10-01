using System;
using UnityEngine;

public class ActionPointModule : CharacterModule
{
    private FillValue actionPoint = new FillValue();

    private int temporaryBonusCurrent;
    private int temporaryBonusMax;
    private bool isConfigured;

    public sealed override Type RegistrationType => typeof(ActionPointModule);

    public int Max => actionPoint.Max + temporaryBonusMax;
    public int Current => actionPoint.Current + temporaryBonusCurrent;

    public bool IsEmpty => Current <= actionPoint.Min;
    public bool IsConfigured => isConfigured;

    public event Action<int, int> OnActionPointChanged;

    public void Initialize(int maxActionPoint)
    {
        maxActionPoint = Mathf.Max(0, maxActionPoint);

        temporaryBonusCurrent = 0;
        temporaryBonusMax = 0;

        actionPoint.SetMax(maxActionPoint);
        actionPoint.SetCurrent(maxActionPoint);
        isConfigured = true;

        NotifyChanged();
    }

    public void PrepareTurn(int baseValue, int level)
    {
        baseValue = Mathf.Max(0, baseValue);
        level = Mathf.Max(1, level);

        temporaryBonusCurrent = 0;
        temporaryBonusMax = 0;

        int maximum = Mathf.Max(1, baseValue + GetLevelDiceMaximum(level));

        if (!isConfigured || actionPoint.Max != maximum)
        {
            actionPoint.SetMax(maximum);
            actionPoint.SetCurrent(maximum);
            isConfigured = true;
            NotifyChanged();
            return;
        }

        Restore(baseValue + RollLevelActionPoint(level));
    }

    public bool CanUse(int amount = 1)
    {
        if (amount <= 0)
            return true;

        return Current >= amount;
    }

    public bool TryUse(int amount = 1)
    {
        if (amount <= 0)
            return true;

        if (!CanUse(amount))
            return false;

        int remainingAmount = amount;

        int bonusUseAmount = Mathf.Min(temporaryBonusCurrent, remainingAmount);

        temporaryBonusCurrent -= bonusUseAmount;

        remainingAmount -= bonusUseAmount;

        if (remainingAmount > 0)
        {
            actionPoint.DecreaseCurrent(remainingAmount);
        }

        NotifyChanged();

        return true;
    }

    public void Restore(int amount)
    {
        if (amount <= 0)
            return;

        actionPoint.IncreaseCurrent(amount);

        NotifyChanged();
    }

    public void RestoreAll()
    {
        temporaryBonusCurrent = 0;
        temporaryBonusMax = 0;


        actionPoint.SetCurrent(actionPoint.Max);

        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnActionPointChanged?.Invoke(Current, Max);
    }

    private int RollLevelActionPoint(int level)
    {
        if (level >= 10)
        {
            return Dice.RollD4() + Dice.RollD4();
        }

        if (level >= 5)
        {
            return Dice.RollD6();
        }

        return Dice.RollD4();
    }

    private int GetLevelDiceMaximum(int level)
    {
        if (level >= 10)
            return 8;

        if (level >= 5)
            return 6;

        return 4;
    }

    public void AddTemporaryActionPoint(int amount)
    {
        if (amount <= 0)
            return;

        temporaryBonusCurrent += amount;
        temporaryBonusMax += amount;

        NotifyChanged();
    }


}
