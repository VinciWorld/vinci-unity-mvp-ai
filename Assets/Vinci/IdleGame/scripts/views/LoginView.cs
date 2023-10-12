using System;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class LoginView : View
{
    [SerializeField]
    private Button _connectWallet;

    public event Action connectWallet;

    public override void Initialize()
    {
        _connectWallet.onClick.AddListener(OnConnectWalletPressed);

        if (Application.platform is RuntimePlatform.LinuxEditor or RuntimePlatform.WindowsEditor or RuntimePlatform.OSXEditor)
        {
            _connectWallet.onClick.RemoveListener(OnConnectWalletPressed);
            _connectWallet.onClick.AddListener(() => 
            {
                Debug.LogWarning("Wallet adapter login is not yet supported in the editor");
                ViewManager.Show<IdleGameMainView>();
            });
               
        }
    }

    async void OnConnectWalletPressed()
    {
        Debug.Log("Cooonect");
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
            
            ViewManager.Show<IdleGameMainView>();
        }
        else
        {
            Debug.LogError("Unable to connect wallet");
        }
    }
}
