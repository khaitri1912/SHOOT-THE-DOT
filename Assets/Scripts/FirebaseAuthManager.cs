using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth _auth;
    private FirebaseUser _currentUser;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            _auth = FirebaseAuth.DefaultInstance;
        });
    }

    public void RegisterNewUser(string email, string password)
    {
        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Dang ky that bai:" + task.Exception.ToString());
                return;
            }

            _currentUser = task.Result.User ;
            Debug.Log("Dang ky thanh cong nguoi choi: " + _currentUser.Email);
        }); 
    }

    public void LoginUser(string email, string password)
    {
        _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError($"{task.Exception.ToString()}");
            }
            else if (task.IsCompleted)
            {
                _currentUser = task.Result.User;
                Debug.Log("Dang nhap thanh cong: " + _currentUser.Email);
            }
        });
    }

    public void SignOutUser()
    {
        _auth.SignOut();
        Debug.Log("Dang xuat thanh cong");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
