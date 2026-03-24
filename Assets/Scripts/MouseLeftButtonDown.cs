using System;
using UnityEngine;

public class MouseLeftButtonDown : MonoBehaviour
{

    void Start()
    {
        InputManager.OnMouseLeftDown += MouseLeftDown;
    }

    private void MouseLeftDown(Vector3 position)
    {
        throw new NotImplementedException();
    }



   // void Start()
   // {
   //     InputManager.OnMouseMove += MoveToMouse;
   //  }

    // void MoveToMouse(Vector2 screenPosition, Vector3 Worldposition)
    // {
    //     transform.position = Worldposition;
    //}
}
