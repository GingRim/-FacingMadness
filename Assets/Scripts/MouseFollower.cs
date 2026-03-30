using UnityEngine;

public class MouseFollower : MonoBehaviour, IFunctionable
{
    void Start()
    {
        RegistrationFunctions();
    }

    void OnDestroy()
    {
        UnRegistrationFunctions();
    }


    public void RegistrationFunctions()
    {
        InputManager.OnMouseRightUp += DestroyOnmouse; //마출 물체는 콜라이더가 있어야 한다.
        InputManager.OnMouseLeftDown += CeateToMouse;
    }

    public void UnRegistrationFunctions()
    {
        InputManager.OnMouseRightUp -= DestroyOnmouse;
        InputManager.OnMouseLeftDown -= CeateToMouse;
    }

    void CeateToMouse(Vector2 screenPosution, Vector3 worldposition)
    {
        GameObject inst = ObjectManager.CreateObject(DataManager.LoadDataFile<GameObject>("Square"));
        inst.transform.position = worldposition;
    }

    void MoveToMouse(Vector2 screenPosition, Vector3 worldposition)
    {
        transform.position = worldposition;
    }

    void DestroyOnmouse(Vector2 screenPosition, Vector3 worldposition)
    {
        ObjectManager.DestroyObject(GameManager.Instance.Input.GetGameObjectUnderCursor());
    }


}
