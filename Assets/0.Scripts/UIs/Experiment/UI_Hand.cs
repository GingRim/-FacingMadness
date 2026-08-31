using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_Hand : MonoBehaviour
{
    [SerializeField]
    private Transform cardParent;

    /// <summary>
    /// 실제 카드 인스턴스 선택 이벤트입니다.
    /// 새로운 카드 시스템에서는 이 이벤트를 사용합니다.
    /// </summary>
    public event Action<CardInstance> OnCardInstanceSelected;

    private readonly List<UI_Card> cardUIs = new();

    /// <summary>
    /// DeckModule의 실제 손패 인스턴스를 기준으로
    /// 손패 UI를 다시 생성합니다.
    /// </summary>
    public void RefreshFromDeck(DeckModule deck)
    {
        ClearHand();

        if (deck == null)
            return;

        foreach (CardInstance cardInstance in deck.HandInstances)
        {
            AddCard(cardInstance);
        }
    }

    /// <summary>
    /// 손패 UI에 실제 카드 인스턴스 한 장을 추가합니다.
    /// </summary>
    public void AddCard(CardInstance cardInstance)
    {
        if (cardInstance == null || cardInstance.Data == null)
        {
            return;
        }

        if (cardParent == null)
        {
            Debug.LogWarning("UI_Hand: cardParent가 없습니다.");

            return;
        }

        GameObject cardObject = ObjectManager.CreateObject("UI_Card", cardParent);

        if (cardObject == null)
            return;

        cardObject.transform.SetParent(cardParent, false);

        UI_Card uiCard = cardObject.GetComponent<UI_Card>();

        if (uiCard == null)
        {
            Debug.LogWarning(
                "생성된 UI_Card 오브젝트에 " +
                "UI_Card 컴포넌트가 없습니다.");

            PooledObject pooled = cardObject.GetComponent<PooledObject>();

            if (pooled != null)
            {
                pooled.OnEnqueue();
            }
            else
            {
                Destroy(cardObject);
            }

            return;
        }

        CardClick cardClick = cardObject.GetComponent<CardClick>();

        if (cardClick == null)
        {
            Debug.LogWarning(
                "생성된 UI_Card 오브젝트에 " +
                "CardClick 컴포넌트가 없습니다.");

            uiCard.OnEnqueue();
            return;
        }

        cardClick.ClearClickListeners();
        uiCard.SetCard(cardInstance);

        cardClick.OnClicked += HandleCardClicked;

        cardUIs.Add(uiCard);
    }

    /// <summary>
    /// 현재 생성된 손패 UI를 모두 풀로 반환합니다.
    /// </summary>
    public void ClearHand()
    {
        foreach (UI_Card card in cardUIs)
        {
            if (card == null)
                continue;

            CardClick cardClick =
                card.GetComponent<CardClick>();

            if (cardClick != null)
            {
                cardClick.OnClicked -= HandleCardClicked;
                cardClick.ClearClickListeners();
            }

            card.OnEnqueue();
        }

        cardUIs.Clear();
    }

    private void HandleCardClicked(CardInstance clickedCard)
    {
        if (clickedCard == null || clickedCard.Data == null)
            return;

        OnCardInstanceSelected?.Invoke(clickedCard);
    }

    private void OnDestroy()
    {
        ClearHand();

        OnCardInstanceSelected = null;
    }
}
