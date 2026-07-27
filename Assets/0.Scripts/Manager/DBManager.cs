using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DBManager : ManagerBase
{
    private FirebaseAuth authentication;
    private FirebaseUser user;
    private DatabaseReference rootDB;
    public TMPro.TMP_InputField nickNameInput;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(InitializeFireBase);
        yield return null;
    }

    void InitializeFireBase(Task<DependencyStatus> task)
    {
        if (task.Result == DependencyStatus.Available)
        {
            authentication = FirebaseAuth.DefaultInstance;

            user = authentication.CurrentUser;

            rootDB = FirebaseDatabase.DefaultInstance.RootReference;

            GuestLogin();

            Debug.Log("Firebase Initialized");
            
        }
        else
        {
            Debug.Log($"Fail to Initialize Firebase : {task.Exception}");
        }
    }

    protected override void OnDisconnected()
    {

    }

    public void MakeUserData()
    {
        WriteData(MakeNewUserData(nickNameInput.text), "users", "userData", user.UserId);
    }

    public async void GuestLogin()
    {
        if (authentication is null) return;

        if (user is not null)
        {
            Debug.Log($"Login Failed : Already Has Login Data ({user.IsValid()}, {user.UserId})");
            UserData resultData = await ReadDataAsync<UserData>("users", "userData", user.UserId);
            if (resultData is not null)
            {
                Debug.Log(resultData.nickname);
            }
            else
            {
                WriteData(MakeNewUserData("제로"), "users", "userData", user.UserId);
            }
            return;
        }
           await authentication.SignInAnonymouslyAsync().ContinueWithOnMainThread(OnLoginResult);
    }

    void OnLoginResult(Task<AuthResult> task)
    {
        if(task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError($"Fail to Sign in : {task.Exception}");
            return;
        }

        user = task.Result.User;
        WriteData(MakeNewUserData("제로"), "users", "userData", user.UserId);
        Debug.LogError("ㅗㅅ오ㅅㅗ");
    }

    [Serializable]
    public class UserData
    {
        public string nickname;
        public DateTime assignDate;
        public int userLevel;
        public int miney;
        public int attendtime;
    }
    UserData MakeNewUserData(string wantNickname) => new()
    {
        nickname = wantNickname,
        assignDate = DateTime.Now,
        userLevel = 1,
        miney = 1,
        attendtime = 0
    };
    public DatabaseReference GetFinalDirectory(DatabaseReference root, params string[] directory)
    {
        if (directory is null || directory.Length == 0) return root;
        DatabaseReference currentRegerence = rootDB;
        foreach (string currentChild in directory)
        {
            currentRegerence = currentRegerence.Child(currentChild);
        }


        return currentRegerence;
    }

    public void WriteData(object wantData, params string[] directory)
    {
        if (rootDB is null || wantData is null) return;

        string jsonData = JsonUtility.ToJson(wantData);
        GetFinalDirectory(rootDB, directory).SetRawJsonValueAsync(jsonData).ContinueWithOnMainThread(OnTaskResult);

    }

    public void WiteData(Dictionary<string, object> changes, params string[] directory)
    {
        if (rootDB is null || changes is null) return;

        GetFinalDirectory(rootDB, directory).UpdateChildrenAsync(changes).ContinueWithOnMainThread(OnTaskResult);
    }

    public void ReadDate(Action<Task<DataSnapshot>> OnReadData, params string[] directory)
    {
        GetFinalDirectory(rootDB, directory).GetValueAsync().ContinueWithOnMainThread(OnReadData);


    }

    public IEnumerator ReadData(Action<Task<DataSnapshot>> OnReadData, params string[] directory)
    {
        Task<DataSnapshot> readTask = GetFinalDirectory(rootDB, directory).GetValueAsync();
        yield return readTask.WaitForTask();
        OnReadData?.Invoke(readTask);
    }

    public async Task<T> ReadDataAsync<T>(params string[] directory)
    {
        DataSnapshot currentTask = await GetFinalDirectory(rootDB, directory).GetValueAsync();

        if (currentTask is null) return default;
        if (!currentTask.Exists) return default;

        //복합타입
        try
        {
            if (currentTask.HasChildren)
            {
                return JsonUtility.FromJson<T>(currentTask.GetRawJsonValue());
            }

            //단일 타입
            //diyble 8바이트 소수점이 있는 숫자
            //float  4바이트 소수점이 있는 숫자
            return (T)System.Convert.ChangeType(currentTask.Value, typeof(T));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return default;
        }
    }

    void OnTaskResult(Task task)
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.Log(task.Exception);
        }
    }

}
