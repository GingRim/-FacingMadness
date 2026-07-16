using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 용어 정리 한번해야 함 직업이 아니라 스텟으로 간소화 및 평균화 필요
public class UI_ButtonSwap : MonoBehaviour
{
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
        SetIngameMessage(": 6", ": 2", ": 5", ": 4", ": 4");
        ResetAllImages();
        SetImage(image, selectedSprite);
    }
    public void Privatedetective()
    {
        SetIngameMessage(": 4", ": 6", ": 4", ": 5", ": 2");
        ResetAllImages();
        SetImage(image, selectedSprite);

    }
    public void Athlete()
    {
        SetIngameMessage(": 5", ": 4", ": 6", ": 2", ": 4");
        ResetAllImages();
        SetImage(image, selectedSprite);

    }
    public void Researcher()
    {
        SetIngameMessage(": 2", ": 4", ": 4", ": 6", ": 5");
        ResetAllImages();
        SetImage(image, selectedSprite);

    }
    public void Religiousfanatic()
    {
        SetIngameMessage(": 5", ": 4", ": 2", ": 4", ": 6");
        ResetAllImages();
        SetImage(image, selectedSprite);

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

}
