using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class UI_KeywordHoverInfo : OpenableUIBase
{
    [SerializeField] Vector2 shiftedPosition;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI commentText;
    [SerializeField] Image image;

    UI_Keyword target;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);

        InputManager.OnMouseLeftButton -= HoverInfoChange;
        InputManager.OnMouseLeftButton += HoverInfoChange;

        // 마우스를 따라가게 하고 싶으면 사용
        // InputManager.OnMouseMove -= MoveToMouse;
        // InputManager.OnMouseMove += MoveToMouse;
    }

    private void HoverInfoChange(bool value, Vector2 screenPosition, Vector3 WorldPosition)
    {
        // 마우스를 누른 순간만 처리
        if (!value)
            return;

        GameObject clickedObject = GameManager.Instance.Input.GetGameObjectUnderCursor();

        UI_Keyword asKeyword =
            clickedObject?.GetComponentInParent<UI_Keyword>();

        if (asKeyword == null)
        {
            target = null;
            Close();
            return;
        }

        target = asKeyword;

        SetKeywordInfo(target.KeywordType);

        Open();
    }



    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);

        InputManager.OnMouseLeftButton -= HoverInfoChange;
       // InputManager.OnMouseMove -= MoveToMouse;
    }

    private void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
    {
        transform.position = screenPosition + shiftedPosition;
    }

    void SetKeywordInfo(KeywordType type)
    {
        switch (type)
        {
            case KeywordType.D:
                nameText.SetText("D(주사위)");
                commentText.SetText("본 게임에서 주사위는 10면채 주사위 1개를 의미합니다.");
                break;

                case KeywordType.Adjudgment:
                nameText.SetText("판정");
                commentText.SetText("주사위를 굴려 나온 값을 가지고 성공 혹은 실패 또는 주사위값을 이용한 모든것을 의미합니다. 이때 보정치의 영향을 받습니다.");
                break;

            case KeywordType.Bonus:
                nameText.SetText("보정치");
                commentText.SetText("판정의 주사위 결과에 + 혹은 -가 되는 것을 의미합니다.");
                break;

            case KeywordType.Blessing:
                nameText.SetText("축복");
                commentText.SetText("판정하때 주사위를 한번 더 굴려 더 높은 값을 사용하는 것을 의미합니다.");
                break;

            case KeywordType.Cursed:
                nameText.SetText("저주");
                commentText.SetText("판정하때 주사위를 한번 더 굴려 더 낮은 값을 사용하는 것을 의미합니다.");
                break;

            case KeywordType.GreatSuccess:
                nameText.SetText("대 선공");
                commentText.SetText("판정에서 주사위 값과 보정치의 합이 15이상을 의미하며 이때 판정치가 아무리 높아도 15초가라도 성공으로 보며 강한 이점을 얻는다.");
                break;
           
            case KeywordType.Fumble:
                nameText.SetText("펌블");
                commentText.SetText("판정에서 주사위 값과 보정치의 합이 1이하일 경우를 의미하며 이때 판정치가 아무리 낮아도 1이하라도 실패 하며 상당한 패널티를 얻는다.");
                break;
           


            default:
                nameText.SetText("???");
                commentText.SetText("존재해선 않되는 기억");
                break;
        }
    }
}
