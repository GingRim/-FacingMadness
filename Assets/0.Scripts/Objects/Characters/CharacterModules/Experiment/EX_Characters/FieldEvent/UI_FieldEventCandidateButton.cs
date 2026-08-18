using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UI_FieldEventCandidateButton : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField]
    private Button button;

    [Header("이벤트 표시")]
    [SerializeField]
    private TextMeshProUGUI eventNameText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    private FieldEventData eventData;
    private Action<FieldEventData> onSelected;

    public FieldEventData EventData => eventData;

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

    public void SetEvent(FieldEventData newEventData, Action<FieldEventData> selectedCallback)
    {
        eventData = newEventData;
        onSelected = selectedCallback;

        if (eventNameText != null)
        {
            eventNameText.SetText(eventData != null ? eventData.EventName : string.Empty);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(eventData != null ? eventData.Description : string.Empty);
        }

        gameObject.SetActive(eventData != null);
    }

    public void Clear()
    {
        eventData = null;
        onSelected = null;

        if (eventNameText != null)
        {
            eventNameText.SetText(string.Empty);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(string.Empty);
        }

        gameObject.SetActive(false);
    }

    private void HandleClicked()
    {
        if (eventData == null)
            return;

        onSelected?.Invoke(eventData);
    }
}
