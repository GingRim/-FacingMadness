using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MissionButton : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button button;

    [Header("미션 정보")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image missionImage;

    private FieldMissionData missionData;

    public FieldMissionData MissionData => missionData;

    public event Action<FieldMissionData> OnSelected;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        BindButton();
    }

    private void OnDestroy()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(HandleClick);
    }

    private void BindButton()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(HandleClick);

        button.onClick.AddListener(HandleClick);
    }

    public void SetMission(FieldMissionData mission)
    {
        missionData = mission;

        if (missionData == null)
        {
            Clear();
            return;
        }

        if (nameText != null)
        {
            nameText.SetText(missionData.MissionName);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(missionData.Description);
        }

        if (missionImage != null)
        {
            missionImage.sprite = missionData.MissionImage;

            missionImage.gameObject.SetActive(missionData.MissionImage != null);
        }

        if (button != null)
        {
            button.interactable = true;
        }

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        missionData = null;

        if (nameText != null)
        {
            nameText.SetText(string.Empty);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(string.Empty);
        }

        if (missionImage != null)
        {
            missionImage.sprite = null;
            missionImage.gameObject.SetActive(false);
        }

        if (button != null)
        {
            button.interactable = false;
        }

        gameObject.SetActive(false);
    }

    private void HandleClick()
    {
        if (missionData == null)
            return;

        OnSelected?.Invoke(missionData);
    }
}
