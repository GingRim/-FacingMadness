using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Card : PooledObject
{

    [SerializeField] TextMeshProUGUI NameText;

    [SerializeField] TextMeshProUGUI descriptionText;
    
    [SerializeField] Image frameImage;

    CardData cardData;

    public CardData CardData => cardData;

    /// <summary>
    /// 카드 데이터를 받아 UI에 표시
    /// </summary>
    public void SetCard(CardData newCard)
    {
        cardData = newCard;
        Refresh();
    }

    void Refresh()
    {
        if (cardData == null) return;

        NameText.SetText(CardData.cardName);

        descriptionText.SetText(cardData.description);
       
        if (frameImage != null)
            frameImage.color = GetCardColor(cardData.color);
    }


    Color GetCardColor(CardColorType type)
    {
        switch (type)
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

            case CardColorType.Colorless:
                return Color.gray;

            default:
                return Color.white;
        }
    }



}

