using UnityEngine;

public delegate void PoolEnqueueEvent(GameObject target);
public delegate void PoolDequeueEvent(GameObject target);
public class PooledObject : MonoBehaviour
{

    public event PoolEnqueueEvent OnEnqueueEvent;
    public event PoolDequeueEvent OnDequeueEvent;

    public void OnEnqueue()
    {
        if (OnEnqueueEvent != null) OnEnqueueEvent.Invoke(gameObject);
        else Destroy(gameObject);
    }

    public void OnDequeue() 
    {
        OnDequeueEvent?.Invoke(gameObject);
    }
}
