using System;
using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class LoginState : StateBase
{
    IdleGameController _controller;
    LoginView loginView;


    public LoginState(IdleGameController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        loginView = ViewManager.GetView<LoginView>();
        loginView.walletConnected += OnwalletConnected;
        loginView.loggedIn += OnLoggedIn;

        if (!GameManager.instance.isLoggedIn)
        {
            ViewManager.Show(loginView);
        }
    }


    public override void Tick(float deltaTime)
    {

    }

    private void OnLoggedIn()
    {
        _controller.SwitchState(new IdleGameState(_controller));
    }


    private void OnwalletConnected()
    {
        _controller.SwitchState(new IdleGameState(_controller));
    }


    public override void OnExitState()
    {
        loginView.walletConnected -= OnwalletConnected;
    }

}