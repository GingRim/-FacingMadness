using System;
using UnityEngine;

[Serializable]
public class FieldEffectValue
{
    [SerializeField]
    private int fixedValue;

    [SerializeField, Min(0)]
    private int diceCount;

    [SerializeField]
    private FieldEffectDiceType diceType;

    public int Roll()
    {
        int result = fixedValue;

        for (int i = 0; i < diceCount; i++)
        {
            result += RollDice();
        }

        return Mathf.Max(0, result);
    }

    private int RollDice()
    {
        switch (diceType)
        {
            case FieldEffectDiceType.D4:
                return Dice.RollD4();

            case FieldEffectDiceType.D6:
                return Dice.RollD6();

            case FieldEffectDiceType.D8:
                return Dice.RollD8();

            case FieldEffectDiceType.D10:
                return Dice.RollD10();

            default:
                return 0;
        }
    }
}
