using System.Collections.Generic;
using UnityEngine;

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
       for(int i = 0; i < _setting.counInitial; i++)
       {
            PrepareObject();     
       }
    }
    GameObject PrepareObject()
    {
        GameObject result = ObjectManager.CreateObject(Setting.target, rootTransfom);
        if (!Setting.target) return null;
        
        
        if(result)
        {
            result.SetActive(false);
        }

        result.name = Setting.poolName;

        prepareQueue.Enqueue(result);

        return result;
    }

    public GameObject CreateObject()
    {
        GameObject result;
        if(prepareQueue.TryDequeue(out result))
        {
            PrepareObject();
        }

        return result;
    }

}
