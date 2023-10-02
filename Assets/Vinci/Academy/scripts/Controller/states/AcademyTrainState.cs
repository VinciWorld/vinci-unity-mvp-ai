using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            new Vector3(0, 1.54f, -8.5f), Quaternion.identity,
            created_env.transform
        );

        created_env.GetComponent<EnvHallway>().Initialize(
            created_agent.GetComponent<HallwayAgent>()
        );


        _controller.academyData.session.currentAgentInstance = created_agent;
        _controller.academyData.session.currentEnvInstance = created_env;
    }

    async void ConnectToRemoteInstance()
    {
        Debug.Log("Starting training Thread");
        RemoteTrainManager.instance.websocketOpen += OnWebSocketOpen;
        RemoteTrainManager.instance.actionsReceived += OnReceivedAgentActions;
        RemoteTrainManager.instance.metricsReceived += OnReceivedTrainMetrics;
        RemoteTrainManager.instance.statusReceived += OnReceivedTrainStatus;

        PostTrainJobRequest trainJobRequest = new PostTrainJobRequest
        {
            agent_config = "agent_config",
            nn_model_config = new TrainJobNNModelConfig
            {
                steps = 10000
            },
            env_config = new TrainJobEnvConfig
            {
                env_id = _controller.academyData.session.selectedTrainEnv.env_id
            }
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
                    _controller.manager.playerData.currentAgentConfig.SetRunID(response.run_id);
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

    void OnWebSocketOpen()
    {
        string run_id = _controller.manager.playerData.currentAgentConfig.GetModelRunID();
        RunId data = new RunId { run_id = run_id };

        string json = JsonConvert.SerializeObject(data);
        RemoteTrainManager.instance.SendWebSocketJson(json);
    }

    void OnReceivedAgentActions(string actionsJson)
    {
        _controller.academyData.session.currentAgentInstance.GetComponent<HallwayAgent>().UpdateActions(actionsJson);

        TrainInfo trainInfo = JsonUtility.FromJson<TrainInfo>(actionsJson);
        trainView.UptadeInfo(trainInfo.episodeCount, trainInfo.stepCount);

        Debug.Log("Actions received: " + actionsJson);
    }

    void OnReceivedTrainMetrics(MetricsMsg metrics)
    {
        trainView.UpdateMetrics(
            metrics.MeanReward,
            metrics.MeanReward,
            metrics.StdOfReward
        );

        Debug.Log("Metrics received: " + metrics);
    }

    void OnReceivedTrainStatus(string status)
    {
        Debug.Log("Status received: " + status);
    }
}