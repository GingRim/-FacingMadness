using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 챕터 이야기 또는 필드 이벤트 결과의
/// 이미지와 문장을 같은 화면에 표시합니다.
/// 버튼 입력은 현재 표시 모드에 맞는 완료 처리로 전달합니다.
/// </summary>
public class UI_ChapterStory : MonoBehaviour
{
    private enum StoryDisplayMode
    {
        None,
        Chapter,
        EventResult
    }

    [Header("챕터 이미지")]
    [SerializeField]
    private Image storyImage;

    [Header("챕터 이야기")]
    [SerializeField]
    private TextMeshProUGUI storyText;

    [Header("다음 / 스킵")]
    [SerializeField]
    private Button skipButton;

    private FieldChapterData currentChapter;
    private StoryDisplayMode displayMode;
    private Action onEventResultConfirmed;
    private bool isOpen;

    public bool IsOpen => isOpen;

    public FieldChapterData CurrentChapter => currentChapter;

    public bool IsShowingEventResult => displayMode == StoryDisplayMode.EventResult;

    public event Action<FieldChapterData> OnStoryCompleted;

    /// <summary>
    /// 버튼 이벤트만 연결합니다.
    /// 초기 활성 상태는 씬 또는 프리팹 설정을 따릅니다.
    /// </summary>
    private void Awake()
    {
        BindSkipButton();
    }

    private void OnDestroy()
    {
        UnbindSkipButton();
    }

    private void BindSkipButton()
    {
        if (skipButton == null)
            return;

        skipButton.onClick.RemoveListener(HandleSkip);
        skipButton.onClick.AddListener(HandleSkip);
    }

    private void UnbindSkipButton()
    {
        if (skipButton == null)
            return;

        skipButton.onClick.RemoveListener(HandleSkip);
    }

    /// <summary>
    /// 결정된 챕터의 이미지와 이야기를 표시합니다.
    /// </summary>
    public bool Open(FieldChapterData chapter)
    {
        if (chapter == null)
        {
            Debug.LogWarning(
                "UI_ChapterStory: 표시할 챕터가 없습니다.");

            return false;
        }

        currentChapter = chapter;
        displayMode = StoryDisplayMode.Chapter;
        onEventResultConfirmed = null;
        isOpen = true;

        SetContent(
            currentChapter.Description,
            currentChapter.ChapterImage);

        if (!ActivateWindow("챕터"))
            return false;

        return true;
    }

    /// <summary>
    /// 필드 이벤트 결과의 문장과 이미지를 표시합니다.
    /// 챕터 완료 이벤트는 발생시키지 않습니다.
    /// </summary>
    public bool OpenEventResult(string resultText, Sprite resultImage, Action onConfirmed)
    {
        currentChapter = null;
        displayMode = StoryDisplayMode.EventResult;
        onEventResultConfirmed = onConfirmed;
        isOpen = true;

        SetContent(resultText, resultImage);

        if (!ActivateWindow("이벤트 결과"))
            return false;

        return true;
    }

    private void SetContent(string text, Sprite image)
    {
        if (storyImage != null)
        {
            storyImage.sprite = image;
            storyImage.gameObject.SetActive(image != null);
        }

        if (storyText != null)
        {
            storyText.SetText(text ?? string.Empty);
        }
    }

    /// <summary>
    /// 결과창을 활성화하고 부모 비활성 문제를 검사합니다.
    /// </summary>
    private bool ActivateWindow(string contentName)
    {
        gameObject.SetActive(true);

        // 같은 Canvas 안에서 다른 필드 UI보다 나중에 그려지도록 한다.
        transform.SetAsLastSibling();

        // 기존에 CanvasGroup이 붙어 있어도 결과창이 투명하게 남지 않도록 한다.
        CanvasGroup existingCanvasGroup =
            GetComponent<CanvasGroup>();

        if (existingCanvasGroup != null)
        {
            existingCanvasGroup.alpha = 1f;
            existingCanvasGroup.interactable = true;
            existingCanvasGroup.blocksRaycasts = true;
        }

        if (gameObject.activeInHierarchy)
            return true;

        Debug.LogError(
            $"UI_ChapterStory: {contentName} 화면의 " +
            "부모 오브젝트가 비활성 상태입니다.");

        return false;
    }

    /// <summary>
    /// 화면과 현재 표시 정보를 초기화합니다.
    /// 완료 이벤트는 발생시키지 않습니다.
    /// </summary>
    public void Close()
    {
        isOpen = false;
        currentChapter = null;
        displayMode = StoryDisplayMode.None;
        onEventResultConfirmed = null;

        if (storyImage != null)
        {
            storyImage.sprite = null;
            storyImage.gameObject.SetActive(false);
        }

        if (storyText != null)
        {
            storyText.SetText(string.Empty);
        }

        gameObject.SetActive(false);
    }

    private void HandleSkip()
    {
        if (!isOpen)
            return;

        if (displayMode == StoryDisplayMode.EventResult)
        {
            CompleteEventResult();
            return;
        }

        CompleteStory();
    }

    private void CompleteStory()
    {
        if (currentChapter == null)
            return;

        FieldChapterData completedChapter =
            currentChapter;

        Close();

        OnStoryCompleted?.Invoke(completedChapter);
    }

    private void CompleteEventResult()
    {
        Action completed =
            onEventResultConfirmed;

        Close();

        completed?.Invoke();
    }
}
