using System.Collections.Generic;
using UnityEngine;

public class UI_Hand : MonoBehaviour
{
    [SerializeField] Transform cardParent;

    readonly List<UI_Card> cardUIs = new();

    /// <summary>
    /// 손패 UI에 카드 1장 추가
    /// </summary>
    public void AddCard(CardData cardData)
    {
        if (cardData == null)
            return;

        Debug.Log($"Hand UI 오브젝트: {gameObject.name}");
        Debug.Log($"Card Parent: {cardParent.name}");
        Debug.Log($"Card Parent 경로: {cardParent.name}");

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
