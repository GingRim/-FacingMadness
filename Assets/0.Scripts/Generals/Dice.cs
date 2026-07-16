using UnityEngine;

public static class Dice
{
    public static int RollD10()
    {
        return Random.Range(1, 11);
    }

    public static int RollD8()
    {
        return Random.Range(1, 9);
    }

    public static int RollD6()
    {
        return Random.Range(1, 7);
    }

    public static int RollD4()
    {
        return Random.Range(1, 5);
    }

    public struct DiceResult
    {
        public int diceValue;
        public int bonus;
        public int total;
        public CriticalType criticalType;
    }

    public static DiceResult RollD10WithCritical(int bonus, int level)
    {
        int dice = RollD10();
        int total = dice + bonus;

        bool conditionDice10 = dice == 10;
        bool conditionTotal = total >= 18 - level;

        CriticalType critical = CriticalType.None;

        if (conditionDice10 && conditionTotal)
            critical = CriticalType.GreatCritical;
        else if (conditionDice10 || conditionTotal)
            critical = CriticalType.Critical;

        return new DiceResult
        {
            diceValue = dice,
            bonus = bonus,
            total = total,
            criticalType = critical
        };
    }
}
