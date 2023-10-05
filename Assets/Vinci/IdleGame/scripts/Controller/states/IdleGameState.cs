using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class IdleGameState : StateBase
{
    IdleGameController _controller;

    public IdleGameState(IdleGameController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        Debug.Log("Enter idle state");
        IdleGameMainView mainView = ViewManager.GetView<IdleGameMainView>();
        mainView.academyBtnPressed += OnAcademyBtnPressed;
        mainView.arenaBtnPressed += OnAreanButtonPressed;
        mainView.headquartersBtnPressed += OnHeadquartersBtnPressed;

    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    void OnAreanButtonPressed()
    {
        _controller.SwitchState(new ArenaState(_controller));
    }

    void OnHeadquartersBtnPressed()
    {
        _controller.SwitchState(new HeadquartersState(_controller));
    }

    async void OnAcademyBtnPressed()
    {
        await SceneLoader.instance.LoadScene("Academy");
    }

    async void OnArenaBtnPressed()
    {
        //await SceneLoader.instance.LoadScene("Arena");
    }
}