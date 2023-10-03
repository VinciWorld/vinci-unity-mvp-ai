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

        //TODO: Clone agent! This will be done in the create step!
        _controller.manager.playerData.AddAgent(_controller.academyData.availableAgents[0]);
    }

    public override void OnExitState()
    {
        GameManager.instance.SavePlayerData();
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
        _controller.session.selectedAgent = _controller.manager.playerData.currentAgentConfig;
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
    }
}