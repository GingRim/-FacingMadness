using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class UI_KeywordHoverInfo : OpenableUIBase
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI commentText;

    UI_Keyword target;

    [SerializeField] private UI_KeywordHoverInfo keywordEncyclopedia;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);

        InputManager.OnMouseLeftButton -= HoverInfoChange;
        InputManager.OnMouseLeftButton += HoverInfoChange;

        InputManager.OnEncyclopedia -= EncyclopediaInput;
        InputManager.OnEncyclopedia += EncyclopediaInput;

        InputManager.OnPausePriority -= TryCloseByPause;
        InputManager.OnPausePriority += TryCloseByPause;
    }
  
    
    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);

        InputManager.OnMouseLeftButton -= HoverInfoChange;
        InputManager.OnEncyclopedia -= EncyclopediaInput;
        InputManager.OnPausePriority -= TryCloseByPause;
    }

  
    private void HoverInfoChange(bool value, Vector2 screenPosition, Vector3 WorldPosition)
    {
        // 마우스를 누른 순간만 처리
        if (!value)
            return;

        if (GameManager.Instance == null || GameManager.Instance.Input == null)
        {
            return;
        }

        GameObject clickedObject = GameManager.Instance.Input.GetGameObjectUnderCursor();

        UI_Keyword asKeyword = clickedObject?.GetComponentInParent<UI_Keyword>();

        if (asKeyword == null)
            return;

        target = asKeyword;

        SetKeywordInfo(target.KeywordType);

        OpenEncyclopedia();
    }

   
    public void OpenEncyclopedia()
    {
        if (IsPauseOpen())
            return;

        Open();
    }


    private void EncyclopediaInput(bool value)
    {
        if (!value)
            return;

        // 일시정지 중에는 Tab 입력 무시
        if (IsPauseOpen())
            return;

        if (IsOpen)
        {
            Close();
        }
        else
        {
            OpenEncyclopedia();
        }
    }


    private bool TryCloseByPause()
    {
        if (!IsOpen)
            return false;

        Close();

        // 도감이 Pause 입력을 처리했으므로
        // UI_BattleScreen의 CanelPause는 실행되지 않음
        return true;
    }


    private bool IsPauseOpen()
    {
        UIBase pauseUI =
            UIManager.GetUIM2(UIType.Pause);

        return pauseUI != null &&
               pauseUI.isActiveAndEnabled;
    }

    /// <summary>
    /// 생성된 도감 버튼에서 이름과 설명을 전달한다.
    /// </summary>
    public void SetEncyclopediaInfo(string displayName, string description)
    {
        if (nameText != null)
        {
            nameText.SetText(displayName);
        }

        if (commentText != null)
        {
            commentText.SetText(description);
        }
    }


    private void SetKeywordInfo(KeywordType type)
    {
        switch (type)
        {
            case KeywordType.D:
                SetEncyclopediaInfo(
                    "D(주사위)",
                    "본 게임에서 주사위는 10면체 주사위 1개를 의미합니다.");
                break;

            case KeywordType.Adjudgment:
                SetEncyclopediaInfo(
                    "판정",
                    "주사위를 굴려 나온 값으로 성공 또는 실패 등을 판단하는 것을 의미합니다. 이때 보정치의 영향을 받습니다.");
                break;

            case KeywordType.Bonus:
                SetEncyclopediaInfo(
                    "보정치",
                    "판정의 주사위 결과에 더하거나 빼는 수치를 의미합니다.");
                break;

            case KeywordType.Blessing:
                SetEncyclopediaInfo(
                    "축복",
                    "판정할 때 주사위를 한 번 더 굴려 더 높은 값을 사용합니다.");
                break;

            case KeywordType.Cursed:
                SetEncyclopediaInfo(
                    "저주",
                    "판정할 때 주사위를 한 번 더 굴려 더 낮은 값을 사용합니다.");
                break;

            case KeywordType.GreatSuccess:
                SetEncyclopediaInfo(
                    "대성공",
                    "크리티컬 조건을 만족하여 더 강한 이점을 얻는 결과입니다.");
                break;

            case KeywordType.Fumble:
                SetEncyclopediaInfo(
                    "펌블",
                    "주사위 값과 보정치의 합이 1 이하일 때 발생하며 상당한 불이익을 받습니다.");
                break;

            default:
                SetEncyclopediaInfo(
                    "???",
                    "존재해서는 안 되는 기억");
                break;
        }
    }

}