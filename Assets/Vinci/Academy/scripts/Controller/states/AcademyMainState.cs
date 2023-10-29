using System;
using System.IO;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using Unity.VisualScripting;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.ML.Utils;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;
using WebSocketSharp;

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

        _mainView.HideAllButtons();

        RemoteTrainManager.instance.statusReceived += OnReceivedTrainStatus;

        if (_controller.session == null)
        {
            _controller.session = new AcademySession();
        }

        _mainView.homeButtonPressed += OnHomeButtonPressed;
        _mainView.selectAgentButtonPressed += OnSelectAgentButtonPressed;
        _mainView.createAgentButtonPressed += OnCreateAgent;
        _mainView.watchTrainingButtonPressed += OnWatchTrainButtonPressed;
        //_controller.session.currentAgentInstance = null;
        //_controller.session.currentEnvInstance = null;

        if(GameManager.instance.playerData.agents.Count > 0)
        {
            _controller.session.selectedAgent = GameManager.instance.playerData.GetAgent(0);
            UpdateTrainJobStatus();
        }
        else
        {
            _mainView.ShowCreateButton();
            _mainView.SetLastJobStatus("Not trained", "#E44962");
        }

        //TODO: Load available models for this model
    }

    public override void OnExitState()
    {
       // RemoteTrainManager.instance.ConnectWebSocketCentralNodeClientStream();

        RemoteTrainManager.instance.statusReceived -= OnReceivedTrainStatus;

        _mainView.homeButtonPressed -= OnHomeButtonPressed;
        _mainView.selectAgentButtonPressed -= OnSelectAgentButtonPressed;
        _mainView.createAgentButtonPressed -= OnCreateAgent;
        _mainView.watchTrainingButtonPressed -= OnWatchTrainButtonPressed;
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
        OnExitState();
        SceneLoader.instance.LoadSceneDelay("IdleGame");
    }

    private void OnWatchTrainButtonPressed()
    {
        _controller.session.selectedAgent = _controller.manager.playerData.GetAgent(0);
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
    }

    void OnSelectAgentButtonPressed()
    {
        _controller.session.selectedAgent = _controller.manager.playerData.GetAgent(0);
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
    }

    async void OnReceivedTrainStatus(TrainJobStatusMsg trainJobStatus)
    {
        if (trainJobStatus.status == TrainJobStatus.SUCCEEDED)
        {
            try
            {

                RemoteTrainManager.instance.CloseWebSocketConnection();

                _mainView.SetLastJobStatus("Loading model...", "#00AE75");
                _mainView.ShowLoaderPopup("Train is completed! Downloading Model...");

                try
                {
                    byte[] model = await RemoteTrainManager.instance.DownloadNNModel(_controller.session.selectedAgent.modelConfig.runId);
                    var (filePath, nnModel) = MLHelper.SaveAndLoadModel(model, _controller.session.selectedAgent.modelConfig.runId, _controller.session.selectedAgent.modelConfig.behavior.behavior_name);

                    _controller.session.selectedAgent.SetModelAndPath(filePath, nnModel);
                    _controller.session.selectedAgent.modelConfig.isModelLoaded = true;

                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUCCEEDED;

                    //TODO: Retrive from server metrics from the train!
                    //agent.modelConfig.ResetLastTrainMetricsEntry();

                    _controller.session.selectedAgent.modelConfig.AddStepsTrained(
                        _controller.session.selectedAgent.modelConfig.behavior.steps
                    );

                    GameManager.instance.SavePlayerData();

                    _mainView.SetLastJobStatus("Train completed", "#FFB33A");
                    _mainView.SetTrainsCount(_controller.session.selectedAgent.modelConfig.trainCount);
                    _mainView.SetLastTrainSteps(_controller.session.selectedAgent.modelConfig.behavior.steps);
                    _mainView.SetTotalStepsTrained(_controller.session.selectedAgent.modelConfig.totalStepsTrained);
                    _mainView.ShowEvaluateButton();

                }
                catch (Exception e)
                {
                    _mainView.SetLastJobStatus("Failed to donwload model!", "#E44962");
                    _mainView.CloseLoaderPopup();
                    Debug.Log("Error Unable to load model: " + e.Message + " stake: " + e.StackTrace);
                }

                //Show button evaluate model
                _mainView.SetLastJobStatus("Train completed", "#FFB33A");
                _mainView.ShowEvaluateButton();
                _mainView.CloseLoaderPopup();

            }
            catch (Exception e)
            {
                Debug.LogError("Unable to save and Load model: " + e.Message);
            }
        }
        else if (trainJobStatus.status == TrainJobStatus.FAILED)
        {
            Debug.Log("JOB FAILED");
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.FAILED;
            _mainView.SetLastJobStatus("Failed", "#E44962");
            _controller.SwitchState(new AcademyTrainState(_controller));
        }
        else if (trainJobStatus.status == TrainJobStatus.SUBMITTED)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUBMITTED;
            _mainView.SetLastJobStatus("On Queue", "#00AE75");
        }
        else if (trainJobStatus.status == TrainJobStatus.RETRIEVED)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RETRIEVED;
            _mainView.SetLastJobStatus("Retrieved", "#00AE75");
        }
        else if (trainJobStatus.status == TrainJobStatus.STARTING)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.STARTING;
            _mainView.SetLastJobStatus("Starting", "#00AE75");
        }
        else if (trainJobStatus.status == TrainJobStatus.RUNNING)
        {
            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RUNNING;
            _mainView.SetLastJobStatus("Training", "#00AE75");
        }

        Debug.Log("Status received: " + trainJobStatus.status);
    }


    async void UpdateTrainJobStatus()
    {

        foreach (var agent in _controller.manager.playerData.agents)
        {
            if (!agent.modelConfig.runId.IsNullOrEmpty())
            {
                if(_controller.session.selectedAgent.modelConfig.trainJobStatus == TrainJobStatus.SUCCEEDED
                    && _controller.session.selectedAgent.modelConfig.isModelLoaded)
                {
                    _mainView.SetLastJobStatus("Completed", "#FFB33A");
                    _mainView.SetTrainsCount(agent.modelConfig.trainCount);
                    _mainView.SetLastTrainSteps(agent.modelConfig.behavior.steps);
                    _mainView.SetTotalStepsTrained(agent.modelConfig.totalStepsTrained);

                    _mainView.ShowWTrainButton();
                }
                else
                {
                    PostResponseTrainJob job = await RemoteTrainManager.instance.GetTrainJobByRunID(
                        agent.modelConfig.runId, null
                    );

                    OnReceiveTrainJobStatus(job);
                }

            /*    
                else
                {
                    _mainView.SetLastJobStatus("Completed");
                    _mainView.SetLastTrainSteps(agent.modelConfig.behavior.steps);
                    _mainView.SetTotalStepsTrained(agent.modelConfig.totalStepsTrained);
                }
            */
            }
            else
            {
                _mainView.ShowWTrainButton();
                _mainView.SetLastJobStatus("Not trained", "#E44962");
            }
        }        
    }

    async private void OnReceiveTrainJobStatus(PostResponseTrainJob job)
    {

        Debug.Log("Receveid train job: " + job.job_status);

        AgentBlueprint agent = GameManager.instance.playerData.GetAgentById(job.env_config.agent_id);
        _controller.session.selectedAgent.modelConfig.trainJobStatus = job.job_status;

        switch (job.job_status)
        {
            case TrainJobStatus.SUBMITTED:
            case TrainJobStatus.RETRIEVED:
            case TrainJobStatus.STARTING:
                {
                    _mainView.SetLastJobStatus("On Queue", "#00AE75");
                    _mainView.ShowWatchButton();
                    break;
                }
            case TrainJobStatus.RUNNING:
                {
                    if (!RemoteTrainManager.instance.isConnected)
                    {
                        RemoteTrainManager.instance.ConnectWebSocketCentralNodeClientStream();
                    }

                    _mainView.SetTrainsCount(agent.modelConfig.trainCount);
                    _mainView.SetLastTrainSteps(agent.modelConfig.behavior.steps);
                    _mainView.SetTotalStepsTrained(agent.modelConfig.totalStepsTrained);

                    _mainView.SetLastJobStatus("Training", "#00AE75");
                    _mainView.ShowWatchButton();
                    break;
                }
            case TrainJobStatus.SUCCEEDED:
                {
                    _mainView.SetLastJobStatus("Loading model...", "#00AE75");
                    _mainView.ShowLoaderPopup("Train is completed! Downloading Model...");

                    try
                    {
                        byte[] model = await RemoteTrainManager.instance.DownloadNNModel(agent.modelConfig.runId);
                        var (filePath, nnModel) = MLHelper.SaveAndLoadModel(model, agent.modelConfig.runId, agent.modelConfig.behavior.behavior_name);

                        agent.SetModelAndPath(filePath, nnModel);
                        agent.modelConfig.isModelLoaded = true;

                        _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUCCEEDED;

                        //TODO: Retrive from server metrics from the train!
                        //agent.modelConfig.ResetLastTrainMetricsEntry();

                        agent.modelConfig.AddStepsTrained(
                            agent.modelConfig.behavior.steps
                        );

                        GameManager.instance.SavePlayerData();

                        _mainView.SetLastJobStatus("Train completed", "#FFB33A");
                        _mainView.SetTrainsCount(agent.modelConfig.trainCount);
                        _mainView.SetLastTrainSteps(agent.modelConfig.behavior.steps);
                        _mainView.SetTotalStepsTrained(agent.modelConfig.totalStepsTrained);
                        _mainView.ShowEvaluateButton();

                    }
                    catch (Exception e)
                    {
                        _mainView.SetLastJobStatus("Failed to donwload model!", "#E44962");
                        Debug.Log("Error Unable to load model: " + e.Message + " stake: " + e.StackTrace);
                    }

                    _mainView.CloseLoaderPopup();
                    break;
                }

            default:
                Debug.Log("status not recognised");
                break;
        }
    }
/*
    void SaveAndLoadModel(Byte[] rawModel, string runId, string behaviourName)
    {
        try
        {
            Debug.Log("behaviour name: " + behaviourName + " run_id: " + runId);
            string directoryPath = Path.Combine(Application.persistentDataPath, "runs", runId, "models");
            string filePath = Path.Combine(directoryPath, $"{behaviourName}.onnx");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            //Save model to disk
            File.WriteAllBytes(filePath, rawModel);
            Debug.Log("Model saved at: " + filePath);

            NNModel nnModel = MLHelper.LoadModelRuntime(behaviourName, rawModel);

    

        }
        catch (Exception e)
        {
            throw e;
        }
    }
    */
}