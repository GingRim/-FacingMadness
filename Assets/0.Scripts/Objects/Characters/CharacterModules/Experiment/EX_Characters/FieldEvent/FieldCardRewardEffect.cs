using UnityEngine;

/// <summary>
/// 필드 이벤트 결과로 지정된 카드를 플레이어의 덱에 추가한다.
/// </summary>
[CreateAssetMenu(fileName = "NewFieldCardRewardEffect", menuName = "Field/Event Effect/Card Reward")]
public class FieldCardRewardEffect : FieldEventEffect
{
    [Header("지급 카드")]
    [SerializeField]
    private CardData rewardCard;

    [Header("지급 수량")]
    [SerializeField, Min(1)]
    private int amount = 1;

    /// <summary>
    /// 지정된 카드를 이벤트를 진행한 플레이어의 덱에 추가한다.
    /// 카드를 모두 추가한 뒤 덱을 한 번 셔플한다.
    /// </summary>
    /// <param name="context">현재 필드 이벤트 실행 정보.</param>
    public override void Execute(FieldEventContext context)
    {
        if (context == null)
        {
            Debug.LogWarning(
                "카드 보상 지급 실패: FieldEventContext가 없습니다.");

            return;
        }

        if (rewardCard == null)
        {
            Debug.LogWarning(
                "카드 보상 지급 실패: 지급할 카드가 설정되지 않았습니다.");

            return;
        }

        CharacterBase receiver = context.Player;

        if (receiver == null &&
            context.FieldManager != null)
        {
            receiver = context.FieldManager.CurrentPlayer;
        }

        if (receiver == null)
        {
            Debug.LogWarning("카드 보상 지급 실패: 보상을 받을 플레이어가 없습니다.");

            return;
        }

        DeckModule deck = receiver.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning($"{receiver.name}: DeckModule이 없어 카드 보상을 지급할 수 없습니다.");

            return;
        }

        int rewardAmount = Mathf.Max(1, amount);

        // 마지막 카드를 넣을 때 한 번만 셔플한다.
        for (int i = 0; i < rewardAmount - 1; i++)
        {
            deck.AddCardToDeck(rewardCard);
        }

        deck.AddCardToDeckAndShuffle(rewardCard);

        AppendResultText(context, rewardAmount == 1 ? $"카드 획득: {rewardCard.cardName}" : $"카드 획득: {rewardCard.cardName} × {rewardAmount}");

        Debug.Log(
            $"필드 카드 보상: {receiver.name}이(가) " +
            $"{rewardCard.cardName} 카드를 {rewardAmount}장 획득했습니다.");
    }

    /// <summary>
    /// 기존 이벤트 결과를 지우지 않고 카드 획득 결과를 추가한다.
    /// </summary>
    /// <param name="context">현재 필드 이벤트 실행 정보.</param>
    /// <param name="message">추가할 결과 문구.</param>
    private void AppendResultText(FieldEventContext context, string message)
    {
        if (string.IsNullOrWhiteSpace(context.ResultTextOverride))
        {
            context.SetResultText(message);
            return;
        }

        context.SetResultText($"{context.ResultTextOverride}\n{message}");
    }
}