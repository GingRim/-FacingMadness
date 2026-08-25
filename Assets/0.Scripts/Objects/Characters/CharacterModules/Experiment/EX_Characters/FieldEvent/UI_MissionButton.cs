using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 하나의 미션 정보를 표시하고
/// 클릭된 미션 데이터를 외부에 전달합니다.
/// </summary>
public class UI_MissionButton : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField]
    private Button button;

    [Header("미션 식별")]
    [SerializeField]
    private TextMeshProUGUI missionIdText;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [Header("미션 설명")]
    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [Header("미션 이미지")]
    [SerializeField]
    private Image missionImage;

    private FieldMissionData missionData;

    public FieldMissionData MissionData =>
        missionData;

    public event Action<FieldMissionData> OnSelected;

    /// <summary>
    /// Button 컴포넌트를 확인하고 클릭 이벤트를 연결합니다.
    /// </summary>
    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        BindButton();
    }

    /// <summary>
    /// 오브젝트가 제거될 때 버튼 클릭 이벤트 연결을 해제합니다.
    /// </summary>
    private void OnDestroy()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(
            HandleClick);
    }

    /// <summary>
    /// 버튼 클릭 시 미션 선택 처리가 실행되도록 연결합니다.
    /// </summary>
    private void BindButton()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(
            HandleClick);

        button.onClick.AddListener(
            HandleClick);
    }

    /// <summary>
    /// 표시할 미션 데이터를 받아
    /// ID, 이름, 설명과 이미지를 갱신합니다.
    /// </summary>
    /// <param name="mission">표시할 미션 데이터입니다.</param>
    public void SetMission(
        FieldMissionData mission)
    {
        missionData = mission;

        if (missionData == null)
        {
            Clear();
            return;
        }

        if (missionIdText != null)
        {
            missionIdText.SetText(
                missionData.MissionId);
        }

        if (nameText != null)
        {
            nameText.SetText(
                missionData.MissionName);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(
                missionData.Description);
        }

        if (missionImage != null)
        {
            missionImage.sprite =
                missionData.MissionImage;

            missionImage.gameObject.SetActive(
                missionData.MissionImage != null);
        }

        if (button != null)
        {
            button.interactable = true;
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 현재 미션 데이터와 표시 내용을 초기화하고
    /// 미션 버튼을 비활성화합니다.
    /// </summary>
    public void Clear()
    {
        missionData = null;

        if (missionIdText != null)
        {
            missionIdText.SetText(
                string.Empty);
        }

        if (nameText != null)
        {
            nameText.SetText(
                string.Empty);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(
                string.Empty);
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

    /// <summary>
    /// 현재 미션이 존재하면 선택 이벤트를 발생시킵니다.
    /// </summary>
    private void HandleClick()
    {
        if (missionData == null)
            return;

        OnSelected?.Invoke(missionData);
    }
}
