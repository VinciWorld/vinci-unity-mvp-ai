using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyMainState : StateBase
{
    AcademyController _controller;
    AcademyMainView _mainView;

    public AcademyMainState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        _mainView = ViewManager.GetView<AcademyMainView>();
        ViewManager.Show<AcademyMainView>();

        _mainView.homeButtonPressed += OnHomeButtonPressed;
        _mainView.selectAgentButtonPressed += OnSelectAgentButtonPressed;
        _mainView.createAgentButtonPressed += OnCreateAgent;
        _controller.session.selectedAgent = null;

        //TODO: Load available models for this model
    }

    public override void OnExitState()
    {
        GameManager.instance.SavePlayerData();
        _mainView.homeButtonPressed -= OnHomeButtonPressed;
        _mainView.selectAgentButtonPressed -= OnSelectAgentButtonPressed;
        _mainView.createAgentButtonPressed -= OnCreateAgent;
    }

    public override void Tick(float deltaTime)
    {

    }

    public void OnCreateAgent()
    {
        if (_controller.manager.playerData.agents.Count == 0)
        {
            //TODO: Clone agent! This will be done in the create step!
            _controller.manager.playerData.AddAgent(_controller.academyData.availableAgents[0]);
        }
        else
        {
            Debug.Log("Agent already created");
        }

        _controller.session.selectedAgent = _controller.manager.playerData.GetAgent(0);
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        GameManager.instance.SavePlayerData();

        _controller.SwitchState(new AcademyTrainState(_controller));

    }

    void OnHomeButtonPressed()
    {
        GameManager.instance.SavePlayerData();
        SceneLoader.instance.LoadSceneDelay("IdleGame");
    }

    void OnSelectAgentButtonPressed()
    {
        _controller.session.selectedAgent = _controller.manager.playerData.GetAgent(0);
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
    }
}