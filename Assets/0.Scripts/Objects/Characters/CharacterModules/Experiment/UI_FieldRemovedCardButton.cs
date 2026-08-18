using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_FieldRemovedCardButton : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField]
    private Button button;

    [Header("카드 표시")]
    [SerializeField]
    private TextMeshProUGUI cardNameText;

    [SerializeField]
    private Image frameImage;

    private CardData card;
    private Action<CardData> onSelected;

    public CardData Card => card;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
            button.onClick.AddListener(HandleClicked);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    public void SetCard(CardData newCard, Action<CardData> selectedCallback)
    {
        card = newCard;
        onSelected = selectedCallback;

        if (cardNameText != null)
        {
            cardNameText.SetText(card != null ? card.cardName : string.Empty);
        }

        if (frameImage != null)
        {
            frameImage.color = card != null ? GetCardColor(card.color) : Color.white;
        }

        gameObject.SetActive(card != null);
    }

    public void Clear()
    {
        card = null;
        onSelected = null;

        if (cardNameText != null)
        {
            cardNameText.SetText(string.Empty);
        }

        gameObject.SetActive(false);
    }

    private void HandleClicked()
    {
        if (card == null)
            return;

        onSelected?.Invoke(card);
    }

    private Color GetCardColor(CardColorType color)
    {
        switch (color)
        {
            case CardColorType.Red:
                return Color.red;

            case CardColorType.Yellow:
                return Color.yellow;

            case CardColorType.Green:
                return Color.green;

            case CardColorType.Blue:
                return Color.blue;

            case CardColorType.Purple:
                return new Color(0.6f, 0.2f, 1f);

            case CardColorType.Black:
                return Color.black;

            default:
                return Color.gray;
        }
    }
}
