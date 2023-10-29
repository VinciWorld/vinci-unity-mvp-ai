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

        if(_controller.session == null)
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
        }
        else
        {
            _mainView.SetLastJobStatus("Not trained");
        }

        //TODO: Load available models for this model

        UpdateTrainJobStatus();

    }

    public override void OnExitState()
    {
        GameManager.instance.SavePlayerData();
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
        GameManager.instance.SavePlayerData();
        SceneLoader.instance.LoadSceneDelay("IdleGame");
    }

    private void OnWatchTrainButtonPressed()
    {
        _controller.session.selectedAgent = _controller.academyData.availableAgents[0];
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
    }

    void OnSelectAgentButtonPressed()
    {
        Debug.Log("_controller.session.selectedAgent: " + _controller.session.selectedAgent);
        _controller.session.selectedAgent = _controller.academyData.availableAgents[0];
        Debug.Log("_controller.session.selectedAgent: after " + _controller.session.selectedAgent);
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
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
                    continue;
                }
                
                if(!_controller.session.selectedAgent.modelConfig.isModelLoaded)
                {
                    PostResponseTrainJob job = await RemoteTrainManager.instance.GetTrainJobByRunID(
                        agent.modelConfig.runId, null
                    );

                    OnReceiveTrainJobStatus(job);
                }
                else
                {
                    _mainView.SetLastJobStatus("Completed");
                    _mainView.SetLastTrainSteps(agent.modelConfig.behavior.steps);
                    _mainView.SetTotalStepsTrained(agent.modelConfig.totalStepsTrained);
                }
            }
            else
            {
                _mainView.SetLastJobStatus("Not trained");
            }
        }        
    }

    async private void OnReceiveTrainJobStatus(PostResponseTrainJob job)
    {

        Debug.Log("Receveid train job: " + job.job_status);

        AgentBlueprint agent = GameManager.instance.playerData.GetAgentById(job.env_config.agent_id);

        switch (job.job_status)
        {
            case TrainJobStatus.SUBMITTED:
                {
                    _mainView.SetLastJobStatus("Waitting to train");
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUBMITTED;
                    break;
                }
            case TrainJobStatus.RETRIEVED:
                {
                    _mainView.SetLastJobStatus("Waitting to train");
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RETRIEVED;
                    break;
                }
            case TrainJobStatus.STARTING:
                {
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.STARTING;
                    _mainView.SetLastJobStatus("Waitting to train");
                    break;
                }
            case TrainJobStatus.RUNNING:
                {
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RUNNING;
                    _mainView.SetLastJobStatus("Training");
                    break;
                }
            case TrainJobStatus.SUCCEEDED:
                {
                    _mainView.SetLastJobStatus("Loading model...");
                    _mainView.ShowLoaderPopup("Train is completed! Downloading Model...");

                    if (!agent.modelConfig.isModelLoaded)
                    {
                        try
                        {
                            byte[] model = await RemoteTrainManager.instance.DownloadNNModel(agent.modelConfig.runId);

                            var (filePath, nnModel) = MLHelper.SaveAndLoadModel(model, agent.modelConfig.runId, agent.modelConfig.behavior.behavior_name);

                            agent.SetModelAndPath(filePath, nnModel);
                            agent.modelConfig.isModelLoaded = true;

                            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUCCEEDED;

                            //TODO: Retrive from server metrics from the train!
                            agent.modelConfig.ResetLastTrainMetricsEntry();

                            agent.modelConfig.AddStepsTrained(
                                agent.modelConfig.behavior.steps
                            );

                            _mainView.SetLastJobStatus("Trained Completed");
                            _mainView.SetLastTrainSteps(agent.modelConfig.behavior.steps);
                            _mainView.SetTotalStepsTrained(agent.modelConfig.GetMostRecentMetric().stepsTrained);
   
                        }
                        catch (Exception e)
                        {
                            _mainView.SetLastJobStatus("Failed to donwload model!");
                            Debug.Log("Error Unable to load model: " + e.Message + " stake: " + e.StackTrace);
                        }
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