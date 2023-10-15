using System;
using System.IO;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using Unity.VisualScripting;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

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

        _controller.session = new AcademySession();

        _mainView.homeButtonPressed += OnHomeButtonPressed;
        _mainView.selectAgentButtonPressed += OnSelectAgentButtonPressed;
        _mainView.createAgentButtonPressed += OnCreateAgent;
        _mainView.watchTrainingButtonPressed += OnWatchTrainButtonPressed;
        _controller.session.selectedAgent = null;
        _controller.session.currentAgentInstance = null;
        _controller.session.currentEnvInstance = null;

        //TODO: Load available models for this model


        if (_controller.manager.playerData.agents.Count > 0 && _controller.manager.playerData.agents[0].modelConfig.isModelSubmitted)
        {
            _controller.session.selectedAgent = _controller.manager.playerData.GetAgent(0);

            if (_controller.manager.playerData.agents[0].modelConfig.isModelSucceeded == false)
            {
                UpdateTrainJobStatus();
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
        _controller.session.selectedAgent = _controller.manager.playerData.GetAgent(0);
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
    }

    void OnSelectAgentButtonPressed()
    {
        Debug.Log("_controller.session.selectedAgent: " + _controller.session.selectedAgent);
        _controller.session.selectedAgent = _controller.manager.playerData.GetAgent(0);
        _controller.session.selectedTrainEnv = _controller.academyData.availableTrainEnvs[0];

        _controller.SwitchState(new AcademyTrainState(_controller));
    }


    async void UpdateTrainJobStatus()
    {

        foreach (var agent in _controller.manager.playerData.agents)
        {
            PostResponseTrainJob job = await RemoteTrainManager.instance.GetTrainJobByRunID(
                agent.GetModelRunID(), null);

            OnReceiveTrainJobStatus(job);
        }        
    }

    async private void OnReceiveTrainJobStatus(PostResponseTrainJob job)
    {

        Debug.Log("Receveid train job: " + job.job_status);

        AgentConfig agent = GameManager.instance.playerData.GetAgentById(job.env_config.agent_id);

        _mainView.SetStepsTrained(agent.modelConfig.trainMetrics.stepsTrained);

        switch (job.job_status)
        {
            case TrainJobStatus.SUBMITTED:
                {
                    _mainView.SetJobStatus("SUBMITTED");
                    agent.modelConfig.isModelSubmitted = true;
                    break;
                }
            case TrainJobStatus.RETRIEVED:
                {
                    _mainView.SetJobStatus("RETRIEVED");
                    agent.modelConfig.isModelTraining = true;
                    break;
                }
            case TrainJobStatus.STARTING:
                {
                    _mainView.SetJobStatus("STARTING");
                    agent.modelConfig.isModelTraining = true;
                    break;
                }
            case TrainJobStatus.RUNNING:
                {
                    _mainView.SetJobStatus("RUNNING");
                    agent.modelConfig.isModelTraining = true;
                    break;
                }
            case TrainJobStatus.SUCCEEDED:
                {
                    _mainView.SetJobStatus("Loading model...");
                    _mainView.ShowLoaderPopup("Train is completed! Downloading Model...");
                    if (agent.modelConfig.isModelTraining)
                    {
                        agent.modelConfig.AddStepsTrained(
                            agent.modelConfig.behavior.steps
                        );

                        agent.modelConfig.isModelTraining = false;
                        agent.modelConfig.modelFinishedTraining = true;
                    }

                    if(!agent.modelConfig.isModelLoaded)
                    {
                        try{
                            byte[] model = await RemoteTrainManager.instance.DownloadNNModel(agent.modelConfig.run_id);

                            SaveAndLoadModel(model, agent.modelConfig.run_id, agent.modelConfig.behavior.behavior_name, agent);

                            agent.modelConfig.isModelSucceeded = true;
                            
                            _mainView.SetJobStatus("SUCCEEDED");
                        }
                        catch(Exception e)
                        {
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

    void SaveAndLoadModel(Byte[] rawModel, string runId, string behaviourName, AgentConfig agent)
    {
        Debug.Log("behaviour name: " + behaviourName + " run_id: " + runId);
        string directoryPath = Path.Combine(Application.persistentDataPath, "runs", runId, "models");
        string filePath = Path.Combine(directoryPath, $"{behaviourName}.onnx");


        // Ensure the directory exists
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Convert and Save model to disk
        NNModel nnModel = SaveAndConvertModel(filePath, rawModel);
        nnModel.name = behaviourName;
        Debug.Log("Model saved at: " + filePath);

        agent.SetModelAndPath(filePath, nnModel);

        // Load model on current agent
        //_controller.session
        //.currentAgentInstance.GetComponent<IAgent>()
        //.LoadModel(behaviourName, nnModel);
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