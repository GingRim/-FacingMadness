using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFieldSessionEnding",
    menuName = "Facing Madness/Field/Session Ending")]
public class FieldSessionEndingData : ScriptableObject
{
    [Header("엔딩 식별")]
    [SerializeField]
    private string endingId;

    [SerializeField]
    private string endingRank;

    [SerializeField]
    private string endingTitle;

    [SerializeField, TextArea(3, 10)]
    private string description;

    [Header("점수 범위 (정상 종료 전용)")]
    [SerializeField]
    private int minimumScore = int.MinValue;

    [SerializeField]
    private int maximumScore = int.MaxValue;

    public string EndingId => endingId;
    public string EndingRank => endingRank;
    public string EndingTitle => endingTitle;
    public string Description => description;
    public int MinimumScore => minimumScore;
    public int MaximumScore => maximumScore;

    public bool ContainsScore(int score)
    {
        return score >= minimumScore && score <= maximumScore;
    }

    private void OnValidate()
    {
        if (minimumScore > maximumScore)
        {
            maximumScore = minimumScore;
        }
    }
}
