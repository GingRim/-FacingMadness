using UnityEngine;
using UnityEngine.EventSystems;

public class UI_PullUpWindow : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        //������ �ǵ��� �Ѵ�.
        transform.SetAsLastSibling();
    }
}
