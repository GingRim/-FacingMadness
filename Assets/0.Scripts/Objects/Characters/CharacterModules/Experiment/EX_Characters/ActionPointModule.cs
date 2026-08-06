using System;
using UnityEngine;

public class ActionPointModule : CharacterModule
{
    private FillValue actionPoint = new FillValue();

    public sealed override Type RegistrationType => typeof(ActionPointModule);

    public int Max => actionPoint.Max;
    public int Current => actionPoint.Current;

    public bool IsEmpty => actionPoint.Current <= actionPoint.Min;

    public event Action<int, int> OnActionPointChanged;

    public void Initialize(int maxActionPoint)
    {
        maxActionPoint = Mathf.Max(0, maxActionPoint);

        actionPoint.SetMax(maxActionPoint);
        actionPoint.SetCurrent(maxActionPoint);

        NotifyChanged();
    }

    public bool CanUse(int amount = 1)
    {
        if (amount <= 0)
            return true;

        return actionPoint.Current >= amount;
    }

    public bool TryUse(int amount = 1)
    {
        if (amount <= 0)
            return true;

        if (!CanUse(amount))
            return false;

        actionPoint.DecreaseCurrent(amount);

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
        actionPoint.SetCurrent(actionPoint.Max);

        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnActionPointChanged?.Invoke(actionPoint.Current, actionPoint.Max);
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

}
