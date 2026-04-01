using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : ManagerBase
{
    public Camera MainCamera { get; private set; }

    public Physics2DRaycaster Raycaster2D { get; private set; }
    public PhysicsRaycaster Raycaster3D { get; private set; }


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
        if (MainCamera) // != nullÆ÷ÇÔ
        {
            Raycaster2D = wantCamera.GetComponent<Physics2DRaycaster>();
            Raycaster3D = wantCamera.GetComponent<PhysicsRaycaster>();
        }

    }

    public void GetRaycastResult2D(Vector2 screenPosition, List<RaycastResult> outResult)
    {
        PointerEventData eventData = new(EventSystem.current);
        eventData.position = screenPosition;
        Raycaster2D?.Raycast(eventData, outResult);
    }

    public void GetRaycastResult3D(Vector2 screenPosition, List<RaycastResult> outResult)
    {
        PointerEventData eventData = new(EventSystem.current);
        eventData.position = screenPosition;
        Raycaster3D?.Raycast(eventData, outResult);

    }
}
