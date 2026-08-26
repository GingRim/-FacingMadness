using TMPro;
using UnityEngine;


/// <summary>
/// 일반 필드 카드를 놓는 드롭 영역이며,
/// 최근 필드 카드 판정 결과를 표시합니다.
/// </summary>
public class UI_FieldCardUseDropTarget : MonoBehaviour
{
    [Header("결과 표시")]
    [SerializeField]
    private TextMeshProUGUI resultText;

    [SerializeField, TextArea(2, 4)]
    private string idleMessage = "사용할 필드 카드를 놓으세요.";

    /// <summary>
    /// 시작할 때 기본 안내 문구를 표시합니다.
    /// </summary>
    private void Awake()
    {
        ResetDisplay();
    }

    /// <summary>
    /// 필드 카드 판정 계산식과 결과를 표시합니다.
    /// </summary>
    public void ShowResult(FieldCardCheckData checkData)
    {
        if (resultText == null)
            return;

        string resultName = GetResultName(checkData.Result);

        resultText.SetText(
            $"D10 {checkData.Dice} " +
            $"+ 능력 보정 {checkData.AbilityModifier} " +
            $"+ 상태 보정 {checkData.StatusModifier}\n" +
            $"= {checkData.JudgmentValue} / " +
            $"목표 {checkData.Target}\n" +
            $"{resultName}"
        );
    }

    /// <summary>
    /// 필드 카드를 사용할 수 없을 때
    /// 실패 원인을 표시합니다.
    /// </summary>
    public void ShowMessage(string message)
    {
        if (resultText == null)
            return;

        resultText.SetText(message);
    }

    /// <summary>
    /// 카드 사용 영역을 기본 안내 상태로 되돌립니다.
    /// </summary>
    public void ResetDisplay()
    {
        if (resultText == null)
            return;

        resultText.SetText(idleMessage);
    }

    /// <summary>
    /// 필드 판정 결과를 표시용 문자열로 변환합니다.
    /// </summary>
    private string GetResultName(FieldCardCheckResult result)
    {
        switch (result)
        {
            case FieldCardCheckResult.Success:
                return "성공";

            case FieldCardCheckResult.Failure:
                return "실패";

            case FieldCardCheckResult.Fumble:
                return "펌블";

            default:
                return "판정 불가";
        }
    }
}
