using System;
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

    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    void OnTestModelButtonPressed()
    {
        _resultsView.ShowTestModelMetrics();
    }

    void OnStopTestButtonPressed()
    {
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

    void OnGameUpdateResults(int goalCompletedCount, int goalFailedCount)
    {
        float successRatio = 0.0f;

        if (goalCompletedCount != 0)
        {
            successRatio = (float)goalCompletedCount / (goalCompletedCount + goalFailedCount);
        }

        _resultsView.UpdateTestMetrics(goalCompletedCount, goalFailedCount, successRatio);
    }
}