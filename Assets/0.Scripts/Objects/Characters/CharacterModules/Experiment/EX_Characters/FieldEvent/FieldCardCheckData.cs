
    public struct FieldCardCheckData
    {
        public CardData Card { get; }
        public StatType StatType { get; }

        public int Dice { get; }
        public int StatValue { get; }

        public int AbilityModifier { get; }
        public int StatusModifier { get; }
        public int JudgmentValue { get; }

        public FieldCardCheckResult Result { get; }

        public bool IsSuccess => Result == FieldCardCheckResult.Success;

        public bool IsFailure => Result == FieldCardCheckResult.Failure;

        public bool IsFumble => Result == FieldCardCheckResult.Fumble;

        public FieldCardCheckData(CardData card, StatType statType, int dice, int statValue, int abilityModifier, int statusModifier, int judgmentValue, FieldCardCheckResult result)
        {
            Card = card;
            StatType = statType;

            Dice = dice;
            StatValue = statValue;

            AbilityModifier = abilityModifier;
            StatusModifier = statusModifier;
            JudgmentValue = judgmentValue;

            Result = result;
        }
    }
