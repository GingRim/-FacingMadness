using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UI_Card : OpenableUIBase
{
    [Header("불러올 카드 데이터 이름")]
    [SerializeField] private string cardDataName;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    [SerializeField] private Image illustrationImage;

    private CardData cardData;

    public CardData CardData => cardData;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);

        LoadCard(cardDataName);
    }

    public void LoadCard(string dataName)
    {
        cardData = DataManager.LoadDataFile<CardData>(dataName);

        if (cardData == null)
            return;

        Refresh();
    }

    public void SetCard(CardData newCard)
    {
        cardData = newCard;
        Refresh();
    }

    public void Refresh()
    {
        if (cardData == null)
            return;

        cardNameText.SetText(cardData.cardName);
        descriptionText.SetText(cardData.description);

        if (illustrationImage != null)
            illustrationImage.sprite = cardData.illustration;

        costText.SetText(GetCostText());
    }

    private string GetCostText()
    {
        if (cardData.costs == null || cardData.costs.Length == 0)
            return "0";

        string result = "";

        foreach (CardCostData cost in cardData.costs)
        {
            result += $"{cost.costType}:{cost.amount} ";
        }

        return result;
    }
}
