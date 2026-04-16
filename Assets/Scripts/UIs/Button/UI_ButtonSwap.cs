using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ButtonSwap : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI SText;
    [SerializeField] TextMeshProUGUI DText;
    [SerializeField] TextMeshProUGUI HText;
    [SerializeField] TextMeshProUGUI IText;
    [SerializeField] TextMeshProUGUI EText;
    
    [SerializeField] Image image;

    [SerializeField] Sprite ChangeSprite;



    // 근력, 민첩, 건강, 지능, 의지(정신력) 순
    public void Hunter()
    {
        SetIngameMessage(": 6", ": 2", ": 5", ": 4", ": 4");
        SetIngameImage(ChangeSprite);
    
    }
    public void Privatedetective()
    {
        SetIngameMessage(": 4", ": 6", ": 4", ": 5", ": 2");
        SetIngameImage(ChangeSprite);
    }
    public void Athlete()
    {
        SetIngameMessage(": 5", ": 4", ": 6", ": 2", ": 4");
        SetIngameImage(ChangeSprite);
    }
    public void Researcher()
    {
        SetIngameMessage(": 2", ": 4", ": 4", ": 6", ": 5");
        SetIngameImage(ChangeSprite);
    }
    public void Religiousfanatic()
    {
        SetIngameMessage(": 5", ": 4", ": 2", ": 4", ": 6");
        SetIngameImage(ChangeSprite);
    }

    public void SetIngameMessage(string stitle, string dtitle, string htitle, string ititle, string etitle)
    {
        SText?.SetText(stitle);
        DText?.SetText(dtitle);
        HText?.SetText(htitle);
        IText?.SetText(ititle);
        EText?.SetText(etitle);
    }

    public void SetIngameImage(Sprite wantSprite)
    {
        if (image != null)
            image.sprite = wantSprite;
    }

}
