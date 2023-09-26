using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class HeadquartersState : StateBase
{
    IdleGameController _controller;

    public HeadquartersState(IdleGameController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        HeadquartersView mainView = ViewManager.GetView<HeadquartersView>();
        ViewManager.Show(mainView);


    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    async void OnAcademyBtnPressed()
    {
        await SceneLoader.instance.LoadScene("Academy");
    }

    async void OnArenaBtnPressed()
    {
        await SceneLoader.instance.LoadScene("Arena");
    }
}