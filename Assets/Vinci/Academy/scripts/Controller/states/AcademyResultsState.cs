using System;
using System.Collections.Generic;
using System.IO;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using Unity.MLAgents;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyResultsState : StateBase
{
    AcademyController _controller;
    AcademyTrainResultsView _resultsView;

    EnvironementBase currentEnvInstance;

    public AcademyResultsState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        _resultsView = ViewManager.GetView<AcademyTrainResultsView>();
        _resultsView.mintModelButtonPressed += OnMintModelButtonPRessed;
        _resultsView.trainAgainButtonPressed += OnTrainAgainButtonPressed;
        _resultsView.evaluateModelButtonPressed += OnTestModelButtonPressed;
        _resultsView.stopEvaluateModelButtonPressed += OnStopTestButtonPressed;

        ViewManager.Show<AcademyTrainResultsView>();
        _resultsView.ShowResultsSubView();

        _resultsView.UpdateTrainResults(
            _controller.session.selectedAgent.modelConfig.trainMetrics.stepsTrained,
            _controller.session.selectedAgent.modelConfig.trainMetrics.meanReward,
            _controller.session.selectedAgent.modelConfig.trainMetrics.stdReward
        );

        currentEnvInstance = _controller.session.currentEnvInstance;
        currentEnvInstance.updateEnvResults += OnUpdateEnvResults;
        currentEnvInstance.StopEnv();
        currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);


        Dictionary<string, string> evaluationResults =
             GameManager.instance.playerData.GetEvaluationResultsByKey(_controller.session.selectedTrainEnv.env_id);

        if (_controller.session.selectedAgent.modelConfig.isEvaluated && evaluationResults != null)
        {   
            _resultsView.UpdateEvaluationMetricsResults(evaluationResults);
        }
        else
        {
            _resultsView.UpdateEvaluationMetricsResults(currentEnvInstance.GetEvaluationMetricResults());
        }
    }

    public override void OnExitState()
    {
        currentEnvInstance.updateEnvResults -= OnUpdateEnvResults;

        _resultsView.mintModelButtonPressed -= OnMintModelButtonPRessed;
        _resultsView.trainAgainButtonPressed -= OnTrainAgainButtonPressed;
        _resultsView.evaluateModelButtonPressed -= OnTestModelButtonPressed;
        _resultsView.stopEvaluateModelButtonPressed -= OnStopTestButtonPressed;
    }

    public override void Tick(float deltaTime)
    {
        //Debug.Log("Episode: " + Academy.Instance.EpisodeCount);
        //Debug.Log("Steps: " + Academy.Instance.StepCount);
    }

    void OnTestModelButtonPressed()
    {
        EvaluateModel();
    }

    void OnStopTestButtonPressed()
    {
        currentEnvInstance.StopEnv();

        GameManager.instance.playerData.AddOrUpdateEvaluationResults(
            _controller.session.selectedTrainEnv.env_id,
            currentEnvInstance.GetEvaluationMetricResults()
        );

        GameManager.instance.SavePlayerData();

        _resultsView.UpdateEvaluationMetricsResults(
            GameManager.instance.playerData.GetEvaluationResultsByKey(_controller.session.selectedTrainEnv.env_id)
        );

        _resultsView.ShowResultsSubView();
    }

    void OnTrainAgainButtonPressed()
    {
        _controller.SwitchState(new AcademyTrainState(_controller));
    }

    void OnMintModelButtonPRessed()
    {
        BlockchainManager.instance.MintNNmodel();
    }

    void EvaluateModel()
    {
        _resultsView.UpdateEvaluationMetrics(currentEnvInstance.GetEvaluationMetricResults());
        _resultsView.ShowTestModelMetrics();

        currentEnvInstance.StartEnv();
        _controller.session.currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);
    }

    void OnUpdateEnvResults(Dictionary<string, string> results)
    {
        _resultsView.UpdateEvaluationMetrics(results);
    }
}