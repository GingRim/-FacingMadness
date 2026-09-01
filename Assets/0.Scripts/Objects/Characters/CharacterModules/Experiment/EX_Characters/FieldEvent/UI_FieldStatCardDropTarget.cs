using UnityEngine;

/// <summary>
/// 이벤트 능력치 판정을 대응 카드로 확정 성공시키는 드롭 위치입니다.
/// </summary>
public class UI_FieldStatCardDropTarget : CardDropReceiver
{
    [SerializeField]
    private UI_FieldCardSelector cardSelector;

    public override bool TryReceiveCard(CardInstance card)
    {
        if (card == null || card.Data == null)
            return false;

        if (cardSelector == null)
        {
            cardSelector = GetComponentInParent<UI_FieldCardSelector>(true);
        }

        if (cardSelector == null)
        {
            Debug.LogWarning(
                "판정 카드 선택 UI가 연결되지 않았습니다.");

            return false;
        }

        return cardSelector.TrySelectCard(card);
    }
}