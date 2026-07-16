using UnityEngine;

public static class JudgeUtility
{
    public static JudgeResult Roll(CharacterBase user, StatType statType, int target)
    {
        if (user == null)
        {
            return new JudgeResult(0, 0, 0, target);
        }

        StatModules stat = user.GetModule<StatModules>();

        int statModifier = 0;

        if (stat != null)
        {
            int statValue = stat.GetStat(statType);
            statModifier = statValue / 2;
        }

        StatusEffectModule status = user.GetModule<StatusEffectModule>();

        int dice = Dice.RollD10();
        int statusModifier = 0;

        if (status != null)
        {
            dice = status.RollJudgeDice();
            statusModifier = status.GetJudgeBonus();
            status.ConsumeJudgeStatus();
        }

        JudgeResult result = new JudgeResult(dice, statModifier, statusModifier, target);

        Debug.Log($"판정: D10 {dice} + 능력치 보정 {statModifier} + 상태 보정 {statusModifier} = {result.total} / 목표 {target} / 성공 {result.success}");

        return result;
    }
}
