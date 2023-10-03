using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyTrainState : StateBase
{
    AcademyController _controller;
    AcademyTrainView trainView;

    bool isTrainJobComplete = false;
    bool isModelLoaded = false;

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
        //TODO: Check if model is already trained or if it is trainning

        if(!RemoteTrainManager.instance.isConnected)
        {
            MainThreadDispatcher.Instance().EnqueueAsync(ConnectToRemoteInstance);
        }

        PrepareEnv();

        trainView.SetTrainSetupSubViewState(false);
        trainView.SetTrainHudSubViewState(true);
    }

    void OnReceivedTrainStatus(TrainJobStatusMsg trainJobStatus)
    {
        if( trainJobStatus.status == TrainJobStatus.SUCCEEDED)
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

        Debug.Log("Status received: " + trainJobStatus.status);
    }

    public void PrepareEnv()
    {
        GameObject created_env = _controller.envManager.CreateTrainEnv(
            _controller.session.selectedTrainEnv
        );

        _controller.session.currentEnvInstance = created_env;
    }

    public void CreateAgent()
    {
        GameObject envSelected = _controller.session.currentEnvInstance;

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

    async void ConnectToRemoteInstance()
    {
        Debug.Log("Starting remote training Thread");
        RemoteTrainManager.instance.websocketOpen += OnWebSocketOpen;
        RemoteTrainManager.instance.actionsReceived += OnReceivedAgentActions;
        RemoteTrainManager.instance.metricsReceived += OnReceivedTrainMetrics;
        RemoteTrainManager.instance.statusReceived += OnReceivedTrainStatus;
        RemoteTrainManager.instance.binaryDataReceived += OnBinaryDataRecived;

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
                env_id = _controller.session.selectedTrainEnv.env_id
            }
        };

        try
        {
            PostResponseTrainJob response = await RemoteTrainManager.instance.StartRemoteTrainning(trainJobRequest);

            _controller.manager.playerData.currentAgentConfig.SetRunID(response.run_id);
            CreateAgent();
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
        string run_id = _controller.manager.playerData.currentAgentConfig.GetModelRunID();
        RunId data = new RunId { run_id = run_id };

        string json = JsonConvert.SerializeObject(data);
        RemoteTrainManager.instance.SendWebSocketJson(json);
    }

    void OnBinaryDataRecived(byte[] data)
    {
        SaveAndLoadModel(data);
        RemoteTrainManager.instance.CloseWebSocketConnection();
    }

    void OnReceivedTrainMetrics(MetricsMsg metrics)
    {
        trainView.UpdateMetrics(
            metrics.MeanReward,
            metrics.StdOfReward
        );

        Debug.Log("Metrics received: " + metrics);
    }

    void OnReceivedAgentActions(string actionsJson)
    {
        _controller.session.currentAgentInstance.GetComponent<HallwayAgent>().UpdateActions(actionsJson);

        TrainInfo trainInfo = JsonUtility.FromJson<TrainInfo>(actionsJson);
        trainView.UptadeInfo(trainInfo.episodeCount, trainInfo.stepCount);

        Debug.Log("Actions received: " + actionsJson);
    }

    void SaveAndLoadModel(Byte[] rawModel)
    {
        string runId = _controller.session.selectedAgent.GetModelRunID();

        string behaviourName = GameManager.instance.playerData.currentAgentConfig.modelConfig.behavior.behavior_name;
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

        GameManager.instance.playerData.SetModelAndPath(filePath, nnModel);

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
}