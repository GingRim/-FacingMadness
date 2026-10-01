

/// <summary>
/// 필드 카드 사용 과정에서 발생한 판정 정보를 보관합니다.
/// </summary>
public struct FieldCardCheckData
{
    public CardData Card { get; }
    public StatType StatType { get; }

    public int Dice { get; }
    public int StatValue { get; }

    public int AbilityModifier { get; }
    public int StatusModifier { get; }

    public int JudgmentValue { get; }
    public int Target { get; }

    public FieldCardCheckResult Result { get; }

    public bool IsSuccess => Result == FieldCardCheckResult.Success;

    public bool IsFailure => Result == FieldCardCheckResult.Failure;

    public bool IsFumble => Result == FieldCardCheckResult.Fumble;

    /// <summary>
    /// 필드 카드 판정에 사용된 모든 정보를 저장합니다.
    /// </summary>
    public FieldCardCheckData(CardData card, StatType statType, int dice, int statValue, int abilityModifier, int statusModifier, int judgmentValue, int target, FieldCardCheckResult result)
    {
        Card = card;
        StatType = statType;


        Dice = dice;
        StatValue = statValue;


        AbilityModifier = abilityModifier;
        StatusModifier = statusModifier;


        JudgmentValue = judgmentValue;
        Target = target;


        Result = result;
    }
}
