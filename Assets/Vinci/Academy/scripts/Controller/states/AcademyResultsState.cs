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

    EnvironementBase currentEnv;

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

        currentEnv = _controller.session.currentEnvInstance;
        currentEnv.updateEnvResults += OnUpdateEnvResults;

        _resultsView.UpdateEvaluationMetricsResults(currentEnv.GetEvaluationMetricResults());

    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    void OnTestModelButtonPressed()
    {
        currentEnv.Reset();
        _resultsView.ShowTestModelMetrics();
        EvaluateModel();
    }

    void OnStopTestButtonPressed()
    {
        _resultsView.UpdateEvaluationMetricsResults(currentEnv.GetEvaluationMetricResults());
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
        EnvHallway env = _controller.session.currentEnvInstance.GetComponent<EnvHallway>();

    }

    void OnUpdateEnvResults(Dictionary<string, string> results)
    {
        _resultsView.UpdateEvaluationMetrics(results);
    }
}