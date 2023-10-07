using System;
using System.IO;
using Newtonsoft.Json;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using Unity.MLAgents;
using UnityEngine;
using Vinci.Core.Managers;
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
        trainView = ViewManager.GetView<AcademyTrainView>();

        ViewManager.Show<AcademyTrainView>();
        trainView.homeButtonPressed += OnHomeButtonPressed;
        trainView.trainButtonPressed += OnTrainButtonPressed;

        trainView.SetTrainSetupSubViewState(true);
        trainView.SetTrainHudSubViewState(false);

        RemoteTrainManager.instance.websocketOpen += OnWebSocketOpen;
        RemoteTrainManager.instance.actionsReceived += OnReceivedAgentActions;
        RemoteTrainManager.instance.metricsReceived += OnReceivedTrainMetrics;
        RemoteTrainManager.instance.statusReceived += OnReceivedTrainStatus;
        RemoteTrainManager.instance.binaryDataReceived += OnBinaryDataRecived;

        Academy.Instance.AutomaticSteppingEnabled = false;
    }

    public override void OnExitState()
    {
        GameManager.instance.SavePlayerData();

        trainView.homeButtonPressed -= OnHomeButtonPressed;
        trainView.trainButtonPressed -= OnTrainButtonPressed;
    
        RemoteTrainManager.instance.websocketOpen -= OnWebSocketOpen;
        RemoteTrainManager.instance.actionsReceived -= OnReceivedAgentActions;
        RemoteTrainManager.instance.metricsReceived -= OnReceivedTrainMetrics;
        RemoteTrainManager.instance.statusReceived -= OnReceivedTrainStatus;
        RemoteTrainManager.instance.binaryDataReceived -= OnBinaryDataRecived;

        _controller.session.currentEnvInstance.episodeAndStepCountUpdated -= trainView.UptadeInfo;
        _controller.session.currentEnvInstance.SetIsReplay(false);

    }

    public override void Tick(float deltaTime)
    {

    }

    void OnTrainButtonPressed(int steps)
    {
        //TODO: Check if model is already trained or if it is trainning

        if(_controller.session.currentEnvInstance == null)
        {
            PrepareEnv();
        }

        _controller.session.selectedAgent.modelConfig.behavior.steps = steps;
        _controller.session.selectedAgent.modelConfig.AddStepsTrained(steps);

        _controller.session.currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        _controller.session.currentEnvInstance.episodeAndStepCountUpdated += trainView.UptadeInfo;
        _controller.session.currentEnvInstance.SetIsReplay(true);

        if (!RemoteTrainManager.instance.isConnected)
        {
            MainThreadDispatcher.Instance().EnqueueAsync(ConnectToRemoteInstance);
        }

        trainView.UptadeInfo(0, 0);
        trainView.UpdateMetrics(0,0);
        trainView.SetTrainSetupSubViewState(false);
        trainView.SetTrainHudSubViewState(true);
    }

    void OnReceivedTrainStatus(TrainJobStatusMsg trainJobStatus)
    {
        if(trainJobStatus.status == TrainJobStatus.SUCCEEDED)
        {
            try
            {
                trainView.SetTrainSetupSubViewState(false);
                trainView.SetTrainHudSubViewState(false);
    
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to save and Load model: " + e.Message);
            }
        }
        else if(trainJobStatus.status == TrainJobStatus.FAILED)
        {
            Debug.Log("JOB FAILED");
            _controller.SwitchState(new AcademyTrainState(_controller));
        }

        Debug.Log("Status received: " + trainJobStatus.status);
    }

    public void PrepareEnv()
    {
        EnvironementBase created_env = _controller.envManager.CreateTrainEnv(
            _controller.session.selectedTrainEnv
        );


        GameObject created_agent = AgentFactory.instance.CreateAgent(
            _controller.session.selectedAgent,
            new Vector3(0, 1.54f, -8.5f), Quaternion.identity,
            created_env.transform
        );

        created_env.Initialize(created_agent);

        _controller.session.currentAgentInstance = created_agent;
        _controller.session.currentEnvInstance = created_env;
    }

    async void ConnectToRemoteInstance()
    {
        Debug.Log("Connecting to remote training Server");

        PostTrainJobRequest trainJobRequest = new PostTrainJobRequest
        {
            agent_config = "agent_config",
            nn_model_config = new BehaviorConfigSmall
            {
                steps = _controller.session.selectedAgent.modelConfig.behavior.steps,
                behavior_name = _controller.session.selectedAgent.modelConfig.behavior.behavior_name
            },
            env_config = new EnvConfigSmall
            {
                env_id = _controller.session.selectedTrainEnv.env_id,
                num_of_areas = _controller.session.selectedTrainEnv.num_of_areas,
                agent_id = _controller.session.selectedAgent.id
            }
        };

        try
        {
            PostResponseTrainJob response = await RemoteTrainManager.instance.StartRemoteTrainning(trainJobRequest);

            _controller.session.selectedAgent.SetRunID(response.run_id);
           
            RemoteTrainManager.instance.ConnectWebSocketCentralNodeClientStream();

            switch (response.job_status)
            {
                case TrainJobStatus.SUBMITTED:
                case TrainJobStatus.RETRIEVED:
                case TrainJobStatus.STARTING:
                case TrainJobStatus.RUNNING:
                    break;
                case TrainJobStatus.SUCCEEDED:
                    break;

                default:
                    Debug.Log("status not recognised");
                    break;
            }

            Debug.Log("response: " + response + " run_id: " + response.run_id);
        }
        catch(Exception e)
        {
            trainView.SetTrainSetupSubViewState(true);
            trainView.SetTrainHudSubViewState(false);
            Debug.LogError("Unable to add job to the queue: " + e.Message);
        }
    }

    void OnWebSocketOpen()
    {
        string run_id = _controller.session.selectedAgent.GetModelRunID();
        RunId data = new RunId { run_id = run_id };

        string json = JsonConvert.SerializeObject(data);
        RemoteTrainManager.instance.SendWebSocketJson(json);
    }

    void OnBinaryDataRecived(byte[] data)
    {
        SaveAndLoadModel(data);

        RemoteTrainManager.instance.CloseWebSocketConnection();
        _controller.session.currentEnvInstance.StopReplay();
        _controller.SwitchState(new AcademyResultsState(_controller));
    }

    void OnReceivedTrainMetrics(MetricsMsg metrics)
    {
        _controller.session.selectedAgent.AddTrainMetrics(metrics.mean_reward, metrics.mean_reward);

        trainView.UpdateMetrics(
            metrics.mean_reward,
            metrics.std_reward
        );
    }

    void OnReceivedAgentActions(string actionsJson)
    {
        _controller.session.currentEnvInstance.OnActionsFromServerReceived(actionsJson);

        //Debug.Log("Actions received: " + actionsJson);
    }

    void OnHomeButtonPressed()
    {
        SceneLoader.instance.LoadSceneDelay("IdleGame");
    }

    void SaveAndLoadModel(Byte[] rawModel)
    {
        string runId = _controller.session.selectedAgent.GetModelRunID();

        string behaviourName = _controller.session.selectedAgent.modelConfig.behavior.behavior_name;
        string directoryPath = Path.Combine(Application.persistentDataPath, "runs", runId, "models");
        string filePath = Path.Combine(directoryPath, $"{behaviourName}.onnx");

        Debug.Log("Model saved at: " + filePath);
        // Ensure the directory exists
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Convert and Save model to disk
        NNModel nnModel = SaveAndConvertModel(filePath, rawModel);
        nnModel.name = behaviourName;

        _controller.session.selectedAgent.SetModelAndPath(filePath, nnModel);

        // Load model on current agent
        _controller.session
        .currentAgentInstance.GetComponent<HallwayAgent>()
        .LoadModel(behaviourName, nnModel);
    }

    NNModel SaveAndConvertModel(string filePath, byte[] rawModel)
    {
        //Save model to disk
        File.WriteAllBytes(filePath, rawModel);

        var converter = new ONNXModelConverter(true);
        var onnxModel = converter.Convert(rawModel);

        NNModelData assetData = ScriptableObject.CreateInstance<NNModelData>();
        using (var memoryStream = new MemoryStream())
        using (var writer = new BinaryWriter(memoryStream))
        {
            ModelWriter.Save(writer, onnxModel);
            assetData.Value = memoryStream.ToArray();
        }
        assetData.name = "Data";
        assetData.hideFlags = HideFlags.HideInHierarchy;

        var asset = ScriptableObject.CreateInstance<NNModel>();
        asset.modelData = assetData;

        return asset;
    }

    /*
    public void CreateAgent()
    {
        EnvironementBase envSelected = _controller.session.currentEnvInstance;

        GameObject created_agent = AgentFactory.instance.CreateAgent(
            _controller.session.selectedAgent,
            new Vector3(0, 1.54f, -8.5f), Quaternion.identity,
            envSelected.transform
        );

        envSelected.GetComponent<EnvHallway>().Initialize(
            created_agent.GetComponent<HallwayAgent>()
        );

        _controller.session.currentAgentInstance = created_agent;
    }
*/
}