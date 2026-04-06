using System;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void DragStartEvent(UI_DraggableWindow dragTarget, Vector2 startPosition);


public class UI_DraggableWindow : UIBase, IPointerDownHandler, IPointerUpHandler
{
    public event DragStartEvent OnDragStart;

    [SerializeField] RectTransform rootTransform;

    internal void SetMouseStartPosition(Vector2 screenPosition)
    {
        
    }

    internal void SetMousePosition(Vector2 screenPosition)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDragStart?.Invoke(this, eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}
