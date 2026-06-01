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
}
