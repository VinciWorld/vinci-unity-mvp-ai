using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class EvaluateAndResultsState : StateBase
{
    AcademyController _controller;
    AcademyTrainResultsView _resultsView;

    EnvironementBase currentEnvInstance;

    private int episodeEvaluationTotal = 10;

    public EvaluateAndResultsState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        _resultsView = ViewManager.GetView<AcademyTrainResultsView>();
        _resultsView.mintModelButtonPressed += OnMintModelButtonPressed;
        _resultsView.trainAgainButtonPressed += OnTrainAgainButtonPressed;
        _resultsView.evaluateModelButtonPressed += OnTestModelButtonPressed;
        _resultsView.stopEvaluateModelButtonPressed += OnStopTestButtonPressed;
        _resultsView.homeButtonPressed += OnHomeButtonPressed;

        ViewManager.Show<AcademyTrainResultsView>();

        

        //_resultsView.ShowResultsSubView();

        if(_controller.session.selectedAgent.modelConfig.isEvaluated)
        {
            ShowResults();
        }
        else
        {
            EvaluateModel();
        }



        //Dictionary<string, MetricValue> evaluationResults =
        //    _controller.session.selectedAgent.GetCommonEvaluationMetrics(_controller.session.selectedTrainEnv.env_id);


    }

    public override void OnExitState()
    {
        //currentEnvInstance.updateCommonResults -= OnUpdateEnvResults;

        _resultsView.mintModelButtonPressed -= OnMintModelButtonPressed;
        _resultsView.trainAgainButtonPressed -= OnTrainAgainButtonPressed;
        _resultsView.evaluateModelButtonPressed -= OnTestModelButtonPressed;
        _resultsView.stopEvaluateModelButtonPressed -= OnStopTestButtonPressed;

        Time.timeScale = 1;
    }

    public override void Tick(float deltaTime)
    {
        //Debug.Log("Episode: " + Academy.Instance.EpisodeCount);
        //Debug.Log("Steps: " + Academy.Instance.StepCount);
    }

    void OnHomeButtonPressed()
    {
        OnExitState();
        SceneLoader.instance.LoadSceneDelay("IdleGame");
    }

    void OnTestModelButtonPressed()
    {
        EvaluateModel();
    }


    void OnTrainAgainButtonPressed()
    {
        _controller.SwitchState(new AcademyTrainState(_controller));
    }

    async void OnMintModelButtonPressed()
    {
        _resultsView.ShowLoaderPopup("Uploading trained Model to Arweave...");

        string uri = await RemoteTrainManager.instance.SaveOnArweaveModelFromS3(_controller.session.selectedAgent.GetModelRunID());
        Debug.Log("uri: " + uri);
        _resultsView.UpdatePopupMessange("Minting model...");

        await BlockchainManager.instance.MintNNmodel(uri);
        _resultsView.CloseLoaderPopup();

    }

    void ShowResults()
    {
        _resultsView.ShowResults();

        _resultsView.UpdateModelTrainResults(
            _controller.session.selectedAgent.modelConfig.GetStepsTrained(),
            _controller.session.selectedAgent.modelConfig.GetLastMeanReward(),
            _controller.session.selectedAgent.modelConfig.GetLastStdReward()
        );

        _resultsView.UpdateEvaluationResultsMetrics(
            _controller.session.selectedAgent.GetCommonEvaluationMetrics(_controller.session.selectedTrainEnv.env_id)
        );
        _resultsView.UpdateEvaluationResultsMetrics(
            _controller.session.selectedAgent.GetEnvEvaluationMetrics(_controller.session.selectedTrainEnv.env_id)
        );

        //_resultsView.UpdateEvaluationCommonMetrics(currentEnvInstance.GetEvaluaitonCommonTemplate());

    
    }

    #region EvluateModel

    void OnStopTestButtonPressed()
    {
        Time.timeScale = 1;
        _controller.session.currentEnvInstance.StopEnv();

        _controller.session.selectedAgent.StoreSessionEvaluationMetrics(
            _controller.session.selectedTrainEnv.env_id,
            _controller.session.currentEnvInstance.GetEvaluationMetricCommonResults(),
            _controller.session.currentEnvInstance.GetEvaluationMetricEnvResults(),
            _controller.session.currentEnvInstance.GetEvaluationMetricAgentResults()
        );

        GameManager.instance.SavePlayerData();

        _controller.session.currentEnvInstance.RemoveListeners(
            _resultsView.UpdateEvaluationCommonMetrics,
            _resultsView.UpdateEvaluationAgentMetrics
        );

        _controller.session.currentEnvInstance.episodeCountUpdated += OnEpisodeUpdated;

        _controller.session.selectedAgent.modelConfig.isEvaluated = true;
   


        ShowResults();
    }

    void EvaluateModel()
    {
        Time.timeScale = 3;
        EnvironementBase createdEnv = _controller.session.currentEnvInstance;

        if (_controller.session.currentEnvInstance == null)
        {
            createdEnv = CreateEnv();
        }

        createdEnv.SetEvaluationEvents(
            _resultsView.UpdateEvaluationCommonMetrics,
            _resultsView.UpdateEvaluationAgentMetrics
        );

        _resultsView.UpdateEvaluationCommonMetrics(createdEnv.GetEvaluaitonCommonTemplate());
        _resultsView.UpdateEvaluationAgentMetrics(createdEnv.GetEvaluaitonEnvTemplate());
        _resultsView.ShowEvaluationHud(episodeEvaluationTotal);
        createdEnv.episodeCountUpdated += OnEpisodeUpdated;

        createdEnv.StartEnv(BehaviorType.InferenceOnly);
    }

    void OnEpisodeUpdated(int episodeCount)
    {
        _resultsView.UpdateEpisodCount(episodeCount);

        if(episodeCount > episodeEvaluationTotal)
        {
            OnStopTestButtonPressed();
        }

    }

    void OnUpdateEnvResults(Dictionary<string, MetricValue> results)
    {
        _resultsView.UpdateEvaluationAgentMetrics(results);
    }

    public EnvironementBase CreateEnv()
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

        return created_env;
    }

    #endregion
}