using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Security.Authentication;
using System.Threading.Tasks;
using UnityEngine;

public class DBManager : ManagerBase
{
    private object user;
    private object DBRefence;

    protected override IEnumerator OnConnected(GameManager newManager)
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(InitializeFireBase);
        yield return null;
    }

    private void InitializeFireBase(Task<DependencyStatus> task)
    {
        if (task.Result == DependencyStatus.Available)
        {
            authentication = FirebaseAuth.DefaultInstance;

            user = authentication.CurrentUser;

            DBRefence = FirebaseDatabase.DefaultInstance.RootReference;

            Debug.Log("Firebase Initialized");
        }
        else
        {
            Debug.Log($"Fail to Initialize Firebase : {task.Exception}");
        }
    }

    protected override void OnDisconnected()
    {
        throw new System.NotImplementedException();
    }

    public void GuestLogin()
    {
        if (authentication is null) return;

        if(user is not null)
        {
            Debug.LogError("Login");
        }

        authentication.SignInAnonymouslyAsync().ContinueWith(OnLoginResult);
    }

    void OnLoginResult(Task<AuthResult> task)
    {
        if(task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError($"Fail to Sign in : {task.Exception}");
            return;
        }

        user = task.Result.User;
        Debug.LogError("zz");
    }
}
