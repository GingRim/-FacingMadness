using System;
using System.Collections;
using UnityEngine;

public delegate void InitializeEvent();
public delegate void UpdateEvent(float deltaTime);
public delegate void DestroyEvent();

public class GameManager : MonoBehaviour
{
    static GameManager _instance;

    public static GameManager Instance => _instance;

    UIManager _ui;
    public UIManager UI =>_ui;

    DataManager _data;
    public DataManager Data => _data;

    ObjectManager _objectM;
    public ObjectManager ObjectM => _objectM;

    SaveManager _save;
    public SaveManager Save => _save;

    SettingManager _setting;
    public SettingManager Setting => _setting;

    LanguageManager _language;
    public LanguageManager Language => _language;

    AudioManager _audio;
    public AudioManager Audio => _audio;

    CameraManager _camera;
    public CameraManager Camera => _camera;

    InputManager _input;
    public  InputManager Input => _input;
    BattleManager _battle;
    public BattleManager Battle => _battle;


    IEnumerator initializing;

    public static event  InitializeEvent OnInitializeManager     ;
    public static event  InitializeEvent OnInitializeController  ;
    public static event  InitializeEvent OnInitializeCharacter   ;
    public static event  InitializeEvent OnInitializeObject      ;
      
    public static event  UpdateEvent     OnUpdateEventManager    ;
    public static event  UpdateEvent     OnUpdateEventController ;
    public static event  UpdateEvent     OnUpdateEventCharacter  ;
    public static event  UpdateEvent     OnUpdateEventObject     ;

    public static event  UpdateEvent     OnPhysicsCharacter      ;
    public static event  UpdateEvent     OnPhysicsObject         ;
    
    public static event  DestroyEvent    OnDestroyEventManager   ;
    public static event  DestroyEvent    OnDestroyEventController;
    public static event  DestroyEvent    OnDestroyEventCharacter ;
    public static event  DestroyEvent    OnDestroyEventObject    ;

    [SerializeField] UIType startScreen;

    public static bool is2D = true;
    bool isLoading = true;
    bool isPlaying =  true;

    //Awake     : �� ��ũ��Ʈ�� ������ ��(�����)(��ħ�� ���� ��)
    //OnEnabled : �� ��ũ��Ʈ�� ������ ��(���� ���� => �غ�) -> ������ ���൵ �ȴ�.
    //OnDisabled: ����
    //Reset     : ���� �����ϱ� ���� �ʱ�ȭ �غ�
    //Start     : �� ��ũ��Ʈ�� ������ ��(�Ϸ��� ����)
    void Awake()
    {
        //���ӸŴ����� �Ͼ ���� ó���� �� �� ���߿� ������ ���� �Ŵ����� �����.
        //������ �ָ� �����Ѵ�.
        //�ϴ� ���� ���̰� �����ϴ� ���� �� ����
        if(Instance  == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
        // �������� �� ������ �� �� ������?
        initializing = InitalizeMangers();

        //�����߱� ������, �� ģ���� "����"��Ű�ų� "�ߴ�"��ų �� �־��
        //������ ��Ű�� ����
        StartCoroutine(initializing);
    }

    

    IEnumerator InitalizeMangers()
    {
        int totalLoadCount = 0;
      totalLoadCount += CreateManager(ref _ui).LoadCount;
      totalLoadCount += CreateManager(ref _data).LoadCount;
      totalLoadCount += CreateManager(ref _objectM).LoadCount;
      totalLoadCount += CreateManager(ref _save).LoadCount ;
      totalLoadCount += CreateManager(ref _setting).LoadCount ;
      totalLoadCount += CreateManager(ref _language).LoadCount ;
      totalLoadCount += CreateManager(ref _audio).LoadCount ;
      totalLoadCount += CreateManager(ref _camera).LoadCount ;
      totalLoadCount += CreateManager(ref _input).LoadCount ;
      totalLoadCount += CreateManager(ref _battle).LoadCount;


        yield return UI.Initialize(this);
       UIBase loadingUI = UIManager.OpenScreenM2(UIType.Loading);
       IProgress<int> loadingProgress = loadingUI as IProgress<int>;
        
       loadingProgress?.Set(0, totalLoadCount);
       
       yield return Data.Connect(this);
        loadingProgress?.AddCurrent(1);
       yield return ObjectM.Connect(this);
        loadingProgress?.AddCurrent(1);
        yield return UI.Connect(this);
        loadingProgress?.AddCurrent(1);
       yield return _save.Connect(this);
        loadingProgress?.AddCurrent(1);
       yield return _setting.Connect(this);
        loadingProgress?.AddCurrent(1);
       yield return _language.Connect(this);
        loadingProgress?.AddCurrent(1);
       yield return _audio.Connect(this);
        loadingProgress?.AddCurrent(1);
       yield return _camera.Connect(this);
        loadingProgress?.AddCurrent(1);
       yield return _input.Connect(this);
        loadingProgress?.AddCurrent(1);
        yield return _battle.Connect(this);
        loadingProgress?.AddCurrent(1);
        yield return new WaitForSeconds(1.0f);


       UIManager.OpenScreenM2(startScreen, ScreenChangeType.ScreenChanger);
        isLoading = false;
    }

    void OnDestroy()
    {
        if(initializing != null) StopCoroutine(initializing);        DeleteManagers();
    }

    void DeleteManagers()
    {
        Battle?.Disconnect();
        //�����Է� 
        Input?.Disconnect();
        //������Ʈ
        ObjectM.Disconnect();
        //�����
        Audio?.Disconnect();
        //���
        Language?.Disconnect();
        //����
        Setting?.Disconnect();
        //���̺�
        Save?.Disconnect();
        //ī�޶�
        Camera?.Disconnect();
        //UI
        UI?.Disconnect();
        //����������
        Data?.Disconnect();
    }
    //�޶����� ���� "�ڷ���"���̶�� �ڷ����� ���� ������ �ۿ��ϴ� �Լ��� ���� �� ���� ������?
    //"Generic Method" => ���� �Լ�
    //��ȯ�� �̸�<�ڷ���>(�Ű�����) where �ڷ��� : �θ�(�����)

    //_input���ٰ� ���� �ְ������ �ٸ� �������� �ٸ� ���� ���� �ִ´�. ����� �Ǵ� ������ �������� �ؾ� �Ѵ�.
    //���� ���� �ٲ�� �Ѵ�. => ���� ���� "����"�Ѵ�. -> ���� ���̶� ����Ǵ� ������ ����� �ֱ�! [Reference => ref]

    ManagerType CreateManager<ManagerType>(ref ManagerType targetVariable) where ManagerType : ManagerBase
    {
        if (targetVariable == null)
        {
            targetVariable = this.TryAddComponent<ManagerType>();
        }
        return targetVariable;
    }

    //���� ���� ���� �ݾ��ִ� ���̴�.
    //���⼭ ���� �Ǹ� ����Ƽ�� ������.
    //��ó���� �����
    //#���� �����ϴ� ģ����!
    //#if, #elif, #else, #endif
    public static void QuitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    //========================================= �Ͻ� ����
    public static void Pause()
    {
        Instance.isPlaying = false;
    }

    public static void Unpause()
    {
        Instance.isPlaying =true;
    }
    //========================================== 



void InvokeInitializeEvent(ref InitializeEvent OriginEvent)
    {
        if (OriginEvent != null)
        {
            InitializeEvent CurrentEvent = OriginEvent;
            OriginEvent = null;
            CurrentEvent.Invoke();
        }
    }

void InvokeDestroyEvent(ref DestroyEvent OriginEvent)
    {
        if (OriginEvent != null)
        {
            DestroyEvent CurrentEvent = OriginEvent;
            OriginEvent = null;
            CurrentEvent.Invoke();
        }
    }


    // ��ΰ� ������Ʈ �ϰڴٰ� �Ѵٸ� ���� ���� ������Ʈ �ϴ� �� �𸣸� ���콺�� ���̵��� ���� ���¿���
    // ���� ��ٸ� ������ ���� ������ ��ġ�� ���
    // �Ͻ����濡�� ������ �ݺ��ؼ� ������ ī�带 ���� �̹� ���� ī����� �� ������ �͵� �� �� �ִ�.
    void Update()
    {   // ���� ������ �� �� �ִ��� ���θ� ������ ���� �ִ�.
        // Pause���´�! => ������Ʈ�� ���� �ʴ´�!
        // �Ŵ��� -> ĳ���� -> ��Ʈ�ѷ� -> ������Ʈ �ʱ�ȭ
        // �Ŵ��� -> ��Ʈ�ѷ� -> ĳ���Ͱ� -> ������Ʈ ������Ʈ �߰� ����! 
        // ������Ʈ -> ��Ʈ�ѷ� -> ĳ���� -> �Ŵ��� ����

        if(isLoading) return;

        //�Ŵ��� �ʱ�ȭ
        InvokeInitializeEvent(ref OnInitializeManager);

        //ĳ���� �ʱ�ȭ
        InvokeInitializeEvent(ref OnInitializeCharacter);

        //��Ʈ�ѷ� �ʱ�ȭ
        InvokeInitializeEvent(ref OnInitializeController);

        //������Ʈ �ʱ�ȭ
        InvokeInitializeEvent(ref OnInitializeObject);

        if (isPlaying)
        {
            float deltaTime = Time.deltaTime;
            //�Ŵ��� ������Ʈ
            OnUpdateEventManager?.Invoke(deltaTime);
            //��Ʈ�ѷ� ������Ʈ        ���� ��Ʈ�ѷ��� �Ǵ��ϰ�
            OnUpdateEventController?.Invoke(deltaTime);
            //ĳ���� ������Ʈ          ĳ���Ͱ� �̸� ���� ��
            OnUpdateEventCharacter?.Invoke(deltaTime);
            //������Ʈ ������Ʈ        ������Ʈ�� �����Ѵ�
            OnUpdateEventObject?.Invoke(deltaTime);
        }


        //������Ʈ ����
        InvokeDestroyEvent(ref OnDestroyEventObject);
        //��Ʈ�ѷ� ����
        InvokeDestroyEvent(ref OnDestroyEventController);
        //ĳ���� ����
        InvokeDestroyEvent(ref OnDestroyEventCharacter);
        //�Ŵ��� ����
        InvokeDestroyEvent(ref OnDestroyEventManager);
    }

    private void FixedUpdate()
    {
        if(isLoading || !isPlaying) return;

        float deltaTime = Time.fixedDeltaTime; //기본값 0.02s

        OnPhysicsCharacter?.Invoke(deltaTime);
        OnPhysicsObject?.Invoke(deltaTime);

    }
}
