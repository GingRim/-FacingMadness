using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;

    public static GameManager Instance => _instance;

    UIManager _ui;
    public UIManager UI =>_ui;

    DataManager _data;
    public DataManager Data => _data;

    SaveManager _save;
    public SaveManager Sace => _save;

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


    }

    

    void InitalizeMangers()
    {
        CreateManager(ref _ui);
        CreateManager(ref _data);
        CreateManager(ref _save);
        CreateManager(ref _setting);
        CreateManager(ref _language);
        CreateManager(ref _audio);
        CreateManager(ref _camera);
        CreateManager(ref _input);
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
            targetVariable = gameObject.AddComponent<ManagerType>();
            targetVariable.Connect(this);
        }
        return targetVariable;
    }

    void Update()
    {
        
    }
}
