using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
        AcademyTrainView trainView = ViewManager.GetView<AcademyTrainView>();
        trainView.homeButtonPressed += OnHomeButtonPressed;
        trainView.trainButtonPressed += OnTrainButtonPressed;
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

    void OnTrainButtonPressed()
    {
        PrepareEnv();
        MainThreadDispatcher.Instance().EnqueueAsync(StartTraining);
    }


    public void PrepareEnv()
    {
        GameObject created_env = _controller.envManager.CreateTrainEnv(
            _controller.academyData.session.selectedTrainEnv
        );

        GameObject created_agent = AgentFactory.instance.CreateAgent(
            _controller.academyData.session.selectedAgent,
            new Vector3(0, 1.54f, -8.5f), Quaternion.identity

        );

        created_env.GetComponent<EnvHallway>().Initialize(
            created_agent.GetComponent<HallwayAgent>()
        );


        _controller.academyData.session.currentAgent = created_agent;
        _controller.academyData.session.currentEnv = created_env;
    }

    async void StartTraining()
    {
        Debug.Log("Starting training Thread");
        RemoteTrainManager.instance.actionsReceived += OnReceivedAgentActions;
        RemoteTrainManager.instance.metricsReceived += OnReceivedTrainMetrics;
        RemoteTrainManager.instance.statusReceived += OnReceivedTrainStatus;

        PostTrainJobRequest trainJobRequest = new PostTrainJobRequest
        {
            agent_config = "agent_config",
            nn_model_config = _controller.academyData.session.selectedAgent.modelConfig.behavior.SerializeToJson(),
            env_config = _controller.academyData.session.selectedTrainEnv.id
        };

        try
        {
            PostResponseTrainJob response = await RemoteTrainManager.instance.StartRemoteTrainning(trainJobRequest);
        }
        catch(Exception e)
        {
            Debug.LogError("Unable to add job to the queue: " + e.Message);
        }
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