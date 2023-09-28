using UnityEngine;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyTrainState : StateBase
{
    AcademyController _controller;

    public AcademyTrainState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        AcademyTrainView mainView = ViewManager.GetView<AcademyTrainView>();
        mainView.trainButtonPressed += OnHomeButtonPressed;

        PrepareEnv();
        StartTrainning();
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

    public void PrepareEnv()
    {
        _controller.envManager.CreateTrainEnv(_controller.academyData.session.selectedTrainEnv);

        _controller.academyData.session.currentAgent = AgentFactory.instance.CreateAgent(
            _controller.academyData.session.selectedAgent,
            Vector3.zero, Quaternion.identity

        );
    }

    async void StartTrainning()
    {
        RemoteTrainManager.instance.actionsReceived += OnReceivedAgentActions;
        RemoteTrainManager.instance.metricsReceived += OnReceivedTrainMetrics;
        RemoteTrainManager.instance.statusReceived += OnReceivedTrainStatus;

        PostTrainJobRequest trainJobRequest = new PostTrainJobRequest
        {
            agent_config = "agent_config",
            nn_model_config = _controller.academyData.session.selectedAgent.modelConfig.behavior.SerializeToJson(),
            env_config = _controller.academyData.session.selectedTrainEnv.id
        };

        PostResponseTrainJob response = await RemoteTrainManager.instance.StartRemoteTrainning(trainJobRequest);
    }

    void OnReceivedAgentActions(Actions actions)
    {
        Debug.Log("Actions received: " + actions.data);
    }

    void OnReceivedTrainMetrics(Metrics metrics)
    {
        Debug.Log("Metrics received: " + metrics.Step);
    }

    void OnReceivedTrainStatus(string status)
    {
        Debug.Log("Status received: " + status);
    }
}