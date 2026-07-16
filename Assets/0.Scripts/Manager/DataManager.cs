using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DataManager : ManagerBase
{
    // ��ü �����͸� �����ϴ� ��ųʸ�(����)
    static Dictionary<System.Type, Dictionary<string, Object>> dataDictionary = new();

    //������Ƽ�� ������������� �Լ�
    // int GetLoadCount();
    event System.Action DisconnectedEvent;

    public override int LoadCount
    {
        get
        {
            var task = Addressables.LoadResourceLocationsAsync("OGlobals");
            var result = task.WaitForCompletion();
            int count = result.Count; //������ ã�ƿ���


            task.Release();// ������ ��ױ�
            return count; // �׷��� �� ������ ������!
        }
    }

    protected override IEnumerator OnConnected(GameManager newManager)
    {

        UIBase loading = UIManager.GetUIM2(UIType.Loading);
        IProgress<int> progressUI = loading as IProgress<int>;
        IStatus<string> statusUI = loading as IStatus<string>;

        int loaded = 0;
        int total = LoadCount;
        string loadString = "Load Data";


        System.Action PrgressOnLoad = () =>
        {
            loaded++;
            progressUI?.AddCurrent(1);
            statusUI?.SetCurrentStatus($"{loadString}({loaded}/{total})");
        };

        // ���ο� Ÿ���� ���𰡸� �߰��Ҷ� ����� �ֱ�
        loadString = "Load Game Objects";
        yield return LoadAllFromAssetBundle<GameObject>("OGlobals", PrgressOnLoad).WaitForTask();

        loadString = "Load Pool Requests";
        yield return LoadAllFromAssetBundle<PoolRequest>("OGlobals", PrgressOnLoad).WaitForTask();
        
        loadString = "Load Items";
        yield return LoadAllFromAssetBundle<ItemContainer>("OGlobals", PrgressOnLoad).WaitForTask();



        //GameObject prefab = LoadDataFile<GameObject>("Square");
        //Instantiate(prefab, Random.insideUnitCircle * 5.0f, Random.rotation);
        //LoadFileFromAssetBundle<GameObject>("Origin/Prefabs/Square.prefab");
        // �ε� ������ => �ִ� �� ������, ���� �� ������ �ߴ���
        //               ���� / �ִ�    1 / 100 = 0.01



        yield return null;
    }

    protected override void OnDisconnected()
    {
        DisconnectedEvent?.Invoke();
    }

    bool TryGetFileFromResources<T>(string path, out T result) where T : Object
    {
        result = Resources.Load<T>(path);
        return result != null;
    }

    //Asset Bundle (���� ����) (���Ƿ� ������ ī�װ��)
    //DLC => Ư�� ī�װ���� �ִ� ��Ҹ� �ٿ�ε� �ϰ� �� ���ΰ� �� ���ΰ�?
    //Addressable(��巿���)
    // async�Լ��� �񵿱� �Լ� => �ٸ� �Լ��� ���� ���ư� �� �ִ� �Լ�!
    // �����Ѵٴ� ���� ����� �ҷ��� �� �ִ�. �׸��� �����Ҷ� ���� �߿��� ���� : ��� ���� ���ΰ�
    public static void SaveDataFile<T>(T target) where T : Object
    {
        if (target == null) return;
        Dictionary<string, Object> innerDictionary;
        // ���ݱ��� �̷� Ÿ���� Object�� ������ �� ó�� ���� ���̱⿡ innerDictionary�� �������� ���� ���̱� ������!
        if (!dataDictionary.TryGetValue(typeof(T), out innerDictionary))
        {
            innerDictionary = new();
            dataDictionary.Add(typeof(T), innerDictionary);
        }

        innerDictionary.TryAdd(target.name.ToLower(), target);

    }

    protected static T GetDataFromDictnay<T>(string fileName) where T : Object
    {
        if (string.IsNullOrEmpty(fileName)) return null;

        fileName = fileName.ToLower();

        if (dataDictionary.TryGetValue(typeof(T), out Dictionary<string, Object> innerDictionary))
        {
            if (innerDictionary.TryGetValue(fileName, out Object result))
            {
                return result as T;
            }
        }
        return null;
    }

    public static T LoadDataFile<T>(string fileName) where T : Object
    {
        //1. ���ڰ� ���� �� fileName is null  nullString    
        //2. ���ڰ� ���� �� fileName.lecgth == 0 emptyString

        T result = GetDataFromDictnay<T>(fileName);
        if(!result)UIManager.ClaimErrorMessage(SystemMessage.FileNameNotFound(fileName));
        return result;
        
    }
     public static bool TryLoadDataFile<T>(string fileName, out T result) where T : Object
     {
        result = GetDataFromDictnay<T>(fileName);
        return result;
    }

    // public static IEnumerator WaitForTask(this Task targetTask)
    //{
    //    yield return new WaitUntil(() => targetTask.IsCompleted);
    //     targetTask.Dispose();
    // }


    public async Task LoadAllFromAssetBundle<T>(string label, System.Action actionForEachLoad) where T : Object
        {
            var finder = Addressables.LoadAssetsAsync(label, (T loaded) =>
            {
                SaveDataFile(loaded); 
                actionForEachLoad(); 
            });
            Task result = finder.Task;
            await result;
            DisconnectedEvent += () => finder.Release();
        }

        public async void LoadFileFromAssetBundle<T>(string address) where T : Object
        {
            var finder = Addressables.LoadAssetAsync<T>(address);
            await finder.Task;
            SaveDataFile(finder.Result);

            // A �Ǵ� An- ���� ���۵Ǵ� �ܾ�� ~�� �ƴ�, �ݴ�Ǵ� ���λ縦 �ǹ��Ѵ�.
            // ���α׷����� �񵿱�ȭ�� �ϳ��� ���μ����� ������ ���� �ƴϴ�. �� ��Ƽ ������
            // ��Ƽ ������ <-> �̱� ������
            // �ѹ��� �����ϴ� ����� ���� �� ������ �Ϸ�� �� �ִ�.
            // ����� ���� �ߴµ�.. ������� ���� ���� �־ ���ٲ۴�.!
            // ����� ���� ���ϰ� �׾����� üũ�� ���ΰ�?
            // ���� �丸 �Ծ��� ������ �� �Դ� �ð��� ��������.
            // �ѹ��� ���ư��� ���� ū ������ �ټ� �ֱ⿡ �ٸ� ���̵��� ��ٸ���.
            // �̸� => "�����"�̶�� �Ѵ�.
        }



    }
