using System;
using System.Collections;
using System.Collections.Generic;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using TMPro;
using UnityEngine;
using Vinci.Core.Managers;

public class MainTopBar : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI pubkey;
    [SerializeField]
    TextMeshProUGUI solanaBalance;
    [SerializeField]
    TextMeshProUGUI stepsAvailable;

    void OnEnable()
    {
        Web3.OnLogin += OnLogin;
        Web3.OnBalanceChange += OneBalanceChange;

        if(GameManager.instance != null)
        {
            pubkey.text = GameManager.instance.UserData.pubkey;
            solanaBalance.text = GameManager.instance.solanaBalance.ToString("f5");
        }

    }

    private void OnDisable() {
        Web3.OnLogin -= OnLogin;
        Web3.OnBalanceChange -= OneBalanceChange;
    }

    private void OneBalanceChange(double sol)
    {
        solanaBalance.text = sol.ToString();
    }

    private void OnLogin(Account account)
    {
        pubkey.text = account.PublicKey;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
