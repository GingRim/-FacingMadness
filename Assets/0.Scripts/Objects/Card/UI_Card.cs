using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// CardInstance의 현재 정보를 카드 UI에 표시합니다.
/// 클릭과 드래그 입력은 CardClick이 담당합니다.
/// </summary>
public class UI_Card : PooledObject
{
    [Header("카드 이미지")]
    [SerializeField]
    private Image illustrationImage;

    [SerializeField]
    private Image frameImage;

    [Tooltip("카드 우측 상단의 작은 아이콘 이미지입니다.")]
    [SerializeField]
    private Image iconImage;

    [Header("카드 텍스트")]
    [SerializeField]
    private TMP_Text nameText;

    [Tooltip("카드 효과 설명을 표시합니다.")]
    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text keywordText;

    [SerializeField]
    private TMP_Text durabilityText;

    [Tooltip("능력치 카드의 현재 등급을 표시합니다.")]
    [SerializeField]
    private TMP_Text gradeText;

    private CardInstance cardInstance;


    /// <summary>
    /// 이 UI에 연결된 실제 카드 한 장입니다.
    /// </summary>
    public CardInstance CardInstance => cardInstance;

    private void Awake()
    {
        OnEnqueueEvent -= HandleEnqueue;
        OnEnqueueEvent += HandleEnqueue;
    }

    private void OnDestroy()
    {
        UnsubscribeCardEvents();

        OnEnqueueEvent -= HandleEnqueue;
    }

    private void HandleEnqueue(GameObject target)
    {
        UnsubscribeCardEvents();

        cardInstance = null;

        Clear();
    }

    /// <summary>
    /// 연결된 카드의 원본 데이터입니다.
    /// </summary>
    public CardData CardData => cardInstance != null ? cardInstance.Data : null;

    private void OnEnable()
    {
        SubscribeCardEvents();
        RefreshAll();
    }

    private void OnDisable()
    {
        UnsubscribeCardEvents();
    }

    /// <summary>
    /// 카드 인스턴스를 UI에 연결합니다.
    /// </summary>
    public void SetCard(CardInstance instance)
    {
        if (cardInstance == instance)
        {
            RefreshAll();
            return;
        }

        UnsubscribeCardEvents();

        cardInstance = instance;

        SubscribeCardEvents();
        RefreshAll();
    }

    /// <summary>
    /// 현재 카드 정보를 다시 표시합니다.
    /// </summary>
    public void RefreshAll()
    {
        if (cardInstance == null || cardInstance.Data == null)
        {
            Clear();
            return;
        }

        RefreshName();
        RefreshIllustration();
        RefreshIcon();
        RefreshFrame();
        RefreshKeywords();
        RefreshDescription();
        RefreshDurability();
        RefreshGrade();
    }

    private void RefreshName()
    {
        if (nameText == null)
            return;

        nameText.SetText(cardInstance.CardName);
    }

    private void RefreshIllustration()
    {
        if (illustrationImage == null)
            return;

        Sprite illustration = cardInstance.Data.Illustration;

        illustrationImage.sprite = illustration;

        illustrationImage.enabled = illustration != null;
    }

    private void RefreshIcon()
    {
        if (iconImage == null)
            return;

        Sprite icon = cardInstance.Data.Icon;

        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }

    private void RefreshDescription()
    {
        if (descriptionText == null)
            return;

        descriptionText.SetText(cardInstance.Data.description ?? string.Empty);
    }

    private void RefreshFrame()
    {
        if (frameImage == null || cardInstance == null)
            return;

        frameImage.color = GetFrameColor(cardInstance.Color);
        frameImage.enabled = true;
    }

    private void RefreshKeywords()
    {
        if (keywordText == null)
            return;

        string keywordDisplay = cardInstance.GetKeywordDisplayText();

        keywordText.SetText(keywordDisplay);

        keywordText.gameObject.SetActive(!string.IsNullOrEmpty(keywordDisplay));
    }

    private void RefreshDurability()
    {
        if (durabilityText == null)
            return;

        if (cardInstance == null || !cardInstance.HasDurability)
        {
            durabilityText.SetText(string.Empty);

            durabilityText.gameObject.SetActive(false);

            return;
        }

        durabilityText.gameObject.SetActive(true);

        durabilityText.SetText(
            $"{cardInstance.CurrentDurability} / " +
            $"{cardInstance.MaximumDurability}");
    }

    private void RefreshGrade()
    {
        if (gradeText == null)
            return;

        bool shouldShow = cardInstance != null &&
                          cardInstance.Data != null &&
                          cardInstance.Data.IsAbilityCard;

        gradeText.gameObject.SetActive(shouldShow);
        gradeText.SetText(shouldShow ? cardInstance.CurrentGrade.ToString() : string.Empty);
    }

    private Color GetFrameColor(CardColorType color)
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

            case CardColorType.Colorless:
            default:
                return Color.gray;
        }
    }

    private void SubscribeCardEvents()
    {
        if (cardInstance == null)
            return;

        UnsubscribeCardEvents();

        cardInstance.OnKeywordChanged += HandleKeywordChanged;

        cardInstance.OnDurabilityChanged += HandleDurabilityChanged;
        cardInstance.OnGradeChanged += HandleGradeChanged;
    }

    private void UnsubscribeCardEvents()
    {
        if (cardInstance == null)
            return;

        cardInstance.OnKeywordChanged -= HandleKeywordChanged;

        cardInstance.OnDurabilityChanged -= HandleDurabilityChanged;
        cardInstance.OnGradeChanged -= HandleGradeChanged;
    }

    private void HandleKeywordChanged(CardInstance changedCard)
    {
        if (changedCard != cardInstance)
            return;

        RefreshKeywords();
        RefreshDurability();
    }

    private void HandleDurabilityChanged(CardInstance changedCard, int current, int maximum)
    {
        if (changedCard != cardInstance)
            return;

        RefreshDurability();
    }

    private void HandleGradeChanged(CardInstance changedCard, int grade)
    {
        if (changedCard != cardInstance)
            return;

        RefreshGrade();
    }

    /// <summary>
    /// UI에 표시된 카드 정보를 제거합니다.
    /// </summary>
    private void Clear()
    {
        if (nameText != null)
        {
            nameText.SetText(string.Empty);
        }

        if (keywordText != null)
        {
            keywordText.SetText(string.Empty);

            keywordText.gameObject.SetActive(false);
        }

        if (durabilityText != null)
        {
            durabilityText.SetText(string.Empty);

            durabilityText.gameObject.SetActive(false);
        }

        if (gradeText != null)
        {
            gradeText.SetText(string.Empty);
            gradeText.gameObject.SetActive(false);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(string.Empty);
        }

        if (illustrationImage != null)
        {
            illustrationImage.sprite = null;
            illustrationImage.enabled = false;
        }

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (frameImage != null)
        {
            frameImage.enabled = false;
        }
    }
}
