using UnityEngine;



/// <summary>
/// 캐릭터의 능력치 보정치와 상태 보정치를 이용하여
/// 범용 판정을 실행합니다.
/// </summary>
public static class JudgeUtility
{
    /// <summary>
    /// 1D10 + 능력치 보정치 + 상태 보정치로
    /// 지정된 목표치에 대한 판정을 실행합니다.
    /// </summary>
    public static JudgeResult Roll(CharacterBase user, StatType statType, int target)
    {
        if (user == null || statType == StatType.None)
        {
            Debug.LogWarning("판정 실패: 사용자 또는 요구 능력치가 없습니다.");

            return new JudgeResult(0, 0, 0, target, false);
        }

        StatModules stat = user.GetModule<StatModules>();

        if (stat == null)
        {
            Debug.LogWarning(
                $"{user.name}: StatModules가 없습니다.");

            return new JudgeResult(0, 0, 0, target, false);
        }

        // 기존에 만들어 둔 능력치 보정치 사용
        int statModifier = stat.GetModifier(statType);

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

        Debug.Log(
            $"판정: D10 {result.dice} " +
            $"+ {statType} 보정 {result.statModifier} " +
            $"+ 상태 보정 {result.statusModifier} " +
            $"= {result.total} / " +
            $"목표 {result.target} / " +
            $"결과:{GetResultName(result)}"
        );

        return result;
    }

    /// <summary>
    /// 판정 결과를 표시용 문자열로 반환합니다.
    /// </summary>
    private static string GetResultName(JudgeResult result)
    {
        if (!result.valid)
            return "판정 불가";

        if (result.fumble)
            return "펌블";

        return result.success ? "성공" : "실패";
    }
}