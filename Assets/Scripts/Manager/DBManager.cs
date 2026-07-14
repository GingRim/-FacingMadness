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

    public void GuestLogin()
    {
        if (authentication is null) return;

        if(user is not null)
        {
            Debug.LogError("Login");
            WriteData(MakeNewUserData("제로"), "users", "userData", user.UserId);
            return;
        }

        authentication.SignInAnonymouslyAsync().ContinueWithOnMainThread(OnLoginResult);
    }

    void OnLoginResult(Task<AuthResult> task)
    {
        if(task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError($"Fail to Sign in : {task.Exception}");
            return;
        }

        user = task.Result.User;
        WriteData(MakeNewUserData("제로"), "users", "userData" );
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

    public void WriteData(object wantData, params string[] directory)
    {
        if (rootDB is null || wantData is null) return;

        string jsonData = JsonUtility.ToJson(wantData);
        DatabaseReference currentRegerence = rootDB;
        foreach (string currentChild in directory) 
        {
            currentRegerence = currentRegerence.Child(currentChild);
        }

        currentRegerence.SetRawJsonValueAsync(jsonData).ContinueWithOnMainThread(OnTaskResult);
        

        Dictionary<string, object> item = new()
        {
            {"name", "돌" },
            //무개
            {"weight", 0.3 },
            {"price", 1 }
        };

        rootDB.Child("Items").Child("Misc").Child("Nature").Child("Stone")
            .UpdateChildrenAsync(item).ContinueWithOnMainThread(OnTaskResult);

    }

    void OnTaskResult(Task task)
    {
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.Log(task.Exception);
        }
    }
}
