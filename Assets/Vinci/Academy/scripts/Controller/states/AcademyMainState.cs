using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyMainState : StateBase
{
    AcademyController _controller;

    public AcademyMainState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        Debug.Log(ViewManager.instance);
        AcademyMainView mainView = ViewManager.GetView<AcademyMainView>();
        Debug.Log(mainView);
        mainView.homeButtonPressed += OnHomeButtonPressed;
    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    async void OnHomeButtonPressed()
    {
        await SceneLoader.instance.LoadScene("IdleGame");
    }
}