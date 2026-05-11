using System;
using UnityEngine;

public class UI_TargetHoverInfo : OpenableUIBase
{

   [SerializeField] Vector2 shiftedPosition;

    CharacterBase target;
    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] UnityEngine.UI.Image Icon;
    [SerializeField] UnityEngine.UI.Slider bar;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        InputManager.OnMouseHover -= HovrInfoChange;
        InputManager.OnMouseHover += HovrInfoChange;
        InputManager.OnMouseMove -= MoveToMouse;
        InputManager.OnMouseMove += MoveToMouse;
    }

    private void MoveToMouse(Vector2 screenPosition, Vector3 WorldPosition)
    {
        transform.position = screenPosition + shiftedPosition;
    }

    public override void Unregistration(UIManager manager)
    {
        base.Registration(manager );
        InputManager.OnMouseHover -= HovrInfoChange;

    }

    void HovrInfoChange(GameObject newTarget, GameObject oldTarget)
    {
        CharacterBase asCharacter = newTarget?.GetComponent<CharacterBase>();

        nameText.SetText(nameText.name);
        
        if(newTarget) Open();

        else Close();

        target = asCharacter;

    }


}
