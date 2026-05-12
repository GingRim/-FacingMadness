using System;
using UnityEngine;

public class UI_TargetHoverInfo : OpenableUIBase
{

   [SerializeField] Vector2 shiftedPosition;
    /// <summary>
    /// 현재 마우스를 가리킨 캐릭터 저장
    /// </summary>
    CharacterBase target;
        
    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] TMPro.TextMeshProUGUI coommentText;
    [SerializeField] UnityEngine.UI.Image Icon;
    [SerializeField] UnityEngine.UI.Slider bar;

    /// <summary>
    /// 초기화 함수
    /// </summary>
    /// <param name="manager"></param>
    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        InputManager.OnMouseHover -= HoverInfoChange;
        InputManager.OnMouseHover += HoverInfoChange;
        //마우스를 따라가는 경우
        //InputManager.OnMouseMove -= MoveToMouse;
        //InputManager.OnMouseMove += MoveToMouse;
    }

    private void MoveToMouse(Vector2 screenPosition, Vector3 WorldPosition)
    {
        transform.position = screenPosition + shiftedPosition;
    }

    /// <summary>
    /// 해제하는 함수
    /// </summary>
    /// <param name="manager"></param>
    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        InputManager.OnMouseHover -= HoverInfoChange;
        //InputManager.OnMouseMove -= MoveToMouse;
    }

    void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
    {
        CharacterBase asCharacter = newTarget?.GetComponent<CharacterBase>();

        nameText.SetText(asCharacter.name);
        //asCharacter.CharacterName, nameText.name
        if (asCharacter != null) Open();
        //asCharacter != null,newTarget
        else Close();

        target = asCharacter;

    }


}
