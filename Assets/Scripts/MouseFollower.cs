using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
        InputManager.OnMouseRightButton += DestroyOnmouse; //���� ��ü�� �ݶ��̴��� �־�� �Ѵ�.
        InputManager.OnMouseLeftButton += CeateToMouse;
    }

    public void UnRegistrationFunctions()
    {
        InputManager.OnMouseRightButton -= DestroyOnmouse;
        InputManager.OnMouseLeftButton -= CeateToMouse;
    }

    void CeateToMouse(bool value, Vector2 screenPosution, Vector3 worldposition)
    {
        GameObject inst = ObjectManager .CreateObject(DataManager.LoadDataFile<GameObject>("Square"));
        inst.transform.position = worldposition;
    }

    void MoveToMouse(Vector2 screenPosition, Vector3 worldposition)
    {
        transform.position = worldposition;
    }

    void DestroyOnmouse(bool value, Vector2 screenPosition, Vector3 worldposition)
    {
        ObjectManager.DestroyObject(GameManager.Instance.Input.GetGameObjectUnderCursor());
    }


    
}
