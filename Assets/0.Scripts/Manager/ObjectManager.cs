using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;



public class ObjectManager : ManagerBase
{

    readonly string[] globalPoolSettings =
    {
        "GlobalCharacterPool",
        "GlobalControllerPool",
        "GlobalEffectPool",
        "GlobalObjectPool",
        "GlobalUIPool",

    };
    //[SerializeField] PoolSetting[] testSettings; �׽�Ʈ ����

    // ����Ʈ : �迭�� ���ѵ� �߰� ���Ű� ����,  �뷮  ŭ  ó���ӵ��� ������
    // �迭 : ����Ʈ�� ����ѵ� �߰� ���Ű� �����,�뷮 ���� ó���ӵ� ����

    List<PoolRequest> loadedPoolRequests = new();

    static  Dictionary<string, ObjectPoolModule> PoolDictionary = new();// ������ ��ųʸ� ��ųʸ� ����(����) 

    protected override IEnumerator OnConnected(GameManager NewManager)
    {
        RegistrationHierarchy();
        RegistrationPool(globalPoolSettings);
        InitializePool();

        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    // ������Ʈ Ǯ��(ȭ�鿡�� on off)

    public static GameObject CreateObject(string WantName)
    {
        if (PoolDictionary.TryGetValue(WantName, out ObjectPoolModule pool))
        {
            return pool.CreateObject();
        }
        GameObject prefab = DataManager.LoadDataFile<GameObject>(WantName);
        if(prefab)
        {
            return Instantiate(prefab);
        }
        return null;
    }
 
    public static GameObject CreateObject(string WantName, Transform parent = null)
    {
        GameObject result = null;

        WantName = WantName.ToLower();

        if (PoolDictionary.TryGetValue(WantName, out ObjectPoolModule pool))
        {
            result = pool.CreateObject(parent);
        }
        else
        {
            if (DataManager.TryLoadDataFile(WantName, out GameObject prefab))
            {
                if (prefab) result = Instantiate(prefab, parent);
                
            }
        }

        if (!result) UIManager.ClaimErrorMessage(SystemMessage.ObjectNameNotFound(WantName));

        RegistrationObject(result);

            return result;
    }
    public static GameObject CreateObject(GameObject prefab, Transform parent = null)
    {
        if (prefab == null) return null;

        GameObject result = Instantiate(prefab, parent); //�����
        RegistrationObject(result); // ����Ѵ�.
        // �� ģ���� ��� ���������� ��� üũ�ұ�?
        // ���� ����� �� "������Ʈ"�� ����� ������
        // "���� ������Ʈ"�� ����� ���� �ƴϱ� ������
        // IFunctionable�� �� ���� "������Ʈ"��.

        return result;
    }

    // �θ� �ڽİ��� ũ�� ���̷� �����Ǳ⿡ �̻��� �ൿ�� ���� ���´�.
    public static GameObject CreateObject(string WantName, Vector3 position)
    {
        GameObject result = CreateObject(WantName);
        if (result) result.transform.position = position;
        return result;
    }
    public static GameObject CreateObject(GameObject prefab, Vector3 position)
    {
        GameObject result = CreateObject(prefab);
        if (result) result.transform.position = position;
        return result;
    }

    public static GameObject CreateObject(string WantName, Vector3 position, Quaternion rotatoon)
    {
        GameObject result = CreateObject(WantName);
        if (result)
        {
            result.transform.position = position;
            result.transform.rotation = rotatoon;
        }
        return result;
    }
    public static GameObject CreateObject(GameObject prefab, Vector3 position, Quaternion rotatoon)
    {
        GameObject result = CreateObject(prefab);
        if (result)
        {
            result.transform.position = position;
            result.transform.rotation = rotatoon;
        }
        return result;
    }

    public static GameObject CreateObject(string WantName, Transform parent, Vector3 position, Space space = Space.Self)
    {
        GameObject result = CreateObject(WantName, parent);
        if (result)
        {
            switch (space)
            {
                case Space.World:
                    result.transform.position = position;// ���밪�� ��������
                    break;

                case Space.Self:
                    result.transform.localPosition = position; // �θ� ��������
                    break;
            }
            result.transform.position = position;
        }
        return result;
    }
    public static GameObject CreateObject(GameObject prefab, Transform parent, Vector3 position, Space space = Space.Self)
    {
        GameObject result = CreateObject(prefab, parent);
        if (result)
        {
            switch(space)
            {
                case Space.World:
                    result.transform.position = position;// ���밪�� ��������
                    break;

                case Space.Self:
                    result.transform.localPosition = position; // �θ� ��������
                    break;
            }
            result.transform.position = position; 
        }
        return result;
    }

    public static GameObject CreateObject(string WantName, Transform parent, Vector3 position, Quaternion rotatoon, Space space = Space.Self)
    {
        GameObject result = CreateObject(WantName, parent);
        if (result)
        {
            switch (space)
            {
                case Space.World:
                    result.transform.position = position; // ���밪 ����
                    result.transform.rotation = rotatoon;
                    break;
                case Space.Self:
                    result.transform.localPosition = position;
                    result.transform.localRotation = rotatoon;
                    break;
            }

        }
        return result;
    }
    public static GameObject CreateObject(GameObject prefab, Transform parent, Vector3 position, Quaternion rotatoon, Space space = Space.Self)
    {
        GameObject result = CreateObject(prefab, parent);
        if (result)
        {
            switch (space)
            {
                case Space.World: 
                    result.transform.position = position; // ���밪 ����
                    result.transform.rotation = rotatoon;
                    break;
                case Space.Self:
                    result.transform.localPosition = position;
                    result.transform.localRotation = rotatoon;
                    break;
            }
               
        }
        return result;
    }

    public static GameObject CreateObject(string WantName, Vector3 position, Quaternion rotatoon, Vector3 scale)
    {
        GameObject result = CreateObject(WantName);
        if (result)
        {
            result.transform.position = position;
            result.transform.rotation = rotatoon;
            result.transform.localScale = scale;
        }
        return result;
    }
    public static GameObject CreateObject(GameObject prefab, Vector3 position, Quaternion rotatoon, Vector3 scale)
    {
        GameObject result = CreateObject(prefab);
        if (result)
        {
            result.transform.position = position;
            result.transform.rotation = rotatoon;
            result.transform.localScale = scale;
        }
        return result;
    }

    public static GameObject CreateObject(string WantName, Transform parent, Vector3 position, Quaternion rotatoon, Vector3 scale, Space space = Space.Self)
    {
        GameObject result = CreateObject(WantName, parent);
        if (result)
        {
            switch (space)
            {
                case Space.World:
                    result.transform.position = position; // ���밪 ����
                    result.transform.rotation = rotatoon;
                    result.transform.localScale = scale;
                    //                    // ��¥ ũ�� 1 �θ��� ũ�� 2 �϶� ���� ũ��� 0.5
                    //                    // ���� ũ�⸦ 3���� �ϰ� �� �Ҷ� �θ��� ũ�� 2 �̶�� ��¥(����)�� ũ��� 1.5�̿��� �Ѵ�.
                    //                    // ��¥ ũ�� ������ �θ� ũ�⸦ ���Ͼ� ���� �����ָ� ���ڴ�.
                    //                    // �� �θ��� �θ� �ִٸ� ��� �ؾ� �ϳ�.
                    //                    // ����(1.2) * (����(0.9) / ����(1.2)) = ����(0.9)
                    //                    // ����(0.9) * (����(1.2) / ����(0.9)) = ����(1.2)
                    //                    //                  3 * (4/3) = 4
                    //                    Vector3 originLocalScal = result.transform.localScale;
                    //                    Vector3 originLossyScal = result.transform.lossyScale;
                    //                    float scaledScaleX = scale.x * (originLocalScal.x / originLossyScal.x);
                    //                    float scaldeScaleY = scale.y * (originLocalScal.y / originLossyScal.y);
                    //                    float scaldeScaleZ = scale.z * (originLocalScal.z / originLossyScal.z);
                    //                    result.transform.localScale = new Vector3(scaledScaleX, scaldeScaleY, scaldeScaleZ);
                    break;
                case Space.Self:
                    result.transform.localPosition = position;
                    result.transform.localRotation = rotatoon;
                    result.transform.localScale = scale;
                    break;
            }

        }
        return result;
    }
    public static GameObject CreateObject(GameObject prefab, Transform parent, Vector3 position, Quaternion rotatoon,Vector3 scale, Space space = Space.Self)
   {
       GameObject result = CreateObject(prefab, parent);
        if (result)
        {
            switch (space)
            {
              case Space.World: 
                   result.transform.position = position; // ���밪 ����
                  result.transform.rotation = rotatoon;
                   result.transform.localScale = scale;
//                    // ��¥ ũ�� 1 �θ��� ũ�� 2 �϶� ���� ũ��� 0.5
//                    // ���� ũ�⸦ 3���� �ϰ� �� �Ҷ� �θ��� ũ�� 2 �̶�� ��¥(����)�� ũ��� 1.5�̿��� �Ѵ�.
//                    // ��¥ ũ�� ������ �θ� ũ�⸦ ���Ͼ� ���� �����ָ� ���ڴ�.
//                    // �� �θ��� �θ� �ִٸ� ��� �ؾ� �ϳ�.
//                    // ����(1.2) * (����(0.9) / ����(1.2)) = ����(0.9)
//                    // ����(0.9) * (����(1.2) / ����(0.9)) = ����(1.2)
//                    //                  3 * (4/3) = 4
//                    Vector3 originLocalScal = result.transform.localScale;
//                    Vector3 originLossyScal = result.transform.lossyScale;
//                    float scaledScaleX = scale.x * (originLocalScal.x / originLossyScal.x);
//                    float scaldeScaleY = scale.y * (originLocalScal.y / originLossyScal.y);
//                    float scaldeScaleZ = scale.z * (originLocalScal.z / originLossyScal.z);
//                    result.transform.localScale = new Vector3(scaledScaleX, scaldeScaleY, scaldeScaleZ);
                    break;
                case Space.Self:
                    result.transform.localPosition = position;
                    result.transform.localRotation = rotatoon;
                    result.transform.localScale = scale;
                    break;
            }
               
        }
       return result;
   }


    public static void RegistrationObject(GameObject target)
    {
        if (target)
        {
            foreach (var current in target.GetComponentsInChildren<IFunctionable>())
            {
                current.RegistrationFunctions();
            }
        }
    }

    public static void DestroyObject(GameObject target)
    {
        if (!target) return;
        UnRegistrationObject(target);
        if (target.TryGetComponent(out PooledObject pool))
        {
            pool.OnEnqueue();
        }
        else
        {
            Destroy(target);
        }
    }

    public static void UnRegistrationObject(GameObject target)
    {
        if (!target) return;

        foreach (var current in target.GetComponentsInChildren<IFunctionable>())
        {
            current.UnRegistrationFunctions();
        }

    }

    public void RegistrationHierarchy()
    {
        foreach(MonoBehaviour current in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) 
        { 
            if(current is IFunctionable currentFunctionable)
            {
                currentFunctionable.RegistrationFunctions();
            }
        }
    }

    public void RegistrationPool(string poolName)
    {
        poolName = poolName.ToLower();

        PoolRequest currentRequest = DataManager.LoadDataFile<PoolRequest>(poolName);
        loadedPoolRequests.Add(currentRequest);

        if (currentRequest == null) return;
        if (currentRequest.settings ==  null) return;
        //         �л�           ���� �л� in   3�г� 4��
        foreach (PoolSetting currentSetting in currentRequest.settings)
        {
            string curretName = currentSetting.poolName.ToLower();
            
            GameObject currentPrefab = currentSetting.target;
           
            if (currentPrefab == null) continue;

            if (PoolDictionary.ContainsKey(curretName)) continue;

            PoolDictionary.Add(curretName, new(currentSetting)); // ���
        }
    }
    public void RegistrationPool(params string[] poolNames)
    {
        foreach (string poolName in poolNames)
        {
            RegistrationPool(poolName);
        }
    }

    public void InitializePool()
    {
        foreach(ObjectPoolModule currentPool in PoolDictionary.Values)
        {
            currentPool?.Initialize();
        }
    }
}
            