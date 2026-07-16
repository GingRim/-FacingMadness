using System.Collections.Generic;
using UnityEngine;

public class UI_Hand : MonoBehaviour
{
    [SerializeField] Transform cardParent;

    readonly List<UI_Card> cardUIs = new();

    /// <summary>
    /// DeckModule의 현재 Hand 데이터를 기준으로 손패 UI를 다시 그림.
    /// </summary>
    public void RefreshFromDeck(DeckModule deck)
    {
        if (deck == null)
            return;

        ClearHand();

        foreach (CardData card in deck.Hand)
        {
            AddCard(card);
        }
    }

    /// <summary>
    /// 손패 UI에 카드 1장 추가
    /// </summary>
    public void AddCard(CardData cardData)
    {
        if (cardData == null)
            return;

        if (cardParent == null)
            return;

        GameObject cardObject =
            ObjectManager.CreateObject("UI_Card", cardParent);

        if (cardObject == null)
            return;

        cardObject.transform.SetParent(cardParent, false);

        UI_Card uiCard = cardObject.GetComponent<UI_Card>();

        if (uiCard == null)
            return;

        uiCard.SetCard(cardData);
        cardUIs.Add(uiCard);
    }


    /// <summary>
    /// 손패 UI 전체 초기화
    /// </summary>
    public void ClearHand()
    {
        foreach (UI_Card card in cardUIs)
        {
            if (card == null)
                continue;

            PooledObject pooled = card.GetComponent<PooledObject>();

            if (pooled != null)
                pooled.OnEnqueue();
            else
                Destroy(card.gameObject);
        }

        cardUIs.Clear();
    }

}
