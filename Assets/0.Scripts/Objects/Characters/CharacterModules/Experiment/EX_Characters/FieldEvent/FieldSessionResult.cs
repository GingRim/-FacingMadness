public sealed class FieldSessionResult
{
    public FieldMissionData Mission { get; }
    public FieldSessionEndingData Ending { get; }
    public int Score { get; }
    public bool IsDeath { get; }

    public FieldSessionResult(
        FieldMissionData mission,
        FieldSessionEndingData ending,
        int score,
        bool isDeath)
    {
        Mission = mission;
        Ending = ending;
        Score = score;
        IsDeath = isDeath;
    }
}
