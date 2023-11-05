using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.MLAgents;
using Unity.Sentis;
using Unity.VisualScripting;
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
        trainView.backButtonPressed += OnBackButtonPressed;
        trainView.trainButtonPressed += OnTrainButtonPressed;
        trainView.watchTrainButtonPressed += OnWatchButtonPressed;



        RemoteTrainManager.instance.websocketOpen += OnWebSocketOpen;
        RemoteTrainManager.instance.actionsReceived += OnReceivedAgentActions;
        RemoteTrainManager.instance.metricsReceived += OnReceivedTrainMetrics;
        RemoteTrainManager.instance.statusReceived += OnReceivedTrainStatus;
        RemoteTrainManager.instance.binaryDataReceived += OnBinaryDataRecived;

        Academy.Instance.AutomaticSteppingEnabled = false;

        if(_controller.session.GetTrainJobStatus() == TrainJobStatus.SUCCEEDED ||
            _controller.session.GetTrainJobStatus() == TrainJobStatus.FAILED ||
            _controller.session.GetTrainJobStatus() == TrainJobStatus.NONE
        )
        {
            trainView.SetTrainSetupSubViewState(true);
            trainView.SetTrainHudSubViewState(false);
        }
        else
        {
            OnWatchButtonPressed();
        }
    }

    public override void OnExitState()
    {
        GameManager.instance.SavePlayerData();

        trainView.homeButtonPressed -= OnHomeButtonPressed;
        trainView.backButtonPressed -= OnBackButtonPressed;
        trainView.trainButtonPressed -= OnTrainButtonPressed;
        trainView.watchTrainButtonPressed -= OnWatchButtonPressed;

        RemoteTrainManager.instance.websocketOpen -= OnWebSocketOpen;
        RemoteTrainManager.instance.actionsReceived -= OnReceivedAgentActions;
        RemoteTrainManager.instance.metricsReceived -= OnReceivedTrainMetrics;
        RemoteTrainManager.instance.statusReceived -= OnReceivedTrainStatus;
        RemoteTrainManager.instance.binaryDataReceived -= OnBinaryDataRecived;


        if(_controller.session.currentEnvInstance)
        {
            _controller.session.currentEnvInstance.episodeAndStepCountUpdated -= trainView.UptadeInfo;
            _controller.session.currentEnvInstance.StopReplay();
        }

        trainView.CloseLoaderPopup();
    }

    void OnHomeButtonPressed()
    {
        OnExitState();
        SceneLoader.instance.LoadSceneDelay("IdleGame");
    }

    private void OnBackButtonPressed()
    {
        OnExitState();
        //ViewManager.ShowLast();
        _controller.SwitchState(new AcademyMainState(_controller));
    }

    public override void Tick(float deltaTime)
    {

    }

    async void OnTrainButtonPressed(int steps)
    {
        //TODO: Check if model is already trained or if it is trainning
        if (_controller.session.currentEnvInstance == null)
        {
            PrepareEnv();
        }
        _controller.session.selectedAgent.modelConfig.behavior.steps = steps * 1000;

        _controller.session.currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        _controller.session.currentEnvInstance.episodeAndStepCountUpdated += trainView.UptadeInfo;
        _controller.session.currentEnvInstance.StartReplay();


        if (!RemoteTrainManager.instance.isConnected)
        {
            //MainThreadDispatcher.Instance().EnqueueAsync(ConnectToRemoteInstance);
        }

        trainView.UptadeInfo(0, 0, 0);
        trainView.UpdateMetrics(0,0);
        trainView.SetTrainSetupSubViewState(false);
        trainView.SetTrainHudSubViewState(true);
        trainView.ShowLoaderPopup("Connecting to remote server...");

        await ConnectToRemoteInstance();

    }

    private void OnWatchButtonPressed()
    {
        if (_controller.session.currentEnvInstance == null)
        {
            PrepareEnv();
        }
        
        if(!RemoteTrainManager.instance.isConnected)
        {
            Debug.Log("Connect to websockt stream!");
            RemoteTrainManager.instance.ConnectWebSocketCentralNodeClientStream();
        }

        _controller.session.currentEnvInstance.StartReplay();
        _controller.session.currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        _controller.session.currentEnvInstance.episodeAndStepCountUpdated += trainView.UptadeInfo;

        trainView.UptadeInfo(0, 0, 0);
        trainView.UpdateMetrics(0, 0);
        trainView.SetTrainSetupSubViewState(false);
        trainView.SetTrainHudSubViewState(true);
        trainView.ShowLoaderPopup("Connecting to remote server...");

        trainView.SetTrainHudSubViewState(true);

    }

    async Task ConnectToRemoteInstance()
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

            if (!RemoteTrainManager.instance.isConnected)
            {
                //Open websocket with central node
                RemoteTrainManager.instance.ConnectWebSocketCentralNodeClientStream();

            }

            //Only subract steps when we have 200 response TODO: In the future this logic will be on a server
            _controller.session.selectedAgent.SetRunID(response.run_id);
            _controller.session.selectedAgent.modelConfig.CreateNewTrainMetricsEntry();
            _controller.session.selectedAgent.modelConfig.trainJobStatus = response.job_status;

            GameManager.instance.playerData.SubtractStepsAvailable(_controller.session.selectedAgent.modelConfig.behavior.steps);
            GameManager.instance.SavePlayerData();

            trainView.UpdateLoaderMessage("Train job submitted \nTrain job status: " + response.job_status);
            Debug.Log("response: " + response + " run_id: " + response.run_id);
        }
        catch (Exception e)
        {
            trainView.SetTrainSetupSubViewState(true);
            trainView.SetTrainHudSubViewState(false);
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.FAILED;
            Debug.LogError("Unable to add job to the queue: " + e.Message);
            trainView.CloseLoaderPopup();
        }
    }


    void OnReceivedTrainStatus(TrainJobStatusMsg trainJobStatus)
    {
        trainView.UpdateLoaderMessage("Connected!\nTrain job status: " + trainJobStatus.status);

        if (trainJobStatus.status == TrainJobStatus.SUCCEEDED)
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
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.FAILED;
            _controller.SwitchState(new AcademyTrainState(_controller));
        }
        else if (trainJobStatus.status == TrainJobStatus.RETRIEVED)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RETRIEVED;
        }
        else if (trainJobStatus.status == TrainJobStatus.STARTING)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.STARTING;
        }
        else if(trainJobStatus.status == TrainJobStatus.RUNNING)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RUNNING;
            trainView.CloseLoaderPopup();
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

    void OnWebSocketOpen()
    {
        string run_id = _controller.session.selectedAgent.GetModelRunID();
        RunId data = new RunId { run_id = run_id };

        string json = JsonConvert.SerializeObject(data);
        RemoteTrainManager.instance.SendWebSocketJson(json);
    }

    void OnBinaryDataRecived(byte[] data)
    {   
        try
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUCCEEDED;

            string runId = _controller.session.selectedAgent.GetModelRunID();

            var (filePath, nnModel) = MLHelper.SaveAndLoadModel(data, runId, _controller.session.selectedAgent.modelConfig.behavior.behavior_name);

            _controller.session.selectedAgent.modelConfig.isModelLoaded = true;
            _controller.session.selectedAgent.SetModelAndPath(filePath, nnModel);

            // Load model on current agent
            _controller.session
            .currentAgentInstance.GetComponent<IAgent>()
            .LoadModel(_controller.session.selectedAgent.modelConfig.behavior.behavior_name, nnModel);


            _controller.session.selectedAgent.modelConfig.AddStepsTrained(
                _controller.session.selectedAgent.modelConfig.behavior.steps
            );

            _controller.session.currentEnvInstance.StopReplay();

            RemoteTrainManager.instance.CloseWebSocketConnection();

            _controller.SwitchState(new EvaluateAndResultsState(_controller));
        }
        catch(Exception e)
        {
            Debug.Log("OnBinaryDataRecived error: " + e.Message + " trace: " + e.StackTrace);
        }
    }

    void OnReceivedTrainMetrics(MetricsMsg metrics)
    {
        _controller.session.selectedAgent.AddTrainMetrics(
            metrics.step,
            metrics.mean_reward,
            metrics.mean_reward,
            metrics.time_elapsed
        );

        trainView.UpdateMetrics(
            metrics.mean_reward,
            metrics.std_reward
        );
    }

    void OnReceivedAgentActions(string actionsJson)
    {
        _controller.session.currentEnvInstance.OnActionsFromServerReceived(actionsJson);

        Debug.Log("Actions received: " + actionsJson);
    }

/*
    (string, Model) SaveAndLoadModel(Byte[] rawModel, string runId, string behaviourName)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "runs", runId, "models");
        string filePath = Path.Combine(directoryPath, $"{behaviourName}.onnx");

        // Ensure the directory exists
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        //Save model to disk
        File.WriteAllBytes(filePath, rawModel);
        Debug.Log("Model saved at: " + filePath);

        Model nnModel = MLHelper.LoadModelRuntime(behaviourName, rawModel);
        //TODO: NAME
        //nnModel = behaviourName;
  
        return (filePath, nnModel);
    }
*/
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