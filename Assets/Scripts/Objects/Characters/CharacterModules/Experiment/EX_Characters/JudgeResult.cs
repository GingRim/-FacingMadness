using UnityEngine;

public struct JudgeResult
{
    public int dice;
    public int statModifier;
    public int statusModifier;
    public int total;
    public int target;
    public bool success;

    public JudgeResult(int dice, int statModifier, int statusModifier, int target)
    {
        this.dice = dice;
        this.statModifier = statModifier;
        this.statusModifier = statusModifier;
        this.total = dice + statModifier + statusModifier;
        this.target = target;
        this.success = this.total >= target;
    }
}