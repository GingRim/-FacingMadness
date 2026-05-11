using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UI_Card : OpenableUIBase
{
    [SerializeField] CardColorType cardColor;
    [SerializeField] CardTagType cardTag;
    [SerializeField] CostType costType;
    [SerializeField] UnityEngine.UI.Image costImage;

    // 임시로 끈거임 수정 해야함
    //public override void Carddrging(UIManager manager)
    //{
    //    
    //    InputManager.OnMouseLeftButton -= LeftButton;
    //    InputManager.OnMouseLeftButton += LeftButton;
    //    InputManager.OnMouseMove -= MoveToMouse;
    //    InputManager.OnMouseMove += MoveToMouse;
    //}
    //
    //private void LeftButton(bool value, Vector2 screenPosition, Vector3 WorldPosition)
    //{
    //    throw new NotImplementedException();
    //}
    //
    //private void MoveToMouse(Vector2 screenPosition, Vector3 WorldPosition)
    //{
    //    throw new NotImplementedException();
    //}
}
