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


    IEnumerator initializing;

    public static event  InitializeEvent OnInitializeManager     ;
    public static event  InitializeEvent OnInitializeController  ;
    public static event  InitializeEvent OnInitializeCharacter   ;
    public static event  InitializeEvent OnInitializeObject      ;
      
    public static event  UpdateEvent     OnUpdateEventManager    ;
    public static event  UpdateEvent     OnUpdateEventController ;
    public static event  UpdateEvent     OnUpdateEventCharacter  ;
    public static event  UpdateEvent     OnUpdateEventObject     ;
    
    public static event  DestroyEvent    OnDestroyEventManager   ;
    public static event  DestroyEvent    OnDestroyEventController;
    public static event  DestroyEvent    OnDestroyEventCharacter ;
    public static event  DestroyEvent    OnDestroyEventObject    ;

    bool isLoading = true;
    bool isPlaying =  true;

    //Awake     : 이 스크립트가 시작할 때(깨어나서)(아침에 눈을 뜸)
    //OnEnabled : 이 스크립트가 시작할 때(정신 차림 => 준비) -> 여러번 실행도 된다.
    //OnDisabled: 기절
    //Reset     : 일을 시작하기 위해 초기화 준비
    //Start     : 이 스크립트가 시작할 때(하루의 시작)
    void Awake()
    {
        //게임매니저가 일어나 제일 처음에 할 일 둘중에 진정한 게임 매니저를 고른다.
        //먼저온 애를 인정한다.
        //하던 놈을 죽이고 유지하는 것이 더 좋음
        if(Instance  == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
        // 저장했을 때 무었을 할 수 있을까?
        initializing = InitalizeMangers();

        //저장했기 때문에, 이 친구를 "시작"시키거나 "중단"시킬 수 있어요
        //시작을 시키는 것은
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
        yield return new WaitForSeconds(1.0f);
       UIManager.OpenScreenM2(UIType.Title);
        isLoading = false;
    }

    void OnDestroy()
    {
        if(initializing != null) StopCoroutine(initializing);        DeleteManagers();
    }

    void DeleteManagers()
    {
        //유저입력 
        Input?.Disconnect();
        //오브젝트
        ObjectM.Disconnect();
        //오디오
        Audio?.Disconnect();
        //언어
        Language?.Disconnect();
        //세팅
        Setting?.Disconnect();
        //세이브
        Save?.Disconnect();
        //카메라
        Camera?.Disconnect();
        //UI
        UI?.Disconnect();
        //데이터파일
        Data?.Disconnect();
    }
    //달라지는 것이 "자료형"뿐이라면 자료형에 따라 변수로 작용하는 함수를 만들 수 있지 않을까?
    //"Generic Method" => 범용 함수
    //반환값 이름<자료형>(매개변수) where 자료형 : 부모(상속자)

    //_input에다가 값을 넣고싶은데 다른 데에서는 다른 곳에 값을 넣는다. 대상이 되는 변수를 가져오긴 해야 한다.
    //원본 값을 바꿔야 한다. => 원본 값을 "참조"한다. -> 원본 값이랑 연결되는 변수로 만들어 주기! [Reference => ref]

    ManagerType CreateManager<ManagerType>(ref ManagerType targetVariable) where ManagerType : ManagerBase
    {
        if (targetVariable == null)
        {
            targetVariable = this.TryAddComponent<ManagerType>();
        }
        return targetVariable;
    }

    //========================================= 일시 정지
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


    // 모두가 업데이트 하겠다고 한다면 누가 먼저 업데이트 하는 지 모르며 마우스가 갱싱되지 않은 상태에서
    // 총을 쏜다면 프레임 전에 지정한 위치에 쏜다
    // 하스스톤에서 여러번 반복해서 때리는 카드를 보면 이미 죽은 카드들을 또 때리는 것도 볼 수 있다.
    void Update()
    {   // 게임 진행을 할 수 있는지 여부를 조정할 수도 있다.
        // Pause상태다! => 업데이트를 하지 않는다!
        // 매니저 -> 캐릭터 -> 컨트롤러 -> 오브젝트 초기화
        // 매니저 -> 컨트롤러 -> 캐릭터가 -> 오브젝트 업데이트 추가 가능! 
        // 오브젝트 -> 컨트롤러 -> 캐릭터 -> 매니저 제거

        if(isLoading) return;

        //매니저 초기화
        InvokeInitializeEvent(ref OnInitializeManager);

        //캐릭터 초기화
        InvokeInitializeEvent(ref OnInitializeCharacter);

        //컨트롤러 초기화
        InvokeInitializeEvent(ref OnInitializeController);

        //오브젝트 초기화
        InvokeInitializeEvent(ref OnInitializeObject);

        if (isPlaying)
        {
            float deltaTime = Time.deltaTime;
            //매니저 업데이트
            OnUpdateEventManager?.Invoke(deltaTime);
            //컨트롤러 업데이트        먼저 컨트롤러가 판단하고
            OnUpdateEventController?.Invoke(deltaTime);
            //캐릭터 업데이트          캐릭터가 이를 실행 후
            OnUpdateEventCharacter?.Invoke(deltaTime);
            //오브젝트 업데이트        오브젝트가 진행한다
            OnUpdateEventObject?.Invoke(deltaTime);
        }


        //오브젝트 제거
        InvokeDestroyEvent(ref OnDestroyEventObject);
        //컨트롤러 제거
        InvokeDestroyEvent(ref OnDestroyEventController);
        //캐릭터 제거
        InvokeDestroyEvent(ref OnDestroyEventCharacter);
        //매니저 제거
        InvokeDestroyEvent(ref OnDestroyEventManager);
    }


}
