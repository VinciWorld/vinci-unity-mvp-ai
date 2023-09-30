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
        AcademyMainView mainView = ViewManager.GetView<AcademyMainView>();
        mainView.homeButtonPressed += OnHomeButtonPressed;
        mainView.selectAgentButtonPressed += OnSelectAgentButtonPressed;
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

    void OnSelectAgentButtonPressed()
    {
        _controller.academyData.session.selectedAgent = _controller.academyData.availableAgents[0];
        _controller.academyData.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
    }


}