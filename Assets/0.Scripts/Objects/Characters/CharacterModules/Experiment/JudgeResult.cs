using UnityEngine;

/// <summary>
/// 범용 판정의 계산 결과를 저장한다.
/// </summary>
public struct JudgeResult
{
    public int dice;
    public int statModifier;
    public int statusModifier;

    public int total;
    public int target;

    public bool valid;
    public bool success;
    public bool fumble;

    /// <summary>
    /// 주사위와 모든 보정치를 합산하여 판정 결과를 계산합니다.
    /// </summary>
    public JudgeResult(int dice, int statModifier, int statusModifier, int target, bool isValid = true)
    {
        this.dice = dice;
        this.statModifier = statModifier;
        this.statusModifier = statusModifier;

        this.total = Mathf.Max(0, dice + statModifier + statusModifier);

        this.target = Mathf.Max(0, target);
        this.valid = isValid;

        // 모든 보정이 적용된 최종 판정값이 1 이하면 펌블
        this.fumble = isValid && this.total <= 1;

        this.success = isValid && !this.fumble && this.total >= this.target;
    }
}
