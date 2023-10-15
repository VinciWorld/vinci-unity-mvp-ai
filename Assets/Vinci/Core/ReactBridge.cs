using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Vinci.Core.Utils;

public class ReactBridge : PersistentSingleton<ReactBridge>
{

    //[DllImport("__Internal")]
    //private static extern void GameOver(string userName, int score);

    public event Action<string> receivedUserJwt;

    public void UserJwt(string token)
    {
        receivedUserJwt?.Invoke(token);
        Debug.Log(token);

        RemoteTrainManager.instance.SetJwtToken(token);
    }

    public void SendGameOver()
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false
    //GameOver ("Player1", 100);
#endif
    }
}
