using System;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void DragStartEvent(UI_DraggableWindow dragTarget, Vector2 startPosition);


public class UI_DraggableWindow : UIBase, IPointerDownHandler
{
    public event DragStartEvent OnDragStart;

    [SerializeField] RectTransform rootTransform;
    //마지막으로 수신받은 마우스 위치
    Vector2 currentScreenPosition;
    // 이동하려고 했는데 막혀버린 위치!
    Vector2 shiftedPosition;

    internal void SetMouseStartPosition(Vector2 screenPosition)
    {
        currentScreenPosition = screenPosition;
        shiftedPosition = Vector2.zero;
    }

    internal void SetMouseCurrentPosition(Vector2 screenPosition)
    {
        Vector2 screenDelta = screenPosition - currentScreenPosition;
        currentScreenPosition = screenPosition;

      //본인의 "Pivot"위치를 기준으로!
        Rect rootRect = rootTransform.rect;

      //                                   원래 위치                    이동량
        rootRect.position += (Vector2)(rootTransform.localPosition / UIManager.UIScale)+ screenDelta;

        //보정해주는 만큼 위치 이동을 자제한다.
        Vector2 overScreen = rootRect.InversedAABB(UIManager.UIBoundary);
       
      //       보정값            실제 움직임
        if(shiftedPosition.x * screenDelta.x > 0.0f)
        {

            float counter = Mathf.Min(Mathf.Abs(screenDelta.x), Mathf.Abs(shiftedPosition.x));
            //원래 값의 부호를 넣어주기!
            counter *= Mathf.Sign(shiftedPosition.x);
            shiftedPosition.x -= counter;
            screenDelta.x -= counter;

        }

        if(shiftedPosition.y * screenDelta.y > 0.0f)
        {
            float counter = Mathf.Min(Mathf.Abs(screenDelta.y), Mathf.Abs(shiftedPosition.y));
            counter *= Mathf.Sign(shiftedPosition.y);
            shiftedPosition.y -= counter;
            screenDelta.y -= counter;
        }
        //magnitude는 규모
        //spr는 제곱 
        //왜 굳이 규모를 제곱해서 보는 걸까?
        if(screenDelta.sqrMagnitude == 0.0f) return;

        //이동한 총량을 저장해 놓기!
        shiftedPosition += overScreen;
        screenDelta += overScreen;

        Vector3 positionDelta = (Vector3)screenDelta;

        if(UIManager.UIScale > 0.0f) positionDelta /= UIManager.UIScale;


        rootTransform.localPosition += positionDelta;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDragStart?.Invoke(this, eventData.position);

    }

}
