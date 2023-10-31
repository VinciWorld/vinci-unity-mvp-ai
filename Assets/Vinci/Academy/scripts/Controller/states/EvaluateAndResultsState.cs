using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class EvaluateAndResultsState : StateBase
{
    AcademyController _controller;
    AcademyTrainResultsView _resultsView;

    EnvironementBase currentEnvInstance;

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

        _resultsView.UpdateTrainResults(
            _controller.session.selectedAgent.modelConfig.GetStepsTrained(),
            _controller.session.selectedAgent.modelConfig.GetLastMeanReward(),
            _controller.session.selectedAgent.modelConfig.GetLastStdReward()
        );

        currentEnvInstance = _controller.session.currentEnvInstance;
        currentEnvInstance.SetEvaluationEvents(
            _resultsView.UpdateEvaluationMetrics,
            _resultsView.UpdateEvaluationMetrics,
            _resultsView.UpdateEvaluationMetrics
        );

        currentEnvInstance.StopEnv();
        currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);

        //_resultsView.ShowResultsSubView();

        EvaluateModel();


        //Dictionary<string, MetricValue> evaluationResults =
        //    _controller.session.selectedAgent.GetCommonEvaluationMetrics(_controller.session.selectedTrainEnv.env_id);

        _resultsView.UpdateEvaluationCommonMetrics(currentEnvInstance.GetEvaluaitonCommonTemplate());
    }

    public override void OnExitState()
    {
        //currentEnvInstance.updateCommonResults -= OnUpdateEnvResults;

        currentEnvInstance.RemoveListeners(
            _resultsView.UpdateEvaluationMetrics,
            _resultsView.UpdateEvaluationMetrics,
            _resultsView.UpdateEvaluationMetrics
        );


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

    void OnStopTestButtonPressed()
    {
        Time.timeScale = 1;
        currentEnvInstance.StopEnv();

        _controller.session.selectedAgent.AddOrUpdateCommonEvaluationMetrics(
            _controller.session.selectedTrainEnv.env_id,
            currentEnvInstance.GetEvaluationMetricCommonResults()
        );

        GameManager.instance.SavePlayerData();

        _resultsView.UpdateEvaluationCommonMetrics(
            _controller.session.selectedAgent.GetCommonEvaluationMetrics(_controller.session.selectedTrainEnv.env_id)
        );

        _resultsView.ShowResults();
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

    void EvaluateModel()
    {
        Time.timeScale = 3;
        _resultsView.UpdateEvaluationMetrics(currentEnvInstance.GetEvaluaitonCommonTemplate());
        _resultsView.ShowTestModelMetrics();

        if (_controller.session.currentEnvInstance == null)
        {
            PrepareEnv();
        }

        currentEnvInstance.StartEnv();
        _controller.session.currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);
    }

    void OnUpdateEnvResults(Dictionary<string, MetricValue> results)
    {
        _resultsView.UpdateEvaluationMetrics(results);
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
}