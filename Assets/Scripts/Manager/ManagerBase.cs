using System.Collections;
using UnityEngine;

public abstract class ManagerBase : MonoBehaviour
{
    GameManager _connectedManager;

    //Cunnect를 자유롭게 하기 위해서 Virtual을 써줄 건데!
    //OCP => Open Closed Principle : 개방폐쇄원칙 (확장에는 열려있으나 수정에는 닫혀 있음

    public IEnumerator Connect(GameManager newManager)
    {
        if(_connectedManager != null) Disconnect();
        
        _connectedManager = newManager;
        yield return OnConnected(newManager);
    }  

    protected abstract IEnumerator OnConnected(GameManager newManager);
    
    public void Disconnect()
    {
        _connectedManager = null;
        OnDisconnected();
    }

    protected abstract void OnDisconnected();
}
