using System;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void DragStartEvent(UI_DraggableWindow dragTarget, Vector2 startPosition);


public class UI_DraggableWindow : UIBase, IPointerDownHandler
{
    public event DragStartEvent OnDragStart;

    [SerializeField] RectTransform rootTransform;
    //���������� ���Ź��� ���콺 ��ġ
    Vector2 currentScreenPosition;
    // �̵��Ϸ��� �ߴµ� ������� ��ġ!
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

      //������ "Pivot"��ġ�� ��������!
        Rect rootRect = rootTransform.rect;

      //                                   ���� ��ġ                    �̵���
        rootRect.position += (Vector2)(rootTransform.localPosition / UIManager.UIScale)+ screenDelta;

        //�������ִ� ��ŭ ��ġ �̵��� �����Ѵ�.
        Vector2 overScreen = rootRect.InversedAABB(UIManager.UIBoundary);
       
      //       ������            ���� ������
        if(shiftedPosition.x * screenDelta.x > 0.0f)
        {

            float counter = Mathf.Min(Mathf.Abs(screenDelta.x), Mathf.Abs(shiftedPosition.x));
            //���� ���� ��ȣ�� �־��ֱ�!
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
        //magnitude�� �Ը�
        //spr�� ���� 
        //�� ���� �Ը� �����ؼ� ���� �ɱ�?
        if(screenDelta.sqrMagnitude == 0.0f) return;

        //�̵��� �ѷ��� ������ ����!
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
