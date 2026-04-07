using UnityEngine;
using UnityEngine.EventSystems;

public class UI_PullUpWindow : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        //막내가 되도록 한다.
        transform.SetAsLastSibling();
    }
}
