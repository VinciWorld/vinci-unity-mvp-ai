using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.ML.Utils;
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
        Debug.Log("OnExitState");
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
            _controller.session.currentEnvInstance.SetIsReplay(false);
        }

        trainView.CloseLoaderPopup();
    }

    private void CheckIfModelFinishedTraining()
    {

    }

    private void OnWatchButtonPressed()
    {
        trainView.SetTrainHudSubViewState(true);
        //_controller.session.currentEnvInstance.SetIsReplay(true);
        //_controller.session.currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        //_controller.session.currentEnvInstance.episodeAndStepCountUpdated += trainView.UptadeInfo;
    }


    void OnHomeButtonPressed()
    {
        OnExitState();
        SceneLoader.instance.LoadSceneDelay("IdleGame");
    }

    private void OnBackButtonPressed()
    {
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

        GameManager.instance.playerData.SubtractStepsAvailable(_controller.session.selectedAgent.modelConfig.behavior.steps);

        _controller.session.currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        _controller.session.currentEnvInstance.episodeAndStepCountUpdated += trainView.UptadeInfo;
        _controller.session.currentEnvInstance.SetIsReplay(true);


        if (!RemoteTrainManager.instance.isConnected)
        {
            //MainThreadDispatcher.Instance().EnqueueAsync(ConnectToRemoteInstance);
        }

        Debug.Log("OnTrainButtonPressed 5");

        trainView.UptadeInfo(0, 0, 0);
        trainView.UpdateMetrics(0,0);
        trainView.SetTrainSetupSubViewState(false);
        trainView.SetTrainHudSubViewState(true);
        trainView.ShowLoaderPopup("Connecting to remote server...");

        if (!RemoteTrainManager.instance.isConnected)
        {
            await ConnectToRemoteInstance();
        }
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
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.FAILED;
            _controller.SwitchState(new AcademyTrainState(_controller));
        }
        else if (trainJobStatus.status == TrainJobStatus.RETRIEVED)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RETRIEVED;
            trainView.UpdateLoaderMessage("Connected!\nTrain job status: " + trainJobStatus.status);
        }
        else if (trainJobStatus.status == TrainJobStatus.STARTING)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.STARTING;
            trainView.UpdateLoaderMessage("Connected!\nTrain job status: " + trainJobStatus.status);
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
        Debug.Log("Cagent: " + created_agent);

        created_env.Initialize(created_agent);

        _controller.session.currentAgentInstance = created_agent;
        _controller.session.currentEnvInstance = created_env;
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
            RemoteTrainManager.instance.ConnectWebSocketCentralNodeClientStream();

            _controller.session.selectedAgent.SetRunID(response.run_id);
            _controller.session.selectedAgent.modelConfig.CreateNewTrainMetricsEntry();

            switch (response.job_status)
            {
                case TrainJobStatus.SUBMITTED:
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUBMITTED;
                    trainView.UpdateLoaderMessage("Connected!\nTrain job status: " + response.job_status);
                    break;

                case TrainJobStatus.RETRIEVED:
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RETRIEVED;
                    trainView.UpdateLoaderMessage("Connected!\nTrain job status: " + response.job_status);
                    break;

                case TrainJobStatus.STARTING:
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.STARTING;
                    trainView.UpdateLoaderMessage("Connected!\nTrain job status: " + response.job_status);
                    break;

                case TrainJobStatus.RUNNING:
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RUNNING;
                    trainView.UpdateLoaderMessage("Connected!\nTrain job status: " + response.job_status);
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
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.FAILED;
            Debug.LogError("Unable to add job to the queue: " + e.Message);
            trainView.CloseLoaderPopup();
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

            _controller.SwitchState(new AcademyResultsState(_controller));
        }
        catch(Exception e)
        {
            Debug.Log("OnBinaryDataRecived error: " + e.Message);
        }

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

    (string, NNModel) SaveAndLoadModel(Byte[] rawModel, string runId, string behaviourName)
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

        NNModel nnModel = MLHelper.LoadModelRuntime(behaviourName, rawModel);
        nnModel.name = behaviourName;
  
        return (filePath, nnModel);
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