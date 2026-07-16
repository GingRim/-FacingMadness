using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : ManagerBase
{
    public Camera MainCamera { get; private set; }

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        SetMainCamera(Camera.main); 
        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    public void SetMainCamera(Camera wantCamera)
    {

        MainCamera = wantCamera;
        if (MainCamera) // != null����
        {

        }

    }


    public void GetRaycastResult(Vector2 screenPosition, List<RaycastResult> outResult)
    {
        EventSystem currentEvent = EventSystem.current;

        if (!currentEvent) return;

        PointerEventData eventData = new(currentEvent);
        eventData.position = screenPosition;
        currentEvent.RaycastAll(eventData, outResult);
        

    }
}
