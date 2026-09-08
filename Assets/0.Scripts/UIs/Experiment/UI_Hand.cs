using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Hand : MonoBehaviour
{
    [SerializeField]
    private Transform cardParent;

    [Header("손패 배치")]
    [SerializeField, Range(0.1f, 1f)]
    private float preferredCardStepRatio = 0.8f;

    [SerializeField, Min(0f)]
    private float horizontalPadding;

    [SerializeField, Min(0f)]
    private float bottomPadding;

    [Header("마우스 강조")]
    [SerializeField, Min(0f)]
    private float hoverLift = 70f;

    [SerializeField, Min(1f)]
    private float hoverScale = 1.08f;

    /// <summary>
    /// 실제 카드 인스턴스 선택 이벤트입니다.
    /// 새로운 카드 시스템에서는 이 이벤트를 사용합니다.
    /// </summary>
    public event Action<CardInstance> OnCardInstanceSelected;

    private readonly List<UI_Card> cardUIs = new();

    private UI_Card hoveredCard;
    private RectTransform cardParentRect;
    private Vector2 previousParentSize;
    private bool isLayoutDirty = true;
    private bool isApplyingLayout;

    private void Awake()
    {
        PrepareHandLayout();
    }

    private void OnEnable()
    {
        PrepareHandLayout();
        MarkLayoutDirty();
    }

    private void LateUpdate()
    {
        if (cardParentRect == null)
        {
            PrepareHandLayout();
        }

        if (cardParentRect == null)
            return;

        Vector2 currentParentSize = cardParentRect.rect.size;

        if (currentParentSize != previousParentSize)
        {
            previousParentSize = currentParentSize;
            isLayoutDirty = true;
        }

        if (isLayoutDirty)
        {
            RefreshCardLayout();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        MarkLayoutDirty();
    }

    private void OnTransformChildrenChanged()
    {
        if (!isApplyingLayout)
        {
            MarkLayoutDirty();
        }
    }

    /// <summary>
    /// 기존 GridLayoutGroup이 10번째 카드를 다음 줄로 내리지 않도록
    /// 손패 영역의 자동 레이아웃을 비활성화합니다.
    /// </summary>
    private void PrepareHandLayout()
    {
        if (cardParent == null)
            return;

        cardParentRect = cardParent as RectTransform;

        LayoutGroup[] layoutGroups =
            cardParent.GetComponents<LayoutGroup>();

        foreach (LayoutGroup layoutGroup in layoutGroups)
        {
            if (layoutGroup != null)
            {
                layoutGroup.enabled = false;
            }
        }

        if (cardParentRect != null)
        {
            previousParentSize = cardParentRect.rect.size;
        }
    }

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

        MarkLayoutDirty();
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

        PrepareHandLayout();

        GameObject cardObject =
            ObjectManager.CreateObject("UI_Card", cardParent);

        if (cardObject == null)
            return;

        cardObject.transform.SetParent(cardParent, false);

        UI_Card uiCard = cardObject.GetComponent<UI_Card>();

        if (uiCard == null)
        {
            Debug.LogWarning(
                "생성된 UI_Card 오브젝트에 " +
                "UI_Card 컴포넌트가 없습니다.");

            PooledObject pooled =
                cardObject.GetComponent<PooledObject>();

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

        UI_HandCardHover cardHover =
            cardObject.GetComponent<UI_HandCardHover>();

        if (cardHover == null)
        {
            cardHover =
                cardObject.AddComponent<UI_HandCardHover>();
        }

        cardHover.Bind(this, uiCard);

        cardClick.ClearClickListeners();
        uiCard.SetCard(cardInstance);

        cardClick.OnClicked += HandleCardClicked;

        cardUIs.Add(uiCard);

        MarkLayoutDirty();
    }

    /// <summary>
    /// 현재 생성된 손패 UI를 모두 풀로 반환합니다.
    /// </summary>
    public void ClearHand()
    {
        hoveredCard = null;

        foreach (UI_Card card in cardUIs)
        {
            if (card == null)
                continue;

            UI_HandCardHover cardHover =
                card.GetComponent<UI_HandCardHover>();

            if (cardHover != null)
            {
                cardHover.Unbind();
            }

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
        MarkLayoutDirty();
    }

    /// <summary>
    /// 손패 영역의 너비 안에서 모든 카드를 한 줄로 배치합니다.
    /// 카드가 많아지면 중심 간격을 줄여 서로 겹치게 합니다.
    /// </summary>
    public void RefreshCardLayout()
    {
        isLayoutDirty = false;

        if (cardParentRect == null)
            return;

        int visibleCardCount = GetVisibleCardCount();

        if (visibleCardCount <= 0)
            return;

        RectTransform firstCardRect = GetFirstVisibleCardRect();

        if (firstCardRect == null)
            return;

        float cardWidth = firstCardRect.rect.width;
        float cardHeight = firstCardRect.rect.height;
        float availableWidth = Mathf.Max(
            0f,
            cardParentRect.rect.width - horizontalPadding * 2f);

        float preferredStep =
            cardWidth * Mathf.Clamp01(preferredCardStepRatio);

        float fittedStep = visibleCardCount > 1
            ? Mathf.Max(
                0f,
                (availableWidth - cardWidth) /
                (visibleCardCount - 1))
            : 0f;

        float cardStep = visibleCardCount > 1
            ? Mathf.Min(preferredStep, fittedStep)
            : 0f;

        float contentWidth =
            cardWidth + cardStep * (visibleCardCount - 1);

        float firstCardX =
            -contentWidth * 0.5f +
            cardWidth * firstCardRect.pivot.x;

        float baseY =
            cardParentRect.rect.yMin +
            bottomPadding +
            cardHeight * firstCardRect.pivot.y -
            cardParentRect.rect.center.y;

        int layoutIndex = 0;
        isApplyingLayout = true;

        foreach (UI_Card card in cardUIs)
        {
            if (!IsCardInHand(card))
                continue;

            RectTransform cardRect =
                card.transform as RectTransform;

            if (cardRect == null)
                continue;

            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(
                firstCardX + cardStep * layoutIndex,
                baseY);
            cardRect.localScale = Vector3.one;
            cardRect.localRotation = Quaternion.identity;
            cardRect.SetSiblingIndex(layoutIndex);

            layoutIndex++;
        }

        if (IsCardInHand(hoveredCard))
        {
            RectTransform hoveredRect =
                hoveredCard.transform as RectTransform;

            if (hoveredRect != null)
            {
                hoveredRect.anchoredPosition +=
                    Vector2.up * hoverLift;
                hoveredRect.localScale =
                    Vector3.one * hoverScale;
                hoveredRect.SetAsLastSibling();
            }
        }

        isApplyingLayout = false;
    }

    internal void SetCardHovered(UI_Card card, bool isHovered)
    {
        if (isHovered)
        {
            if (!IsCardInHand(card))
                return;

            hoveredCard = card;
        }
        else if (hoveredCard == card)
        {
            hoveredCard = null;
        }

        MarkLayoutDirty();
    }

    private int GetVisibleCardCount()
    {
        int count = 0;

        foreach (UI_Card card in cardUIs)
        {
            if (IsCardInHand(card))
            {
                count++;
            }
        }

        return count;
    }

    private RectTransform GetFirstVisibleCardRect()
    {
        foreach (UI_Card card in cardUIs)
        {
            if (!IsCardInHand(card))
                continue;

            return card.transform as RectTransform;
        }

        return null;
    }

    private bool IsCardInHand(UI_Card card)
    {
        return card != null &&
               card.gameObject.activeInHierarchy &&
               card.transform.parent == cardParent;
    }

    private void MarkLayoutDirty()
    {
        isLayoutDirty = true;
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
