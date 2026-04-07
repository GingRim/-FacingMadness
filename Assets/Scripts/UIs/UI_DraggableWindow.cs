using System;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void DragStartEvent(UI_DraggableWindow dragTarget, Vector2 startPosition);


public class UI_DraggableWindow : UIBase, IPointerDownHandler
{
    public event DragStartEvent OnDragStart;

    [SerializeField] RectTransform rootTransform;

    Vector2 currentScreenPosition;

    internal void SetMouseStartPosition(Vector2 screenPosition)
    {
        currentScreenPosition = screenPosition;
    }

    internal void SetMouseCurrentPosition(Vector2 screenPosition)
    {
        Vector2 screenDelta = screenPosition - currentScreenPosition;

        Rect rootRect = rootTransform.rect;

        rootRect.position += screenDelta;

        //보정해주는 만큼 위치 이동을 자제한다.
        screenDelta += rootRect.InversedAABB(UIManager.UIBoundary);

        Vector3 positionDelta = (Vector3)screenDelta;

        if(UIManager.UIScale > 0.0f) positionDelta /= UIManager.UIScale;


        rootTransform.localPosition += positionDelta;

        currentScreenPosition = screenPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDragStart?.Invoke(this, eventData.position);

    }

}
