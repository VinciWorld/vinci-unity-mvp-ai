using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class AcademyResultsState : StateBase
{
    AcademyController _controller;

    public AcademyResultsState(AcademyController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        AcademyTrainResultsView resultsView = ViewManager.GetView<AcademyTrainResultsView>();
        resultsView.mintModelButtonPressed += OnMintModelButtonPRessed;
    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    void OnMintModelButtonPRessed()
    {
        Debug.Log("Mint model!");
    }


}