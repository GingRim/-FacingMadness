using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 시스템에서 결정된 챕터의 이미지와 이야기를 표시합니다.
/// 스킵 버튼을 누르면 챕터 이야기 표시를 종료합니다.
/// </summary>
public class UI_ChapterStory : MonoBehaviour
{
    [Header("챕터 이미지")]
    [SerializeField]
    private Image storyImage;

    [Header("챕터 이야기")]
    [SerializeField]
    private TextMeshProUGUI storyText;

    [Header("스킵")]
    [SerializeField]
    private Button skipButton;

    private FieldChapterData currentChapter;

    private bool isOpen;

    public bool IsOpen => isOpen;

    public FieldChapterData CurrentChapter =>
        currentChapter;

    public event Action<FieldChapterData>
        OnStoryCompleted;

    /// <summary>
    /// 스킵 버튼의 클릭 이벤트를 연결하고
    /// 챕터 이야기 화면을 초기 상태로 닫습니다.
    /// </summary>
    private void Awake()
    {
        BindSkipButton();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 오브젝트가 제거될 때 스킵 버튼 이벤트를 해제합니다.
    /// </summary>
    private void OnDestroy()
    {
        UnbindSkipButton();
    }

    /// <summary>
    /// 스킵 버튼을 챕터 이야기 완료 처리와 연결합니다.
    /// </summary>
    private void BindSkipButton()
    {
        if (skipButton == null)
            return;

        skipButton.onClick.RemoveListener(HandleSkip);

        skipButton.onClick.AddListener(HandleSkip);
    }

    /// <summary>
    /// 스킵 버튼에 연결된 이벤트를 해제합니다.
    /// </summary>
    private void UnbindSkipButton()
    {
        if (skipButton == null)
            return;

        skipButton.onClick.RemoveListener(
            HandleSkip);
    }

    /// <summary>
    /// 결정된 챕터의 이미지와 이야기를 표시합니다.
    /// </summary>
    /// <param name="chapter">표시할 챕터 데이터입니다.</param>
    /// <returns>스토리 화면을 열었으면 true를 반환합니다.</returns>
    public bool Open(
        FieldChapterData chapter)
    {
        if (chapter == null)
        {
            Debug.LogWarning(
                "UI_ChapterStory: 표시할 챕터가 없습니다.");

            return false;
        }

        currentChapter = chapter;
        isOpen = true;

        if (storyImage != null)
        {
            storyImage.sprite =
                currentChapter.ChapterImage;

            storyImage.gameObject.SetActive(
                currentChapter.ChapterImage != null);
        }

        if (storyText != null)
        {
            storyText.SetText(
                currentChapter.Description);
        }

        gameObject.SetActive(true);

        return true;
    }

    /// <summary>
    /// 챕터 이야기 화면과 현재 표시 정보를 초기화합니다.
    /// 완료 이벤트는 발생시키지 않습니다.
    /// </summary>
    public void Close()
    {
        isOpen = false;
        currentChapter = null;

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

    /// <summary>
    /// 스킵 버튼을 눌렀을 때 현재 챕터 이야기 표시를 완료합니다.
    /// </summary>
    private void HandleSkip()
    {
        if (!isOpen)
            return;

        CompleteStory();
    }

    /// <summary>
    /// 챕터 이야기 화면을 닫고
    /// 완료된 챕터를 진행 제어기에 전달합니다.
    /// </summary>
    private void CompleteStory()
    {
        if (currentChapter == null)
            return;

        FieldChapterData completedChapter =
            currentChapter;

        Close();

        OnStoryCompleted?.Invoke(
            completedChapter);
    }
}