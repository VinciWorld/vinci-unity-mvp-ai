using System;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.Managers;
using Vinci.Core.UI;

public class LoginView : View
{
    [SerializeField]
    private Button _connectWallet;

    [SerializeField]
    TextMeshProUGUI plyerUsername;
    [SerializeField]
    Image userAvatarImage;

    public event Action connectWallet;

    public event Action walletConnected;
    public event Action loggedIn;

    public override void Initialize()
    {
        _connectWallet.onClick.AddListener(OnConnectWalletPressed);

        if (Application.platform is RuntimePlatform.LinuxEditor or RuntimePlatform.WindowsEditor or RuntimePlatform.OSXEditor)
        {
            _connectWallet.onClick.RemoveListener(OnConnectWalletPressed);
            _connectWallet.onClick.AddListener(() =>
            {
                GameManager.instance.isLoggedIn = true;
                Debug.LogWarning("Wallet adapter login is not yet supported in the editor");


                UserUpdate userDataUpdate = new UserUpdate();
                userDataUpdate.pubkey = "Teste unity";

                RemoteTrainManager.instance.LoginCentralNode(userDataUpdate, OnUserDataReceived);
            });

        }
        //Debug.Log("Show: " + GameManager.instance.isLoggedIn);
    }

    public override void Show()
    {
        if (GameManager.instance.isLoggedIn)
        {
            loggedIn?.Invoke();
        }
        else
        {
            base.Show();
        }
    }

    public override void Hide()
    {
        //_connectWallet.onClick.RemoveAllListeners();

        base.Hide();
    }

    async void OnConnectWalletPressed()
    {
        connectWallet?.Invoke();

        if (Web3.Instance == null) return;
        var account = await Web3.Instance.LoginWalletAdapter();
        CheckAccount(account);
    }

    private void CheckAccount(Account account)
    {
        if (account != null)
        {
            Debug.Log(account.PublicKey);
            GameManager.instance.isLoggedIn = true;

            UserUpdate userDataUpdate = new UserUpdate();
            userDataUpdate.pubkey = account.PublicKey;

            RemoteTrainManager.instance.LoginCentralNode(userDataUpdate, OnUserDataReceived);
        }
        else
        {
            Debug.LogError("Unable to connect wallet");
        }
    }

    private void OnUserDataReceived(UserData userData, Sprite userAvatar)
    {
        Debug.Log("Received data from central node!");
        GameManager.instance.UserData = userData;

        plyerUsername.text = userData.username;
        if(userAvatar != null)
        {
            userAvatarImage.sprite = userAvatar;
        }

        walletConnected?.Invoke();
    }
}
