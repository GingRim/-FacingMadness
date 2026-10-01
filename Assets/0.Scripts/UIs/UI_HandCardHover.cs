using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 손패에 들어 있는 카드의 마우스 진입과 이탈만 UI_Hand에 전달합니다.
/// 풀에서 다른 화면으로 이동하면 Unbind되어 손패 강조가 작동하지 않습니다.
/// </summary>
public sealed class UI_HandCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI_Hand hand;
    private UI_Card card;

    public void Bind(UI_Hand newHand, UI_Card newCard)
    {
        Unbind();

        hand = newHand;
        card = newCard;
    }

    public void Unbind()
    {
        if (hand != null && card != null)
        {
            hand.SetCardHovered(card, false);
        }

        hand = null;
        card = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hand != null && card != null)
        {
            hand.SetCardHovered(card, true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hand != null && card != null)
        {
            hand.SetCardHovered(card, false);
        }
    }

    private void OnDisable()
    {
        if (hand != null && card != null)
        {
            hand.SetCardHovered(card, false);
        }
    }
}
