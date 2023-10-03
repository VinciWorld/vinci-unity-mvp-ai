using System;
using System.Collections.Generic;
using System.IO;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
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
            _controller.session.selectedAgent.modelConfig.behavior.steps,
            0.0f
        );

        currentEnvInstance = _controller.session.currentEnvInstance;
        currentEnvInstance.updateEnvResults += OnUpdateEnvResults;
        currentEnvInstance.StopEnv();
        currentEnvInstance.SetAgentBehavior(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);


        Dictionary<string, string> evaluationREsults =
             GameManager.instance.playerData.GetEvaluationResultsByKey(_controller.session.selectedTrainEnv.env_id);

        if (_controller.session.selectedAgent.modelConfig.isEvaluated && evaluationREsults != null)
        {   
            _resultsView.UpdateEvaluationMetricsResults(evaluationREsults);
        }
        else
        {
            _resultsView.UpdateEvaluationMetricsResults(currentEnvInstance.GetEvaluationMetricResults());
        }
    }

    public override void OnExitState()
    {
        currentEnvInstance.updateEnvResults -= OnUpdateEnvResults;
    }

    public override void Tick(float deltaTime)
    {

    }

    void OnTestModelButtonPressed()
    {
        EvaluateModel();
    }

    void OnStopTestButtonPressed()
    {
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
        Debug.Log("Mint model!");
    }

    void EvaluateModel()
    {
        _resultsView.UpdateEvaluationMetrics(currentEnvInstance.GetEvaluationMetricResults());
        _resultsView.ShowTestModelMetrics();
        currentEnvInstance.StartEnv();
    }

    void OnUpdateEnvResults(Dictionary<string, string> results)
    {
        _resultsView.UpdateEvaluationMetrics(results);
    }
}