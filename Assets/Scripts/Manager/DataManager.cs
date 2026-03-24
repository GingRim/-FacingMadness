using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DataManager : ManagerBase
{
    // 전체 데이터를 저장하는 딕셔너리(사전)
    static Dictionary<System.Type, Dictionary<string, Object>> dataDictionary = new();
    
    //프로퍼티는 변수모양이지만 함수
    // int GetLoadCount();
    
    public override int LoadCount
    {
        get 
        {
            var task = Addressables.LoadResourceLocationsAsync("OGlobals");
            var result = task.WaitForCompletion();
            int count = result.Count; //개수를 찾아오기
            

            task.Release();// 대이터 잠그기
            return count; // 그래서 그 개수를 돌려줌!
        }
    }

    protected override IEnumerator OnConnected(GameManager newManager)
    {

        UIBase loading = UIManager.GetUIM2(UIType.Loading);
        IProgress<int> progressUI = loading as IProgress<int>;
        IStatus<string> statusUI = loading as IStatus<string>;

        int loaded =0;
        int total = LoadCount;

        System.Action PrgressOnLoad = () =>
        {
            loaded++;
            progressUI.AddCurrent(1);
            statusUI.SetCurrentStatus($"Load Data({loaded}/{total})");
        };
        
        LoadAllFromAssetBundle<GameObject>("OGlobals", PrgressOnLoad);



        //GameObject prefab = LoadDataFile<GameObject>("Square");
        //Instantiate(prefab, Random.insideUnitCircle * 5.0f, Random.rotation);
       //LoadFileFromAssetBundle<GameObject>("Origin/Prefabs/Square.prefab");
       // 로딩 진행율 => 최대 몇 개인지, 현재 몇 개까지 했는지
        //               현재 / 최대    1 / 100 = 0.01
       
            
        
        yield return null;
    }

    protected override void OnDisconnected()
    {

    }

    bool TryGetFileFromResources<T>(string path, out T result) where T : Object
    {
        result = Resources.Load<T>(path);
        return result != null;
    }

    //Asset Bundle (에셋 번들) (임의로 지정한 카테고리)
    //DLC => 특정 카테고리에 있는 요소를 다운로드 하게 할 것인가 말 것인가?
    //Addressable(어드렛써블)
    // async함수는 비동기 함수 => 다른 함수와 같이 돌아갈 수 있는 함수!
    // 저장한다는 것은 언재든 불러올 수 있다. 그리고 저장할때 재일 중요한 것은 : 어떻게 꺼낼 것인가
    public static void SaveDataFile<T>(T target) where T : Object
    {
        if (target == null) return;
        Dictionary<string, Object> innerDictionary;
        // 지금까지 이런 타입의 Object가 없었다 즉 처음 보는 것이기에 innerDictionary가 존재하지 않을 것이기 떄문에!
        if (!dataDictionary.TryGetValue(typeof(T), out innerDictionary))
        {
            innerDictionary = new();
            dataDictionary.Add(typeof(T), innerDictionary);
        }

        innerDictionary.TryAdd(target.name, target);

    }

    public static T LoadDataFile<T>(string fileName) where T : Object
    {
        if (dataDictionary.TryGetValue(typeof(T), out Dictionary<string, Object> innerDictionary))
        {
            if(innerDictionary.TryGetValue(fileName, out Object result))
            {
                return result as T;
            }
        }
        return null;
    }

    public async void LoadAllFromAssetBundle<T>(string label, System.Action actionForEachLoad) where T : Object
    {
        var finder = Addressables.LoadAssetsAsync<T>(label, (T loaded) => 
        {
            SaveDataFile(loaded); // 로드 되었으니까 저장
            actionForEachLoad(); // 할일 있다고 하니 해둬야 겠다.
        });
        await finder.Task;
        finder.Release();
    }

    public async void LoadFileFromAssetBundle<T>(string address) where T : Object
    {
        var finder = Addressables.LoadAssetAsync<T>(address);
        await finder.Task;
        SaveDataFile(finder.Result);

        // A 또는 An- 으로 시작되는 단어는 ~이 아닌, 반대되는 접두사를 의미한다.
        // 프로그렘에서 비동기화는 하나의 프로세스로 돌리는 것이 아니다. 즉 멀티 스레드
        // 멀티 스레드 <-> 싱글 스레드
        // 한번에 실행하는 기능의 개수 즉 빠르게 완료될 수 있다.
        // 생명력 감소 했는데.. 생명령을 누가 쓰고 있어서 못바꾼다.!
        // 생명력 감소 안하고 죽었는지 체크할 것인가?
        // 원래 밥만 먹었을 때보다 밥 먹는 시간은 느려진다.
        // 한번에 돌아가는 무언가 큰 변수를 줄수 있기에 다른 아이들이 기다린다.
        // 이를 => "데드락"이라고 한다.
    }
}
