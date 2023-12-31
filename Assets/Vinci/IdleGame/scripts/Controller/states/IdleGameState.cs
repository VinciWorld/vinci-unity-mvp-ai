using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class IdleGameState : StateBase
{
    IdleGameController _controller;
    IdleGameMainView mainView;


    public IdleGameState(IdleGameController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        Debug.Log("Enter Idgle Game state");
        mainView = ViewManager.GetView<IdleGameMainView>();
        ViewManager.Show(mainView);
        mainView.academyBtnPressed += OnAcademyBtnPressed;
        mainView.arenaBtnPressed += OnAreanButtonPressed;
        mainView.headquartersBtnPressed += OnHeadquartersBtnPressed;

        //if(GameManager.instance.playerData.isLoggedIn == false)
        //{
        //    mainView.ShowLoginView();
        //}
    }

    public override void OnExitState()
    {
        mainView.academyBtnPressed -= OnAcademyBtnPressed;
        mainView.arenaBtnPressed -= OnAreanButtonPressed;
        mainView.headquartersBtnPressed -= OnHeadquartersBtnPressed;
    }

    public override void Tick(float deltaTime)
    {

    }

    void OnAreanButtonPressed()
    {
        if(GameManager.instance.playerData.agents.Count == 0)
        {
            mainView.ShowLoaderPopup("Info",
                "Visit the Academy to train and initialize your model.",
                true
            );

        }
        else
        {
            _controller.SwitchState(new ArenaState(_controller));
        }


    }

    void OnHeadquartersBtnPressed()
    {
        _controller.SwitchState(new HeadquartersState(_controller));
    }

    void OnAcademyBtnPressed()
    {
        Debug.Log("Academy");
        SceneLoader.instance.LoadSceneDelay("Academy");
    }

    async void OnArenaBtnPressed()
    {
        //await SceneLoader.instance.LoadScene("Arena");
    }
}