using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 용어 정리 한번해야 함 직업이 아니라 스텟으로 간소화 및 평균화 필요
public class UI_ButtonSwap : MonoBehaviour
{
    private UI_CharacterCreationScreen creationScreen;

    [Header("캐릭터 생성 데이터")]
    [SerializeField]
    private CharacterPresetData characterPreset;

    [SerializeField] TextMeshProUGUI SText;
    [SerializeField] TextMeshProUGUI DText;
    [SerializeField] TextMeshProUGUI HText;
    [SerializeField] TextMeshProUGUI IText;
    [SerializeField] TextMeshProUGUI EText;

    [SerializeField] Image image;
    [SerializeField] Image hunterImage;
    [SerializeField] Image privateDetectiveImage;
    [SerializeField] Image athleteImage;
    [SerializeField] Image researcherImage;
    [SerializeField] Image religiousFanaticImage;

    [SerializeField] Sprite hunterdefaultSprite;
    [SerializeField] Sprite privateDetectivedefaultSprite;
    [SerializeField] Sprite athletedefaultSprite;
    [SerializeField] Sprite researcherdefaultSprite;
    [SerializeField] Sprite religiousFanaticdefaultSprite;
    [SerializeField] Sprite selectedSprite;



    // �ٷ�, ��ø, �ǰ�, ����, ����(���ŷ�) ��
    public void Hunter()
    {
        ResetAllImages();
        SetImage(image, selectedSprite);
        SelectConfiguredPreset();
    }
    public void Privatedetective()
    {
        ResetAllImages();
        SetImage(image, selectedSprite);
        SelectConfiguredPreset();

    }
    public void Athlete()
    {
        ResetAllImages();
        SetImage(image, selectedSprite);
        SelectConfiguredPreset();

    }
    public void Researcher()
    {
        ResetAllImages();
        SetImage(image, selectedSprite);
        SelectConfiguredPreset();

    }
    public void Religiousfanatic()
    {
        ResetAllImages();
        SetImage(image, selectedSprite);
        SelectConfiguredPreset();

    }

    public void SetIngameMessage(string stitle, string dtitle, string htitle, string ititle, string etitle)
    {
        SText?.SetText(stitle);
        DText?.SetText(dtitle);
        HText?.SetText(htitle);
        IText?.SetText(ititle);
        EText?.SetText(etitle);
    }


    void ResetAllImages()
    {
        SetImage(hunterImage, hunterdefaultSprite);
        SetImage(privateDetectiveImage, privateDetectivedefaultSprite);
        SetImage(athleteImage, athletedefaultSprite);
        SetImage(researcherImage, researcherdefaultSprite);
        SetImage(religiousFanaticImage, religiousFanaticdefaultSprite);
    }
    public void SetImage(Image targetImage, Sprite wantSprite)
    {
        if (targetImage != null)
            targetImage.sprite = wantSprite;
    }

    /// <summary>
    /// 버튼에 연결된 CharacterPresetData의 표시값과 생성값을 함께 적용합니다.
    /// </summary>
    private void SelectConfiguredPreset()
    {
        if (characterPreset == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Character Preset이 연결되지 않았습니다.");
            return;
        }

        if (creationScreen == null)
        {
            creationScreen =
                GetComponentInParent<UI_CharacterCreationScreen>();
        }

        if (creationScreen == null)
        {
            Debug.LogWarning(
                "UI_ButtonSwap: 캐릭터 생성 화면을 찾지 못했습니다.");
            return;
        }

        SetIngameMessage(
            $": {characterPreset.strength}",
            $": {characterPreset.agility}",
            $": {characterPreset.health}",
            $": {characterPreset.intelligence}",
            $": {characterPreset.will}");

        creationScreen.SelectBuildData(
            characterPreset.ToBuildData());
    }

}
