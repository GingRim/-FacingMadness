using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Card : PooledObject
{

    [SerializeField] TextMeshProUGUI NameText;

    [SerializeField] TextMeshProUGUI descriptionText;
    
    [SerializeField] Image frameImage;

    CardData cardData;
    CanvasGroup canvasGroup;

    public event Action<UI_Card> OnClicked;

    public CardData CardData => cardData;


    private void Awake()
    {
        EnsureCanvasGroup();
    }

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

        if (NameText != null)
            NameText.SetText(CardData.cardName);
        
        if (descriptionText != null)
            descriptionText.SetText(cardData.description);
       
        if (frameImage != null)
            frameImage.color = GetCardColor(cardData.color);
    }

    /// <summary>
    /// 드래그 중 카드 UI가 마우스 아래 오브젝트 감지를 막지 않게 설정.
    /// true = 카드가 Raycast를 막음.
    /// false = 카드 뒤의 오브젝트를 감지할 수 있음.
    /// </summary>
    public void SetRaycastBlock(bool value)
    {
        EnsureCanvasGroup();

        if (canvasGroup == null)
            return;

        canvasGroup.blocksRaycasts = value;
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null)
            return;

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (cardData == null)
            return;

        OnClicked?.Invoke(this);
    }

    public void ClearClickListeners()
    {
        OnClicked = null;
    }

}

