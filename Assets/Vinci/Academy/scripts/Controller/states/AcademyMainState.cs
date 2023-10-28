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

        //TODO: Load available models for this model

        UpdateTrainJobStatus();

        //Retrieve trained Model from central node if it was't downloaded yet
        if (_controller.session.selectedAgent != null)
        {
            if (!_controller.session.selectedAgent.modelConfig.run_id.IsNullOrEmpty()
                && !_controller.session.selectedAgent.modelConfig.isModelLoaded)
            {
                
                //if (_controller.manager.playerData.agents[0].modelConfig.isModelSucceeded == false)
               // {  
               // }
            }
        }

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
            if (!agent.modelConfig.run_id.IsNullOrEmpty())
            {
                if(_controller.session.selectedAgent.modelConfig.trainJobStatus == TrainJobStatus.SUCCEEDED
                    && _controller.session.selectedAgent.modelConfig.isModelLoaded)
                {
                    continue;
                }
                
                if(!_controller.session.selectedAgent.modelConfig.isModelLoaded)
                {
                    PostResponseTrainJob job = await RemoteTrainManager.instance.GetTrainJobByRunID(
                        agent.modelConfig.run_id, null
                    );

                    OnReceiveTrainJobStatus(job);
                }
                else
                {
                    _mainView.SetLastJobStatus("Completed");
                    _mainView.SetLastTrainSteps(agent.modelConfig.behavior.steps);
                    _mainView.SetTotalStepsTrained(agent.modelConfig.trainMetrics.stepsTrained);
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

        AgentConfig agent = GameManager.instance.playerData.GetAgentById(job.env_config.agent_id);

        switch (job.job_status)
        {
            case TrainJobStatus.SUBMITTED:
                {
                    _mainView.SetLastJobStatus("Waitting to train");
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUBMITTED;
                    agent.modelConfig.isModelSubmitted = true;
                    break;
                }
            case TrainJobStatus.RETRIEVED:
                {
                    _mainView.SetLastJobStatus("Waitting to train");
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RETRIEVED;
                    agent.modelConfig.isModelTraining = true;
                    break;
                }
            case TrainJobStatus.STARTING:
                {
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.STARTING;
                    _mainView.SetLastJobStatus("Waitting to train");
                    agent.modelConfig.isModelTraining = true;
                    break;
                }
            case TrainJobStatus.RUNNING:
                {
                    _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.RUNNING;
                    _mainView.SetLastJobStatus("Training");
                    agent.modelConfig.isModelTraining = true;
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
                            byte[] model = await RemoteTrainManager.instance.DownloadNNModel(agent.modelConfig.run_id);

                            var (filePath, nnModel) = MLHelper.SaveAndLoadModel(model, agent.modelConfig.run_id, agent.modelConfig.behavior.behavior_name);

                            agent.SetModelAndPath(filePath, nnModel);
                            agent.modelConfig.isModelSucceeded = true;

                            _controller.session.selectedAgent.modelConfig.trainJobStatus = TrainJobStatus.SUCCEEDED;
                            agent.modelConfig.isModelTraining = false;
                            agent.modelConfig.modelFinishedTraining = true;

                            agent.modelConfig.AddStepsTrained(
                                agent.modelConfig.behavior.steps
                            );

                            _mainView.SetLastJobStatus("Trained Completed");
                            _mainView.SetLastTrainSteps(agent.modelConfig.behavior.steps);
                            _mainView.SetTotalStepsTrained(agent.modelConfig.trainMetrics.stepsTrained);
   
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