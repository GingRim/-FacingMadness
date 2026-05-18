using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ObjectPoolModule
{
    PoolSetting _setting;
    public PoolSetting Setting => _setting;

    public ObjectPoolModule(PoolSetting newSetting)
    {
        _setting = newSetting;
    }

    Transform rootTransfom;

    Queue<GameObject> prepareQueue = new();

    public void Initialize()
    {
        rootTransfom = new GameObject(Setting.poolName).transform;
        for(int i = 0; i < Setting.countInitial; i++)
        {
            PrepareObject();
        }
    }
    GameObject PrepareObject()
    {
        if (!Setting.target) return null;
        GameObject result = CreateFromPrefab();
        EnqueueObject(result);
        return result;
    }

    void PrepareObjects(uint count, out GameObject activeObject)
    {
        if (!Setting.target)
        {
            activeObject = null;
            return;
        }

        activeObject = CreateFromPrefab();

        for (int i = 1; i < count; i++)
        {
            GameObject result = CreateFromPrefab();
            EnqueueObject(result);
        }   
    }
    public GameObject CreateFromPrefab()
    {
        GameObject result = ObjectManager.CreateObject(Setting.target, rootTransfom);
        if (result)
        {
            result.name = Setting.poolName;
            if(result.TryGetComponent(out PooledObject pool))
            {
                pool.OnEnqueueEvent -= DestroyObject;
                pool.OnEnqueueEvent += DestroyObject;
            }
        }
        return result;
    }

    public GameObject CreateObject(Transform parent = null)
    {
        GameObject result;

        if(!prepareQueue.TryDequeue(out result))
        {
            PrepareObjects(Setting.coutAdditional, out result);
        }

        if(result)
        {
            result.transform.SetParent(parent, false);
            result.SetActive(true);
            //실험
            if (result.TryGetComponent(out RectTransform rect))
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.identity;
            }
            //----------

            if (result.TryGetComponent(out PooledObject pool))
            {
                pool.OnDequeue();
            }

        }
        return result;
    }

    public void DestroyObject(GameObject target)
    {
        EnqueueObject(target);
        if(target)
        {
            target.transform.SetParent(rootTransfom);
        }
    }

    public void EnqueueObject(GameObject target)
    {
        if(target)
        {
            target.SetActive(false);
           
            prepareQueue.Enqueue(target);

        }
    }

}
