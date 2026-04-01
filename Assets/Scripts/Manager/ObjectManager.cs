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
        "GlobalUIPool"
    };
    //[SerializeField] PoolSetting[] testSettings; 테스트 셋팅

    // 리스트 : 배열과 비스한데 추가 제거가 쉬움,  용량  큼  처리속도가 느리다
    // 배열 : 리스트와 비슷한데 추가 제거가 어려움,용량 적음 처리속도 빠름

    List<PoolRequest> loadedPoolRequests = new();

    static  Dictionary<string, ObjectPoolModule> PoolDictionary = new();// 프리펩 디셔너리 디셔너리 생성(선언) 

    protected override IEnumerator OnConnected(GameManager NewManager)
    {
        RegistrationPool(globalPoolSettings);
        InitializePool();

        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    // 오브젝트 풀링(화면에서 on off)

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
            GameObject prefab = DataManager.LoadDataFile<GameObject>(WantName);
            if (prefab)
            {
                result = Instantiate(prefab, parent);
            }
        }
        RegistrationObject(result);

            return result;
    }
    public static GameObject CreateObject(GameObject prefab, Transform parent = null)
    {
        if (prefab == null) return null;

        GameObject result = Instantiate(prefab, parent); //만들고
        RegistrationObject(result); // 등록한다.
        // 이 친구가 등록 가능한지를 어떻게 체크할까?
        // 저희가 만드는 건 "컴포넌트"를 만드는 것이지
        // "게임 오브젝트"를 만드는 것이 아니기 때문에
        // IFunctionable이 들어간 곳은 "컴포너트"다.

        return result;
    }

    // 부모 자식간의 크기 차이로 결정되기에 이상한 행동이 많이 나온다.
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
                    result.transform.position = position;// 절대값을 기준으로
                    break;

                case Space.Self:
                    result.transform.localPosition = position; // 부모를 기준으로
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
                    result.transform.position = position;// 절대값을 기준으로
                    break;

                case Space.Self:
                    result.transform.localPosition = position; // 부모를 기준으로
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
                    result.transform.position = position; // 절대값 기준
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
                    result.transform.position = position; // 절대값 기준
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
                    result.transform.position = position; // 절대값 기준
                    result.transform.rotation = rotatoon;
                    result.transform.localScale = scale;
                    //                    // 진짜 크기 1 부모의 크기 2 일때 나의 크기는 0.5
                    //                    // 나의 크기를 3으로 하고 자 할때 부모의 크기 2 이라면 진짜(로컬)의 크기는 1.5이여야 한다.
                    //                    // 진짜 크기 나누기 부모 크기를 비교하야 값을 자져주면 좋겠다.
                    //                    // 단 부모의 부모가 있다면 어떻게 해야 하나.
                    //                    // 로컬(1.2) * (월드(0.9) / 로컬(1.2)) = 월드(0.9)
                    //                    // 월드(0.9) * (로컬(1.2) / 월드(0.9)) = 로컬(1.2)
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
                   result.transform.position = position; // 절대값 기준
                  result.transform.rotation = rotatoon;
                   result.transform.localScale = scale;
//                    // 진짜 크기 1 부모의 크기 2 일때 나의 크기는 0.5
//                    // 나의 크기를 3으로 하고 자 할때 부모의 크기 2 이라면 진짜(로컬)의 크기는 1.5이여야 한다.
//                    // 진짜 크기 나누기 부모 크기를 비교하야 값을 자져주면 좋겠다.
//                    // 단 부모의 부모가 있다면 어떻게 해야 하나.
//                    // 로컬(1.2) * (월드(0.9) / 로컬(1.2)) = 월드(0.9)
//                    // 월드(0.9) * (로컬(1.2) / 월드(0.9)) = 로컬(1.2)
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

    public void RegistrationPool(string poolName)
    {
        poolName = poolName.ToLower();

        PoolRequest currentRequest = DataManager.LoadDataFile<PoolRequest>(poolName);
        loadedPoolRequests.Add(currentRequest);

        if (currentRequest == null) return;
        if (currentRequest.settings ==  null) return;
        //         학생           다음 학생 in   3학년 4반
        foreach (PoolSetting currentSetting in currentRequest.settings)
        {
            string curretName = currentSetting.poolName.ToLower();
            
            GameObject currentPrefab = currentSetting.target;
           
            if (currentPrefab == null) continue;

            if (PoolDictionary.ContainsKey(curretName)) continue;

            PoolDictionary.Add(curretName, new(currentSetting)); // 등록
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
            