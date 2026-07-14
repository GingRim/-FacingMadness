using UnityEngine;

public class Motivation : StatusEffectHandler
{
    public override StatusEffectType Type => StatusEffectType.Motivation;
    public override int MaxStack => 5;

    [SerializeField] private int stack;

    private const int BonusPerStack = 2;

    public int Stack => stack;
    public bool HasMotivation => stack > 0;

    public void AddMotivation(int value)
    {
        if (value <= 0)
            return;

        stack = Mathf.Clamp(stack + value, 0, MaxStack);

        Debug.Log($"의욕 {value} 획득 / 현재 의욕: {stack}");
    }

    public int GetJudgeBonus()
    {
        if (stack <= 0)
            return 0;

        return stack * BonusPerStack;
    }

    public void ConsumeOnJudge()
    {
        if (stack <= 0)
            return;

        stack--;

        Debug.Log($"의욕 발동: 1 감소 / 현재 의욕: {stack}");
    }

    public void ClearMotivation()
    {
        if (stack <= 0)
            return;

        stack = 0;

        Debug.Log("의욕 해제");
    }
}
