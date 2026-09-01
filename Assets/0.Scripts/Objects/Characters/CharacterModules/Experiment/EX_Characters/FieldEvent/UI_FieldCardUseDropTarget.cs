using TMPro;
using UnityEngine;

/// <summary>
/// 일반 필드 카드를 사용하는 드롭 위치이며,
/// 최근 필드 카드 판정 결과를 표시합니다.
/// </summary>
public class UI_FieldCardUseDropTarget : CardDropReceiver
{
    [Header("필드 카드 전달")]
    [SerializeField]
    private FieldCardUseController
        cardUseController;

    [Header("결과 표시")]
    [SerializeField]
    private TextMeshProUGUI resultText;

    [SerializeField, TextArea(2, 4)]
    private string idleMessage =
        "사용할 필드 카드를 놓으세요.";

    private void Awake()
    {
        ResetDisplay();
    }

    public override bool TryReceiveCard(CardInstance card)
    {
        if (card == null || card.Data == null)
            return false;

        if (cardUseController == null)
        {
            cardUseController = GetComponentInParent<FieldCardUseController>(true);
        }

        if (cardUseController == null)
        {
            Debug.LogWarning("필드 카드 처리기가 연결되지 않았습니다.");

            return false;
        }

        return
            cardUseController.TryUseDroppedCard(card);
    }

    /// <summary>
    /// 필드 카드 판정 계산식과 결과를 표시합니다.
    /// </summary>
    public void ShowResult(FieldCardCheckData checkData)
    {
        if (resultText == null)
        {
            return;
        }

        string resultName = GetResultName(checkData.Result);

        resultText.SetText(
            $"D10 {checkData.Dice} " +
            $"+ 능력 보정 {checkData.AbilityModifier} " +
            $"+ 상태 보정 {checkData.StatusModifier}\n" +
            $"= {checkData.JudgmentValue} / " +
            $"목표 {checkData.Target}\n" +
            $"{resultName}");
    }

    public void ShowMessage(string message)
    {
        if (resultText != null)
        {
            resultText.SetText(message);
        }
    }

    public void ResetDisplay()
    {
        if (resultText != null)
        {
            resultText.SetText(idleMessage);
        }
    }

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
