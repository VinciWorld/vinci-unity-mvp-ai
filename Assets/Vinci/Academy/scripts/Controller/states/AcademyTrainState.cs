using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Unity.Barracuda;
using UnityEngine;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyTrainState : StateBase
{
    AcademyController _controller;
    AcademyTrainView trainView;
    public AcademyTrainState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        Debug.Log("AcademyTrainView");
        trainView = ViewManager.GetView<AcademyTrainView>();
        ViewManager.Show<AcademyTrainView>();
        trainView.homeButtonPressed += OnHomeButtonPressed;
        trainView.trainButtonPressed += OnTrainButtonPressed;

        trainView.SetTrainSetupSubViewState(true);
        trainView.SetTrainHudSubViewState(false);
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
        MainThreadDispatcher.Instance().EnqueueAsync(ConnectToRemoteInstance);

        trainView.SetTrainSetupSubViewState(false);
        trainView.SetTrainHudSubViewState(true);
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


        _controller.academyData.session.currentAgentInstance = created_agent;
        _controller.academyData.session.currentEnvInstanc = created_env;
    }

    async void ConnectToRemoteInstance()
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

            switch (response.job_status)
            {
                case TrainJobStatus.SUBMITTED:
                case TrainJobStatus.RETRIEVED:
                case TrainJobStatus.STARTING:
                case TrainJobStatus.RUNNING:
                    RemoteTrainManager.instance.ConnectWebSocketCentralNodeClientStream();
                    break;
                case TrainJobStatus.SUCCEEDED:
                    LoadTrainedModel(response.run_id);
                    break;

                default:
                    Debug.Log("status not recognised");
                    break;
            }

            Debug.Log("response: " + response + " run_id: " + response.run_id);
        }
        catch(Exception e)
        {
            Debug.LogError("Unable to add job to the queue: " + e.Message);
        }
    }

    async private void  LoadTrainedModel(string runId)
    {
        NNModel nnModel =  await RemoteTrainManager.instance.DownloadNNModel(runId);


    }

    void OnReceivedAgentActions(Actions actions)
    {
        Debug.Log("Actions received: " + actions.data);
    }

    void OnReceivedTrainMetrics(MetricsMsg metrics)
    {
        Debug.Log("Metrics received: " + metrics.Step);
    }

    void OnReceivedTrainStatus(string status)
    {
        Debug.Log("Status received: " + status);
    }
}