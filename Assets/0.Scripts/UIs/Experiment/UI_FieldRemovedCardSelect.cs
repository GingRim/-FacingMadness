using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_FieldRemovedCardSelect : MonoBehaviour
{
    [Header("화면")]
    [SerializeField]
    private GameObject panel;

    [Header("카드 선택")]
    [SerializeField]
    private Transform core;

    [SerializeField]
    private UI_FieldRemovedCardButton buttonTemplate;

    private readonly List<UI_FieldRemovedCardButton> buttonPool = new();
    private readonly List<CardInstance> displayedCards = new();

    private Action<CardInstance> onCardSelected;
    private bool isSelecting;

    public bool IsSelecting => isSelecting;

    private void Awake()
    {
        if (buttonTemplate != null)
        {
            buttonTemplate.gameObject.SetActive(false);
        }

        Close();
    }

    /// <summary>
    /// 복귀 가능한 제거 카드 목록을 보여준다.
    /// 표시할 카드가 없으면 false를 반환한다.
    /// </summary>
    public bool Open(IReadOnlyList<CardInstance> cards, Action<CardInstance> selectedCallback)
    {
        CollectValidCards(cards);

        if (displayedCards.Count == 0)
        {
            Close();
            return false;
        }

        onCardSelected = selectedCallback;
        isSelecting = true;

        EnsurePoolSize(displayedCards.Count);

        if (buttonPool.Count < displayedCards.Count)
        {
            Debug.LogWarning("UI_FieldRemovedCardSelect: 카드 버튼을 생성하지 못했습니다.");

            Close();
            return false;
        }

        ClearButtons();

        for (int i = 0; i < displayedCards.Count; i++)
        {
            buttonPool[i].SetCard(displayedCards[i], HandleCardSelected);
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        return true;
    }

    public void Close()
    {
        isSelecting = false;
        onCardSelected = null;

        ClearButtons();
        displayedCards.Clear();

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void CollectValidCards(IReadOnlyList<CardInstance> cards)
    {
        displayedCards.Clear();

        if (cards == null)
            return;

        foreach (CardInstance card in cards)
        {
            if (card == null || card.Data == null)
                continue;

            // 무색 카드는 복귀 선택 대상이 아님
            if (card.Data.color == CardColorType.Colorless)
                continue;

            displayedCards.Add(card);
        }
    }

    private void EnsurePoolSize(int requiredCount)
    {
        if (core == null || buttonTemplate == null)
        {
            Debug.LogWarning("UI_FieldRemovedCardSelect: Core 또는 Button Template이 없습니다.");

            return;
        }

        while (buttonPool.Count < requiredCount)
        {
            UI_FieldRemovedCardButton newButton = Instantiate(buttonTemplate, core);

            newButton.name = $"RemovedCardButton_{buttonPool.Count}";

            newButton.Clear();

            buttonPool.Add(newButton);
        }
    }

    private void ClearButtons()
    {
        foreach (UI_FieldRemovedCardButton button in buttonPool)
        {
            if (button == null)
                continue;

            button.Clear();
        }
    }

    private void HandleCardSelected(CardInstance selectedCard)
    {
        if (!isSelecting || selectedCard == null)
            return;

        // 중복 클릭 방지를 위해 먼저 선택 상태 종료
        Action<CardInstance> callback = onCardSelected;

        isSelecting = false;
        onCardSelected = null;

        ClearButtons();
        displayedCards.Clear();

        if (panel != null)
        {
            panel.SetActive(false);
        }

        callback?.Invoke(selectedCard);
    }
}
